using UnityEngine;

[CreateAssetMenu(fileName = "CoverCard", menuName = "Card System/Cards/Cover")]
public class CoverCardData : CardData
{
    
    private void OnEnable()
    {
        cardID = "CoverCard";
        type = CardType.Passive;
    }

}