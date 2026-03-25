using UnityEngine;

public class InfiniteChunk : MonoBehaviour
{
    private Transform player;
    private float chunkSize;

    public void Init(Transform playerTransform, float size)
    {
        player = playerTransform;
        chunkSize = size;
    }

    void Update()
    {
        if (player == null) return;

        // 플레이어와 청크 중심 사이의 거리 차이 계산
        float diffX = player.position.x - transform.position.x;
        float diffY = player.position.y - transform.position.y;

        // 청크 크기의 1.5배 이상 거리가 벌어지면 재배치
        if (Mathf.Abs(diffX) > chunkSize * 1.5f || Mathf.Abs(diffY) > chunkSize * 1.5f)
        {
            Reposition(diffX, diffY);
        }
    }

    private void Reposition(float diffX, float diffY)
    {
        // 방향에 따라 청크를 3칸(chunkSize * 3) 옆으로 텔레포트
        float dirX = diffX > 0 ? 1 : -1;
        float dirY = diffY > 0 ? 1 : -1;

        if (Mathf.Abs(diffX) > Mathf.Abs(diffY))
        {
            transform.Translate(Vector3.right * dirX * chunkSize * 3);
        }
        else
        {
            transform.Translate(Vector3.up * dirY * chunkSize * 3);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(chunkSize, chunkSize, 0));
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(chunkSize * 1.5f * 2, chunkSize * 1.5f * 2, 0));
        
    }
}