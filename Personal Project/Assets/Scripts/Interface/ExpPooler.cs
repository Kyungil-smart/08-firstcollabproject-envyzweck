using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class ExpPooler : MonoBehaviour
{
    public static ExpPooler Instance;

    [System.Serializable]
    public struct GemSetting
    {
        public string gemName;     
        public GameObject prefab;  
        public int expValue;       
    }

    [Header("보석 설정 (큰 단위 순서대로 넣기)")]
    [SerializeField] private List<GemSetting> gemSettings;
    
    private Dictionary<int, IObjectPool<ExpGem>> pools = new Dictionary<int, IObjectPool<ExpGem>>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        foreach (var setting in gemSettings)
        {
            int value = setting.expValue;
            GameObject prefab = setting.prefab;

            pools[value] = new ObjectPool<ExpGem>(
                () => CreateGem(prefab, value),
                gem => gem.gameObject.SetActive(true),
                gem => gem.gameObject.SetActive(false),
                gem => Destroy(gem.gameObject),
                maxSize: 200
            );
        }
    }

    private ExpGem CreateGem(GameObject prefab, int value)
    {
        ExpGem gem = Instantiate(prefab, transform).GetComponent<ExpGem>();
        gem.SetPool(pools[value]);
        return gem;
    }

    public void SpawnExp(Vector3 position, int totalExp)
    {
        int remainingExp = totalExp;

        foreach (var setting in gemSettings)
        {
            if (remainingExp <= 0) break;

            int unitValue = setting.expValue;
            int count = remainingExp / unitValue; 

            for (int i = 0; i < count; i++)
            {
                ExpGem gem = pools[unitValue].Get();
                
                // 생성 위치에 미세한 랜덤 오프셋 추가 (겹침 방지)
                Vector2 randomOffset = Random.insideUnitCircle * 0.2f;
                gem.transform.position = position + new Vector3(randomOffset.x, randomOffset.y, 0);
                
                gem.Init(unitValue);

                // 튕겨나가는 연출
                Rigidbody2D rb = gem.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.simulated = true;
                    rb.linearVelocity = Vector2.zero; 
                    
                    Vector2 randomDir = Random.insideUnitCircle.normalized;
                    float randomForce = Random.Range(3f, 6f);
                    rb.AddForce(randomDir * randomForce, ForceMode2D.Impulse);
                }
            }
            // 남은 값 계산
            remainingExp %= unitValue; 
        }
    }
}