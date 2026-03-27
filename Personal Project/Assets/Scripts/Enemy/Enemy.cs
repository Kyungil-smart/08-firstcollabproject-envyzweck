using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable, IPoolable<Enemy>
{
    [Header("기본 데이터")]
    [SerializeField] private EnemyStatData statData;

    [Header("이펙트 세팅")]
    [SerializeField] private GameObject hitEffectObj; 
    private SpriteRenderer mainSprite; 
    private Coroutine hitEffectCoroutine;

    private float currentHealth;
    private Rigidbody2D rb;
    private Transform player;
    private Transform visualTransform;
    private IObjectPool<Enemy> managedPool;
    private bool isFacingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        if (transform.childCount > 0)
        {
            visualTransform = transform.GetChild(0);
            mainSprite = visualTransform.GetComponent<SpriteRenderer>();
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    public void SetPool(IObjectPool<Enemy> pool) => managedPool = pool;

    public void OnSpawn()
    {
        if (statData != null) currentHealth = statData.maxHealth;
        gameObject.SetActive(true);
        if (rb != null) rb.linearVelocity = Vector2.zero;
        ResetVisuals();
    }

    public void OnDespawn()
    {
        StopAllCoroutines();
        hitEffectCoroutine = null;
        ResetVisuals();
        gameObject.SetActive(false);
    }

    private void ResetVisuals()
    {
        if (hitEffectObj != null) hitEffectObj.SetActive(false);
        if (mainSprite != null) mainSprite.color = Color.white;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (hitEffectCoroutine != null) StopCoroutine(hitEffectCoroutine);
        hitEffectCoroutine = StartCoroutine(PlayHitEffect());
        if (currentHealth <= 0) Die();
    }

    private IEnumerator PlayHitEffect()
    {
        if (hitEffectObj != null) hitEffectObj.SetActive(true);
        if (mainSprite != null) mainSprite.color = new Color(1f, 0.4f, 0.4f);
        yield return new WaitForSeconds(0.1f);
        if (hitEffectObj != null) hitEffectObj.SetActive(false);
        if (mainSprite != null) mainSprite.color = Color.white;
        hitEffectCoroutine = null;
    }

    private void Die()
    {
        // 죽을 때 경험치 보석 생성
        if (ExpPooler.Instance != null && statData != null)
        {
            ExpPooler.Instance.SpawnExp(transform.position, statData.expValue);
        }

        if (managedPool != null) managedPool.Release(this);
        else Destroy(gameObject);
    }

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
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController != null && statData != null)
            {
                playerController.TakeDamage(statData.attackPower * Time.deltaTime);
            }
        }
    }
}