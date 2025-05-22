using System.Collections;
using System.Collections.Generic;
// CardData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card System/Card Data")]
public class CardData : ScriptableObject
{
    public string cardID;
    public Sprite artwork; // La imagen 2D de tu carta
}