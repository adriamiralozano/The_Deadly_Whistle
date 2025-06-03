using UnityEngine;

[CreateAssetMenu(fileName = "BagCard", menuName = "Card System/Cards/Bag")]
public class BagCard : CardData
{
    private void OnEnable()
    {
        cardID = "BagCard";
        type = CardType.Effect;
    }

    public override void ExecuteEffect()
    {
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