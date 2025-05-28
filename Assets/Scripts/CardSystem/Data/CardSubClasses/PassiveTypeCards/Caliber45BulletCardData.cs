// Caliber45BulletCardData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewCaliber45BulletCard", menuName = "Card System/Caliber 45 Bullet Card Data")]
public class Caliber45BulletCardData : PassiveCardData //hereda de PassiveCardData
{
    public string bulletCardID = "Caliber45Bullet"; // Para identificarla fácilmente en el código

    // No necesitamos sobrescribir ExecuteEffect() aquí de nuevo,
    // ya que PassiveCardData ya tiene una implementación (aunque sea vacía/de debug).
    // Si la bala tuviera un efecto *específico* al ser "jugada" como pasiva,
    // entonces sí lo sobrescribiríamos aquí.
}