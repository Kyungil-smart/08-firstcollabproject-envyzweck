using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ExpBarUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Settings")]
    [SerializeField] private float lerpSpeed = 5f;
    
    private float targetValue; 

    void Start()
    {
        StartCoroutine(InitializeUI());
    }

    private IEnumerator InitializeUI()
    {
        while (PlayerStats.Instance == null)
        {
            yield return null;
        }
        
        UpdateUI();
        
        PlayerStats.Instance.onExpChanged.AddListener(OnExpIncreased);
        PlayerStats.Instance.onLevelUp.AddListener(OnLevelUp);
    }
    
    private void OnExpIncreased()
    {
        targetValue = (float)PlayerStats.Instance.currentExp / PlayerStats.Instance.maxExp;
    }
    
    private void OnLevelUp()
    {
        targetValue = (float)PlayerStats.Instance.currentExp / PlayerStats.Instance.maxExp;
        expSlider.value = 0f;
        
        if (levelText != null)
        {
            levelText.text = $"Lv.{PlayerStats.Instance.level}";
        }
    }
    
    void Update()
    {
        if (expSlider == null) return;
        
        // 슬라이더를 부드럽게
        expSlider.value = Mathf.Lerp(expSlider.value, targetValue, Time.deltaTime * lerpSpeed);
    }

    // 강제 동기화
    public void UpdateUI()
    {
        if (PlayerStats.Instance == null) return;
        
        targetValue = (float)PlayerStats.Instance.currentExp / PlayerStats.Instance.maxExp;
        expSlider.value = targetValue;
        
        if (levelText != null)
            levelText.text = $"Lv.{PlayerStats.Instance.level}";
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지를 위해 이벤트 연결 해제
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.onExpChanged.RemoveListener(OnExpIncreased);
            PlayerStats.Instance.onLevelUp.RemoveListener(OnLevelUp);
        }
    }
}