using UnityEngine;
using UnityEngine.Pool;

public class Weapon : MonoBehaviour
{
    [Header("Data References")]
    [SerializeField] private WeaponData weaponData; 
    private PlayerStatData playerStat;               
    private PlayerController playerController;

    [Header("Targeting Settings")]
    [SerializeField] private LayerMask enemyLayer;       

    private IObjectPool<Projectile> pool;
    private float nextFireTime;
    private Collider2D[] detectionResults = new Collider2D[10];
    
    public void Setup(PlayerStatData stat, PlayerController controller)
    {
        playerStat = stat;
        playerController = controller;
        
        if (pool == null)
        {
            pool = new ObjectPool<Projectile>(
                createFunc: CreateProjectile,
                actionOnGet: (p) => p.OnSpawn(),      
                actionOnRelease: (p) => p.OnDespawn(), 
                actionOnDestroy: (p) => Destroy(p.gameObject), 
                collectionCheck: false, 
                defaultCapacity: 20, 
                maxSize: 100
            );
        }
    }

    private void Update()
    {
        if (playerStat == null || weaponData == null) return;

        // 발사 간격 체크 (FireRate)
        if (Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + weaponData.baseFireRate; 
        }
    }

    private void Fire()
    {
        if (playerController == null) return;
        
        Transform target = GetNearestEnemy();
        
        // 기본적으로 플레이어가 바라보는 방향으로 설정
        Vector2 fireDirection = playerController.CurrentLookDirection;

        // 사거리 안에 적이 있다면 해당 적의 방향으로 발사
        if (target != null)
        {
            fireDirection = (target.position - transform.position).normalized;
        }
        
        float finalDamage = weaponData.baseDamage + playerStat.attackPower; 
        Projectile p = pool.Get();
        
        if (p != null)
        {
            p.transform.position = transform.position;
            // Init 호출 시 방향, 속도, 데미지, 지속시간 전달
            p.Init(fireDirection, weaponData.baseProjectileSpeed, finalDamage, weaponData.baseDuration);
        }
    }

    private Transform GetNearestEnemy()
    {
        if (weaponData == null) return null;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);
        filter.useTriggers = true;
        
        float range = weaponData.baseDetectionRange;
        int count = Physics2D.OverlapCircle(transform.position, range, filter, detectionResults);

        if (count == 0) return null;

        Transform nearest = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            float distance = Vector2.Distance(transform.position, detectionResults[i].transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = detectionResults[i].transform;
            }
        }

        return nearest;
    }
    
    // 에디터에서 범위를 시각적으로 확인하기 위한 Gizmos
    private void OnDrawGizmosSelected()
    {
        if (weaponData == null) return;

        Gizmos.color = Color.red;
        // WeaponData의 범위를 빨간색 원으로 그려줍니다.
        Gizmos.DrawWireSphere(transform.position, weaponData.baseDetectionRange);
    }
    
    private Projectile CreateProjectile()
    {
        if (weaponData.projectilePrefab == null) return null;
        GameObject go = Instantiate(weaponData.projectilePrefab);
        Projectile p = go.GetComponent<Projectile>();
        if (p != null) p.SetPool(pool);
        return p;
    }
}