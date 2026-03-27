using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("실시간 레벨 데이터")]
    public int level = 1;
    public int currentExp = 0;
    public int maxExp = 100;

    [Header("실시간 능력치")]
    public float magnetRange; 

    [Header("이벤트")]
    public UnityEvent onLevelUp;
    public UnityEvent onExpChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddExp(int amount)
    {
        currentExp += amount;
        Debug.Log($"[EXP] +{amount} 획득! 현재: {currentExp}/{maxExp}");

        while (currentExp >= maxExp)
        {
            LevelUp();
        }
        onExpChanged?.Invoke();
    }

    private void LevelUp()
    {
        currentExp -= maxExp;
        level++;
        maxExp = level * 100; 
        Debug.Log($"<color=yellow> 레벨업! 현재 레벨: {level} </color>");
        onLevelUp?.Invoke();
    }
}