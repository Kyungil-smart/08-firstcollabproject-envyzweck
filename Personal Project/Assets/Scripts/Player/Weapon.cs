using UnityEngine;
using UnityEngine.Pool;

public class Weapon : MonoBehaviour
{
    [Header("Data References")]
    [SerializeField] private WeaponData weaponData; 
    private PlayerStatData playerStat;               
    private PlayerController playerController;

    [Header("Targeting Settings")]
    [SerializeField] private float detectionRadius = 5f; 
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
        Vector2 fireDirection = playerController.CurrentLookDirection;

        if (target != null)
        {
            fireDirection = (target.position - transform.position).normalized;
        }
        
        float finalDamage = weaponData.baseDamage + playerStat.attackPower; 
        Projectile p = pool.Get();
        
        if (p != null)
        {
            p.transform.position = transform.position;
            p.Init(fireDirection, weaponData.baseProjectileSpeed, finalDamage, weaponData.baseDuration);
        }
    }

    private Transform GetNearestEnemy()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);
        filter.useTriggers = true;

        //OverlapCircle로 범위안의 콜라이더 찾기
        int count = Physics2D.OverlapCircle(transform.position, detectionRadius, filter, detectionResults);

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
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
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