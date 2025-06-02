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
            PlayerStats.Instance.Heal(1);
        }
        else
        {
            Debug.LogWarning("[CervezaCardData] No se encontró PlayerStats.Instance para curar.");
        }
    }
}
