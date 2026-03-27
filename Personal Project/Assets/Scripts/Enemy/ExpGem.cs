using UnityEngine;
using UnityEngine.Pool;

public class ExpGem : MonoBehaviour
{
    private IObjectPool<ExpGem> managedPool;
    private int currentExpValue;
    
    [Header("자석 설정")]
    [SerializeField] private float followSpeed = 12f;
    
    private bool isFollowing = false;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetPool(IObjectPool<ExpGem> pool) => managedPool = pool;

    public void Init(int value)
    {
        currentExpValue = value;
        isFollowing = false;
        if (rb != null) { rb.simulated = true; rb.linearVelocity = Vector2.zero; }
    }

    void Update()
    {
        if (PlayerStats.Instance == null) return;

        float distance = Vector2.Distance(transform.position, PlayerStats.Instance.transform.position);

        // 자석 범위 가져오기
        if (!isFollowing && distance <= PlayerStats.Instance.magnetRange)
        {
            isFollowing = true;
            if (rb != null) rb.simulated = false; 
        }

        if (isFollowing)
        {
            transform.position = Vector2.MoveTowards(transform.position, PlayerStats.Instance.transform.position, followSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 경험치 추가 하기
            PlayerStats.Instance.AddExp(currentExpValue);
            
            if (rb != null) rb.simulated = true; 
            managedPool.Release(this);
        }
    }
}