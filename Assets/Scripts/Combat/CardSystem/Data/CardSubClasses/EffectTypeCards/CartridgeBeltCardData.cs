using UnityEngine;

[CreateAssetMenu(fileName = "CardbridgeBeltCard", menuName = "Card System/Cards/Cartbridge Belt")]
public class CardbridgeBeltCardData : CardData
{
    private void OnEnable()
    {
        cardID = "CartbridgeBeltCard";
        type = CardType.Effect;
    }

    public override void ExecuteEffect()
    {

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ActivateEffect();
            Debug.Log($"[CervezaCardData] Carta '{cardID}' ha llamado a PlayerStats.ActivateEffect().");
        }
        else
        {
            Debug.LogError("[CervezaCardData] PlayerStats.Instance es null. ¡No se pudo activar el efecto de carta!");
        }

    }

}