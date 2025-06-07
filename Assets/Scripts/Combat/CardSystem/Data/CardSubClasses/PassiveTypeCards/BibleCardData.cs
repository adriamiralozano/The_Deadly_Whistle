using UnityEngine;

[CreateAssetMenu(fileName = "BibleCard", menuName = "Card System/Cards/Bible")]
public class BibleCardData : CardData
{
    
    private void OnEnable()
    {
        cardID = "BibleCard";
        type = CardType.Passive;
    }

}