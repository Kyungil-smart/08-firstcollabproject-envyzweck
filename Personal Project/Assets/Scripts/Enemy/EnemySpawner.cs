using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnConfig
    {
        public string enemyName;
        public GameObject enemyPrefab;
        public int initialCapacity = 50;
        public int maxSize = 500;
    }

    [Header("스폰 설정")]
    [SerializeField] private List<EnemySpawnConfig> enemyConfigs;
    [SerializeField] private float spawnInterval = 1.0f; 
    [SerializeField] private float spawnRadius = 15f;    

    private Dictionary<string, IObjectPool<Enemy>> pools;
    // 키 리스트 캐싱
    private List<string> poolKeys; 
    private Transform player;
    private float nextSpawnTime;

    void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        pools = new Dictionary<string, IObjectPool<Enemy>>();
        
        foreach (var config in enemyConfigs)
        {
            if (config.enemyPrefab == null) continue;
            
            GameObject prefab = config.enemyPrefab;
            IObjectPool<Enemy> currentPool = null;

            currentPool = new ObjectPool<Enemy>(
                createFunc: () => {
                    GameObject go = Instantiate(prefab);
                    Enemy e = go.GetComponent<Enemy>();
                    // 자기 자신의 풀을 참조
                    e.SetPool(currentPool); 
                    return e;
                },
                actionOnGet: (e) => e.OnSpawn(),
                actionOnRelease: (e) => e.OnDespawn(),
                actionOnDestroy: (e) => Destroy(e.gameObject),
                collectionCheck: false,
                defaultCapacity: config.initialCapacity,
                maxSize: config.maxSize
            );

            pools.Add(config.enemyName, currentPool);
        }

        // 리스트를 계속 새로 만들지 않도록 미리 키를 저장
        poolKeys = new List<string>(pools.Keys);
        
        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        if (player == null || poolKeys == null || poolKeys.Count == 0) return;

        // 타이머
        if (Time.time >= nextSpawnTime)
        {
            SpawnRandomEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnRandomEnemy()
    {
        if (poolKeys.Count == 0) return;

        string randomKey = poolKeys[Random.Range(0, poolKeys.Count)];
        IObjectPool<Enemy> selectedPool = pools[randomKey];
        
        Vector2 spawnPos = (Vector2)player.position + Random.insideUnitCircle.normalized * spawnRadius;

        Enemy e = selectedPool.Get();
        if (e != null)
        {
            e.transform.position = spawnPos;
        }
    }
}