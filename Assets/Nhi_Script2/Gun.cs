using UnityEngine;

public class Gun : MonoBehaviour
{
    
    [SerializeField] private float rotateOffset = 180f;

  
    [SerializeField] private Transform firePos;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float shotDelay = 0.15f;
    private float nextShot;

    [SerializeField] private AudioSource shootSound;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip reloadClip;


    [SerializeField] private int maxAmmo = 24;
    public int currentAmmo;

   

    void Start()
    {
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        RotateGun();
        Shoot();
        Reload();
    }

    void RotateGun()
    {
        if (Input.mousePosition.x < 0 || Input.mousePosition.x > Screen.width || Input.mousePosition.y < 0 || Input.mousePosition.y > Screen.height)
        {
            return;
        }

        Vector3 displacement = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float angle = Mathf.Atan2(displacement.y, displacement.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotateOffset);
        if (angle < -90 || angle > 90)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, -1, 1);
        }
    }



    private void Shoot()
    {
        if (Input.GetMouseButtonDown(0) && currentAmmo > 0 && Time.time >= nextShot)
        {
            nextShot = Time.time + shotDelay;

            Instantiate(bulletPrefab, firePos.position, firePos.rotation);
           shootSound.PlayOneShot(shootClip);

            currentAmmo--;

        }
    }
    void Reload()
    {
        if (Input.GetKeyDown(KeyCode.R)&& currentAmmo< maxAmmo)
        {
            currentAmmo = maxAmmo;

            // play dedicated reload sound
            if (shootSound != null && reloadClip != null)
                shootSound.PlayOneShot(reloadClip, 1f);
        }

    }
    
}
