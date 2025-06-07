using UnityEngine;

[CreateAssetMenu(fileName = "NewRevolverCard", menuName = "Card System/Cards/Revolver")]
public class RevolverCardData : CardData
{
    [Header("Revolver Stats")]
    public int baseDamagePerHit = 1; 
    
    private void OnEnable()
    {
        cardID = "RevolverCard";
        type = CardType.Weapon;
    }

}