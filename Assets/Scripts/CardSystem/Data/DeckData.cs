// DeckData.cs
using UnityEngine;
using System.Collections.Generic;

// ¡Importante! Esto hace que sea un ScriptableObject y le da un menuName.
[CreateAssetMenu(fileName = "NewDeck", menuName = "Card System/Deck Data")]
public class DeckData : ScriptableObject // <-- Debe heredar de ScriptableObject
{
    [SerializeField]
    private List<CardData> initialCards = new List<CardData>();

    private const int MAX_DECK_SIZE = 20; // Ajustado a 10

    public IReadOnlyList<CardData> InitialCards => initialCards;

    void OnValidate()
    {
        if (initialCards.Count > MAX_DECK_SIZE)
        {
            Debug.LogWarning($"El mazo '{name}' excede el tamaño máximo de {MAX_DECK_SIZE} cartas ({initialCards.Count}). " +
                             "Por favor, reduce la cantidad de cartas en el Inspector.", this);
        }
    }
}