using UnityEngine;

public interface IPoolable<T> where T : class
{
    void SetPool(UnityEngine.Pool.IObjectPool<T> pool);
    void OnSpawn();
    void OnDespawn();
}
