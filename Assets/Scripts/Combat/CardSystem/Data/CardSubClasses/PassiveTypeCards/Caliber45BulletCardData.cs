// Caliber45BulletCardData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Caliber45BulletCardData", menuName = "Card System/Cards/Caliber 45 Bullet")]
public class Caliber45BulletCardData : CardData 
{
    public string bulletCardID = "Caliber45Bullet"; // Para identificarla fácilmente en el código
    private void OnEnable()
    {
        cardID = "Caliber45Bullet";
        type = CardType.Passive;
    }

}