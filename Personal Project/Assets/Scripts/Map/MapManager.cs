using UnityEngine;

public partial class MapManager : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private float chunkSize = 20f;   
    [SerializeField] private Transform player;        

    void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // 중심을 기준으로 3x3 (총 9개) 청크 생성
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3 spawnPos = new Vector3(x * chunkSize, y * chunkSize, 0);
                GameObject go = Instantiate(chunkPrefab, spawnPos, Quaternion.identity, transform);
                
                InfiniteChunk chunk = go.GetComponent<InfiniteChunk>();
                if (chunk != null) chunk.Init(player, chunkSize);
            }
        }
    }
    private void OnDrawGizmos()
    {
        // 가이드 라인 색상 (연한 회색)
        Gizmos.color = new Color(1, 1, 1, 0.2f);

        // 예시로 중심점 기준 5x5 범위를 그리드로 표시
        float gridRange = 5f; 
        for (float x = -gridRange; x <= gridRange; x++)
        {
            for (float y = -gridRange; y <= gridRange; y++)
            {
                Vector3 pos = new Vector3(x * chunkSize, y * chunkSize, 0);
                Gizmos.DrawWireCube(pos, new Vector3(chunkSize, chunkSize, 0));
            }
        }
    }
}