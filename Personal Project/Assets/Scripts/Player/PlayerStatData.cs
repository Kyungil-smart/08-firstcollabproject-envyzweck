using UnityEngine;

[CreateAssetMenu(fileName = "NewStatData", menuName = "ScriptableObjects/PlayerStatData")]
public class PlayerStatData : ScriptableObject
{
    [Header("시각 정보")]
    public string characterName;
    public Sprite characterIcon; 
    public GameObject characterPrefab; 

    [Header("전투 능력치")]
    public float maxHealth = 100f; 
    public float moveSpeed = 5f;   
    public float armor = 0f;      
    public float magnetRange = 1f; 

    [Header("시작 무기")]
    public string startingWeaponName;
    
}