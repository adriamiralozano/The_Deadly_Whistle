using UnityEngine;

[CreateAssetMenu(fileName = "BagCard", menuName = "Card System/Cards/Bag")]
public class BagCardData : CardData
{
    private void OnEnable()
    {
        cardID = "BagCard";
        type = CardType.Effect;
    }

    public override void ExecuteEffect()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ActivateEffect();
            Debug.Log($"[BagCardData] Carta '{cardID}' ha llamado a PlayerStats.ActivateEffect().");
        }
        else
        {
            Debug.LogError("[BagCardData] PlayerStats.Instance es null. ¡No se pudo activar el efecto de carta!");
        }


        DrawTwoCards();
    }

    private void DrawTwoCards()
    {
        if (CardManager.Instance != null)
        {
            Debug.Log("[BagCard] Ejecutando efecto: robar 2 cartas adicionales (con retardo).");
            CardManager.Instance.StartCoroutine(CardManager.Instance.DrawCards(2, 0.5f));
        }
        else
        {
            Debug.LogWarning("[BagCard] No se encontró CardManager.Instance para robar cartas.");
        }
    }
}