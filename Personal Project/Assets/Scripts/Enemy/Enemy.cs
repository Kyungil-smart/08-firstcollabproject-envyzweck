using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable, IPoolable<Enemy>
{
    [Header("기본 데이터")]
    [SerializeField] private EnemyStatData statData;

    [Header("공격세팅")]
    [SerializeField] private float attackCooldown = 1f;
    private float nextAttackTime;

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
        rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete; 
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (transform.childCount > 0)
        {
            visualTransform = transform.GetChild(0);
        }

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
        if (statData != null)
        {
            currentHealth = statData.maxHealth;
        }
        gameObject.SetActive(true);
        
        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        // 스폰 직후 즉시 공격 방지
        nextAttackTime = Time.time + 0.5f; 
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
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (managedPool != null) managedPool.Release(this);
        else Destroy(gameObject); 
    }
    #endregion

    void FixedUpdate()
    {
        if (player == null || statData == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * statData.moveSpeed;

        if (direction.x != 0) Flip(direction.x);
    }

    private void Flip(float dirX)
    {
        if ((dirX > 0 && !isFacingRight) || (dirX < 0 && isFacingRight))
        {
            isFacingRight = !isFacingRight;
            if (visualTransform != null)
            {
                Vector3 scale = visualTransform.localScale;
                scale.x = isFacingRight ? 1f : -1f;
                visualTransform.localScale = scale;
            }
        }
    }
    
    private void OnCollisionStay2D(Collision2D collision) => CheckAndAttack(collision.gameObject);
    private void OnTriggerStay2D(Collider2D collision) => CheckAndAttack(collision.gameObject);

    private void CheckAndAttack(GameObject target)
    {
        if (target.CompareTag("Player") && Time.time >= nextAttackTime)
        {
            PlayerController playerCtrl = target.GetComponent<PlayerController>();
            if (playerCtrl != null && statData != null)
            {
                playerCtrl.TakeDamage(statData.damage);
                
                nextAttackTime = Time.time + attackCooldown;
                Debug.Log($"[Enemy] {statData.enemyName}이(가) 플레이어에게 {statData.damage} 데미지를 입힘!");
            }
        }
    }
}