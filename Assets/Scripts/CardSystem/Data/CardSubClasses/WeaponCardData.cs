using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponCard", menuName = "Card System/Weapon Card Data")]
public class WeaponCardData : CardData // Hereda de CardData
{
    [Header("Weapon Stats")]
    public int baseDamagePerHit = 1; 
}