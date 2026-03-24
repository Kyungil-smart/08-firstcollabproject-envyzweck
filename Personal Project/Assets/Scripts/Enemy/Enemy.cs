using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable, IPoolable<Enemy>
{
    [Header("Data References")]
    [SerializeField] private EnemyStatData statData;

    private float currentHealth;
    private bool isFacingRight = true;
    
    private Rigidbody2D rb;
    private Transform player;
    private Transform visualTransform;
    private IObjectPool<Enemy> managedPool;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        // 대량 오브젝트 시 Discrete가 성능에 유리
        rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete; 
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // 첫 번째 자식을 스프라이트 제어용으로 사용
        if (transform.childCount > 0)
        {
            visualTransform = transform.GetChild(0);
        }

        // 프리팹에 데이터가 없다면 경고
        if (statData == null)
        {
            Debug.LogError($"{gameObject.name}에 EnemyStatData가 할당되지 않았습니다!");
        }
    }

    #region IPoolable 구현
    public void SetPool(IObjectPool<Enemy> pool)
    {
        managedPool = pool;
    }

    public void OnSpawn()
    {
        // 데이터 기반 체력 초기화
        if (statData != null)
        {
            currentHealth = statData.maxHealth;
        }
        gameObject.SetActive(true);
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void OnDespawn()
    {
        gameObject.SetActive(false);
    }
    #endregion

    #region IDamageable 구현
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 죽을 때 풀로 반납
        if (managedPool != null)
        {
            managedPool.Release(this);
        }
        else
        {
            // 풀이 없는 경우 폭파
            Destroy(gameObject); 
        }
    }
    #endregion

    void FixedUpdate()
    {
        if (player == null || statData == null) return;

        // 플레이어 방향 계산 및 이동
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * statData.moveSpeed;

        // 자식 스프라이트만 좌우 반전 체크
        if (direction.x != 0)
        {
            Flip(direction.x);
        }
    }

    private void Flip(float dirX)
    {
        if ((dirX > 0 && !isFacingRight) || (dirX < 0 && isFacingRight))
        {
            isFacingRight = !isFacingRight;
            
            if (visualTransform != null)
            {
                Vector3 scale = visualTransform.localScale;
                scale.x *= -1;
                visualTransform.localScale = scale;
            }
        }
    }
}