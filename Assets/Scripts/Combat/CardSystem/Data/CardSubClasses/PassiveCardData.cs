using UnityEngine;

//Puedes decidir si es abstracta si nunca esperas crear una "PassiveCardData" genérica
[CreateAssetMenu(fileName = "NewPassiveCard", menuName = "Card System/Passive Card Data")]
public class PassiveCardData : CardData // Hereda de CardData
{
    // Las cartas pasivas generalmente no tienen un efecto activo al ser "jugadas" en el DropTarget.
    // Su lógica se activa por otras condiciones o por otras cartas que las consumen.
    // Podrías añadir propiedades comunes a todas las pasivas aquí, si las tuvieran.
    // Por ejemplo:
    // public bool isConsumable = true;
    // public float statBonus = 0f;

/*     public override void ExecuteEffect()
    {
        Debug.Log($"[PassiveCardData] Executing base passive effect for: {cardID}. Passive cards typically don't have an active played effect.");
        // Este método podría ser vacío, o simplemente un log, ya que las pasivas
        // rara vez se "juegan" directamente con un efecto, sino que actúan como recursos o modificadores.
    } */
}