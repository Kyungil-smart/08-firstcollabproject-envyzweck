using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "ScriptableObjects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public GameObject projectilePrefab;
    
    [Header("무기 기본 스탯")]
    public float baseDamage = 10f;
    public float baseFireRate = 1.0f;
    public float baseProjectileSpeed = 10f;
    public float baseDuration = 2f;
}