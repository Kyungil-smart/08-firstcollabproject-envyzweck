using UnityEngine;

public interface IPoolable<T> where T : class
{
    // 어느 풀에 속해 있는지 설정
    void SetPool(UnityEngine.Pool.IObjectPool<T> pool);
    // 풀에서 꺼내질 때 실행 (초기화)
    void OnSpawn();
    // 풀로 돌아갈 때 실행 (정리)
    void OnDespawn();
}
