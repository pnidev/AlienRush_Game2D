using UnityEngine;

[CreateAssetMenu(menuName = "Config/AwsConfig")]
public class AwsConfig : ScriptableObject
{
    [Header("Endpoints (fill from DevOps)")]
    public string apiBaseUrl;      // https://p2ieqed516.execute-api.ap-southeast-2.amazonaws.com/prod
    public string websocketUrl;    // wss://j8q0gmlje4.execute-api.ap-southeast-2.amazonaws.com/prod/
    public string hostedLoginUrl;  // full Cognito Hosted UI url (from DevOps)
    public string clientId;        // Cognito client id
    public string redirectUri;    // https://d84l1y8p4kdic.cloudfront.net (as provided)
    public string bucketName;      // S3 bucket name
}
