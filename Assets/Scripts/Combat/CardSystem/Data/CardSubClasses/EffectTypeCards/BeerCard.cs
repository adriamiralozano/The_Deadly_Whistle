using UnityEngine;

[CreateAssetMenu(fileName = "CervezaCard", menuName = "Card System/Cards/Cerveza")]
public class CervezaCardData : CardData
{
    private void OnEnable()
    {
        cardID = "BeerCard";
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


        DrinkBeer();
    }

    private void DrinkBeer()
    {
        if (PlayerStats.Instance != null)
        {
            Debug.Log("[CervezaCardData] Ejecutando efecto: curar 1 vida al jugador.");
            PlayerStats.Instance.Heal(1);
        }
        else
        {
            Debug.LogWarning("[CervezaCardData] No se encontró PlayerStats.Instance para curar.");
        }
    }
}