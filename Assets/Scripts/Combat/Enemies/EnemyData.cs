using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Base Stats")]
    public string enemyName = "New Enemy";
    public GameObject enemyPrefab;

    [Tooltip("Número de corazones/vidas del enemigo.")]
    public int maxHealth = 5; 
    
}