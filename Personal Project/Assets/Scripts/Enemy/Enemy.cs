using UnityEngine;
using UnityEngine.Pool;
using System.Collections; // 코루틴 사용을 위해 필수

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable, IPoolable<Enemy>
{
    [Header("기본 데이터")]
    [SerializeField] private EnemyStatData statData;

    [Header("이펙트 세팅")]
    [SerializeField] private GameObject hitEffectObj; // 프리팹 내 HitEffect 자식 오브젝트
    [SerializeField] private float effectDuration = 0.2f; // 이펙트 지속 시간
    private SpriteRenderer mainSprite; // 본체 빨간색 깜빡임용
    private Coroutine hitEffectCoroutine; // 중복 실행 방지용

    [Header("공격세팅")]
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
            // 비주얼 오브젝트에서 SpriteRenderer를 가져옵니다.
            mainSprite = visualTransform.GetComponent<SpriteRenderer>();
        }

        if (statData == null)
            Debug.LogError($"{gameObject.name}에 EnemyStatData가 할당되지 않았습니다!");

        // 시작 시 이펙트는 꺼둡니다.
        if (hitEffectObj != null) hitEffectObj.SetActive(false);
    }

    #region IPoolable 구현
    public void SetPool(IObjectPool<Enemy> pool) => managedPool = pool;

    public void OnSpawn()
    {
        if (statData != null) currentHealth = statData.maxHealth;
        gameObject.SetActive(true);
        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        nextAttackTime = Time.time + 0.5f; 

        // ⚠️ 스폰 시 비주얼 상태 초기화
        ResetVisuals();
    }

    public void OnDespawn()
    {
        // ⚠️ 풀로 돌아가기 전 모든 코루틴 중지 및 리셋
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
    #endregion

    #region IDamageable 구현
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // 피격 이펙트 실행 (이미 실행 중이면 멈추고 새로 시작)
        if (hitEffectCoroutine != null) StopCoroutine(hitEffectCoroutine);
        hitEffectCoroutine = StartCoroutine(PlayHitEffect());

        if (currentHealth <= 0) Die();
    }

    private IEnumerator PlayHitEffect()
    {
        // 1. 피격 상태 (이펙트 ON + 본체 빨간색)
        if (hitEffectObj != null) hitEffectObj.SetActive(true);
        if (mainSprite != null) mainSprite.color = new Color(1f, 0.4f, 0.4f); // 밝은 빨강

        yield return new WaitForSeconds(effectDuration);

        // 2. 원래 상태로 복구
        if (hitEffectObj != null) hitEffectObj.SetActive(false);
        if (mainSprite != null) mainSprite.color = Color.white;

        hitEffectCoroutine = null;
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
                nextAttackTime = Time.time + statData.attackCooldown;
            }
        }
    }
}