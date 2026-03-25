using UnityEngine;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    public Transform player;
    public GameObject currentStage;
    public float activationDistance = 35f;
    
    private List<Chunk> chunkList = new List<Chunk>(); // 모든 청크 리스트
    private float sqrActivationDist;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        sqrActivationDist = activationDistance * activationDistance;

        if (currentStage != null)
        {
            // 자식 청크들을 모두 찾아 리스트에 넣습니다. (꺼져있는 애들도 포함)
            Chunk[] chunks = currentStage.GetComponentsInChildren<Chunk>(true);
            chunkList.AddRange(chunks);
        }
    }

    void Update()
    {
        if (player == null || chunkList.Count == 0) return;

        // 매 프레임 모든 청크의 거리를 매니저가 대신 계산합니다.
        foreach (Chunk chunk in chunkList)
        {
            float sqrDist = (player.position - chunk.transform.position).sqrMagnitude;
            
            // 거리에 따라 활성화/비활성화 결정
            chunk.SetAlive(sqrDist <= sqrActivationDist);
        }
    }
}