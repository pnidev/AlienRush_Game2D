using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class BaseEnemyController : MonoBehaviour
{
    [Header("General")]
    public float detectionRadius = 8f;
    public float desiredDistance = 4f;
    public float moveSpeed = 3f;
    [HideInInspector] public Transform player;

    [Header("Debug")]
    public bool debugMovement = true;
    public bool forceDetect = false; // bật để giả lập đã detect
    public bool forceMoveTowards = false; // bật để ép move test

    protected Rigidbody2D rb;
    protected Animator animator;
    protected Vector2 velocity;
    protected bool isPlayerDetected = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;
    }


    void Update()
    {
        // debug draw
        if (debugMovement && player != null) Debug.DrawLine(transform.position, player.position, Color.red);

        // debug forceDetect: dùng để test nếu bạn muốn xem enemy chạy khi bị "giả detect"
        if (forceDetect && !isPlayerDetected)
        {
            isPlayerDetected = true;

            OnPlayerDetected(Vector2.Distance(transform.position, player.position));
        }

        if (player == null)
        {
            if (isPlayerDetected) { isPlayerDetected = false; OnPlayerLost(); }
            UpdateAnimator();
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= detectionRadius)
        {
            if (!isPlayerDetected)
            {
                isPlayerDetected = true;

            }
            OnPlayerDetected(dist);
        }
        else
        {
            if (isPlayerDetected)
            {
                isPlayerDetected = false;

                OnPlayerLost();
            }
        }

        // debug forced move
        if (forceMoveTowards && player != null)
        {
            float dir = Mathf.Sign(player.position.x - transform.position.x);
            velocity = new Vector2(dir * moveSpeed, 0f);
        }

        UpdateAnimator();

    }
    
        void FixedUpdate()
    {
        if (debugMovement)
        {
            Debug.Log($"{name} FIXED: requestedVel={velocity} rb.vel={rb.velocity} pos={rb.position} parent={transform.parent?.name}");
        }
        rb.velocity = velocity;

    }
    
    protected virtual void OnPlayerDetected(float dist)
    {
        FacePlayer();
        MoveToMaintainDistance(desiredDistance);
       
    }

    protected virtual void OnPlayerLost()
    {
        velocity = Vector2.zero;
    }

    protected void UpdateAnimator()
    {
        if (animator == null) return;
        bool isMovingAnim = isPlayerDetected;
        animator.SetBool("IsMoving", isMovingAnim);
    }

    protected void FacePlayer()
    {
        if (player == null) return;
        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) < 0.05f) return;
        bool shouldFaceRight = dx > 0f;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipX = !shouldFaceRight;
    }

    //protected void MoveToMaintainDistance(float desired)
    //{
    //    if (player == null) return;
    //    float dx = player.position.x - transform.position.x;
    //    float absdx = Mathf.Abs(dx);
    //    float sign = Mathf.Sign(dx);
    //    float tol = 0.2f;
    //    if (absdx > desired + tol) velocity = new Vector2(sign * moveSpeed, 0f);
    //    else if (absdx < desired - tol) velocity = new Vector2(-sign * moveSpeed, 0f);
    //    else velocity = Vector2.zero;
    //}

    protected void MoveToMaintainDistance(float desired)
    {
        if (player == null) return;

        Vector2 delta = (player.position - transform.position);
        float dist = delta.magnitude;
        float tol = 0.2f;

        if (dist > desired + tol)
        {
            // tiến về player
            Vector2 dir = delta.normalized;
            velocity = dir * moveSpeed;
        }
        else if (dist < desired - tol)
        {
            // lùi ra (tránh quá sát)
            Vector2 dir = -delta.normalized;
            velocity = dir * moveSpeed;
        }
        else
        {
            velocity = Vector2.zero;
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, desiredDistance);
    }
}
