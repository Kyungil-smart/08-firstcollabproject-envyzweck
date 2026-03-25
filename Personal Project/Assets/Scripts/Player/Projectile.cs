using UnityEngine;
using UnityEngine.Pool;

public class Projectile : MonoBehaviour, IPoolable<Projectile>
{
    private float speed;
    private float damage;
    private float duration;
    private float timer;
    
    private IObjectPool<Projectile> managedPool;

    // 인터페이스
    public void SetPool(IObjectPool<Projectile> pool) => managedPool = pool;

    // 풀에서 생성시 활성화
    public void OnSpawn()
    {
        timer = 0;
        gameObject.SetActive(true);
    }

    // 풀로 들어갈때 비활성화
    public void OnDespawn()
    {
        gameObject.SetActive(false);
    }

    public void Init(Vector2 direction, float spd, float dmg, float dur)
    {
        speed = spd;
        damage = dmg;
        duration = dur;
        transform.right = direction;
        
    }

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            // 시간 만료 시 반납
            managedPool.Release(this); 
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(damage);
            // 적중 시 반납
            managedPool.Release(this); 
        }
    }
}