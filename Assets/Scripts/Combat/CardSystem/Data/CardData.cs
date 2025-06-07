using UnityEngine;
using System; // Necesario para Guid

public enum CardType
{
    None,
    Weapon,
    Effect,
    Passive
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Card System/Card Data")]
public class CardData : ScriptableObject
{

    public Sprite artwork; // La imagen 2D de tu carta
    public CardType type;

    [NonSerialized] public string cardID;

    [NonSerialized] public string instanceID;

    public CardData Clone()
    {
        CardData clone = Instantiate(this);
        clone.instanceID = Guid.NewGuid().ToString();
        return clone;
    }

    public virtual void ExecuteEffect()
    {
        Debug.Log($"[CardData] Executing base effect for: {cardID} (Type: {type})");
    }
}