using UnityEngine;

public class Chunk : MonoBehaviour
{
    // 스스로 Update를 돌리지 않습니다. (비활성화되면 어차피 안 돌아가니까요)
    
    public void SetAlive(bool isActive)
    {
        // 매니저가 이 함수를 호출해서 껐다 켰다 해줄 겁니다.
        if (gameObject.activeSelf != isActive)
        {
            gameObject.SetActive(isActive);
        }
    }

    // 기즈모는 에디터에서 배치 확인용으로만 남겨둡니다.
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireCube(transform.position, new Vector3(20, 20, 0));
    }
}