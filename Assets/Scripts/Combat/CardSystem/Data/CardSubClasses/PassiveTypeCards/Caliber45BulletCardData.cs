// Caliber45BulletCardData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Caliber45BulletCardData", menuName = "Card System/Cards/Caliber 45 Bullet")]
public class Caliber45BulletCardData : CardData
{
    private void OnEnable()
    {
        cardID = "Caliber45Bullet";
        type = CardType.Passive;
    }

}