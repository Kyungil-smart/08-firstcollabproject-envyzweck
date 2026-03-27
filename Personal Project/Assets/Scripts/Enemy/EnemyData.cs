using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "ScriptableObjects/EnemyData")]
public class EnemyStatData : ScriptableObject
{
    public string enemyName;
    public float maxHealth = 20f;
    public float moveSpeed = 3f;
    public float attackPower = 5f;
    public float attackCooldown = 1f;
    public int expValue = 10;      
}