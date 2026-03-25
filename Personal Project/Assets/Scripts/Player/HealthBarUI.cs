using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

    private Transform target;

    void LateUpdate()
    {
        // 위치 오프셋 갱신
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    public void Initialize(Transform targetTransform, float maxHealth)
    {
        target = targetTransform;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    public void UpdateHealth(float currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }
}