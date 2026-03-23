using UnityEngine;
using UnityEngine.Pool;

public class Weapon : MonoBehaviour
{
    [Header("Data References")]
    [SerializeField] private WeaponData weaponData; 
    private PlayerStatData playerStat;               

    private IObjectPool<Projectile> pool;
    private float nextFireTime;

    // PlayerController에서 호출하여 초기화
    public void Setup(PlayerStatData stat)
    {
        playerStat = stat;
        
        if (pool == null)
        {
            pool = new ObjectPool<Projectile>(
                createFunc: CreateProjectile,
                actionOnGet: OnGetFromPool,      
                actionOnRelease: OnReleaseToPool, 
                actionOnDestroy: OnDestroyObject, 
                collectionCheck: true, 
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
        float finalDamage = weaponData.baseDamage + playerStat.attackPower; 

        // 풀에서 투사체 가져오기 (OnGetFromPool이 자동 실행)
        Projectile p = pool.Get();
        
        if (p != null)
        {
            p.transform.position = transform.position;
            // 투사체 초기화 로직 실행
            p.Init(Vector2.right, weaponData.baseProjectileSpeed, finalDamage, weaponData.baseDuration);
        }
    }

    #region Object Pooling Callbacks (Interface 기반)

    private Projectile CreateProjectile()
    {
        if (weaponData.projectilePrefab == null)
        {
            Debug.LogError($"{weaponData.weaponName}에 투사체 프리팹이 없습니다!");
            return null;
        }

        GameObject go = Instantiate(weaponData.projectilePrefab);
        Projectile p = go.GetComponent<Projectile>();
        
        if (p != null)
        {
            // IPoolable 규칙에 따라 소속 풀을 알려줌
            p.SetPool(pool);
        }
        return p;
    }
    
    private void OnGetFromPool(Projectile p)
    {
        p.OnSpawn();
    }
    
    private void OnReleaseToPool(Projectile p)
    {
        p.OnDespawn();
    }

    private void OnDestroyObject(Projectile p)
    {
        Destroy(p.gameObject);
    }

    #endregion
}