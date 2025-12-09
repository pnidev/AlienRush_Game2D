using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

public class AlienReceiver : MonoBehaviour
{
    TcpListener listener;
    Thread listenerThread;
    Texture2D texture;

    void Start()
    {
        listenerThread = new Thread(ListenForClients);
        listenerThread.IsBackground = true;
        listenerThread.Start();
    }

    void ListenForClients()
    {
        listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 9999);
        listener.Start();

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            NetworkStream stream = client.GetStream();

            while (client.Connected)
            {
                byte[] sizeBytes = new byte[4];
                int bytesRead = stream.Read(sizeBytes, 0, 4);
                if (bytesRead == 0) break;

                int size = System.Net.IPAddress.NetworkToHostOrder(
                    System.BitConverter.ToInt32(sizeBytes, 0)
                );

                byte[] data = new byte[size];
                int totalRead = 0;
                while (totalRead < size)
                {
                    int read = stream.Read(data, totalRead, size - totalRead);
                    if (read == 0) break;
                    totalRead += read;
                }

                // Convert byte[] → Texture2D
                if (texture == null) texture = new Texture2D(2, 2);
                texture.LoadImage(data);

                // Apply texture lên Quad / Plane trong Unity
                GetComponent<Renderer>().material.mainTexture = texture;
            }
        }
    }
}