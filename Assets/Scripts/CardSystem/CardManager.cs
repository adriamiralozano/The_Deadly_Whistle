// CardManager.cs
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq; // Necesario para .FirstOrDefault() si lo usas en el futuro, aunque no directamente aquí

public class CardManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private DeckData playerStartingDeckData;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI deckCountText;
    [SerializeField] private TextMeshProUGUI discardPileCountText;

    private List<CardData> currentDeck = new List<CardData>();
    private List<CardData> playerHand = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();
    private List<CardData> playedCardsThisTurn = new List<CardData>(); // Para futuras expansiones si las cartas "jugadas" no van al descarte inmediatamente

    private const int MAX_HAND_SIZE = 5;

    public static event Action<int> OnHandCountUpdated;
    public static event Action OnCardDiscarded;
    public static event Action<CardData> OnCardPlayed;


    void OnEnable()
    {
        Debug.Log("[CardManager] OnEnable: Suscribiendo a eventos.");
        TurnManager.OnRequestDrawCard += DrawCard;
        TurnManager.OnRequestHandCount += GetHandCount;
        // TurnManager.OnRequestDiscardCard ahora se suscribe a DiscardFirstCardFromHandIfOverLimit
        TurnManager.OnRequestDiscardCard += DiscardFirstCardFromHandIfOverLimit; // <--- ¡CAMBIO AQUÍ!
        TurnManager.OnRequestPlayFirstCard += PlayFirstCardFromHand;
    }

    void OnDisable()
    {
        Debug.Log("[CardManager] OnDisable: Desuscribiendo de eventos.");
        TurnManager.OnRequestDrawCard -= DrawCard;
        TurnManager.OnRequestHandCount -= GetHandCount;
        // TurnManager.OnRequestDiscardCard ahora se desuscribe de DiscardFirstCardFromHandIfOverLimit
        TurnManager.OnRequestDiscardCard -= DiscardFirstCardFromHandIfOverLimit; // <--- ¡CAMBIO AQUÍ!
        TurnManager.OnRequestPlayFirstCard -= PlayFirstCardFromHand;
    }

    void Start()
    {
        InitializeCombatDeck();
        UpdateDeckCountDisplay();
        UpdateDiscardPileCountDisplay();
    }

    public void InitializeCombatDeck()
    {
        currentDeck.Clear();
        playerHand.Clear();
        discardPile.Clear();
        playedCardsThisTurn.Clear();

        if (playerStartingDeckData == null)
        {
            Debug.LogError("¡ERROR! No se ha asignado un 'Player Starting Deck Data' en el CardManager. Por favor, asigna tu asset 'PlayerStartingDeck' en el Inspector!", this);
            return;
        }

        currentDeck.AddRange(playerStartingDeckData.InitialCards);
        ShuffleDeck();
        Debug.Log($"[CardManager] Mazo de combate inicializado con {currentDeck.Count} cartas y barajado.");
    }

    public void ShuffleDeck()
    {
        System.Random rng = new System.Random();
        int n = currentDeck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            CardData value = currentDeck[k];
            currentDeck[k] = currentDeck[n];
            currentDeck[n] = value;
        }
        Debug.Log("[CardManager] Mazo barajado.");
    }

    public void DrawCard()
    {
        if (currentDeck.Count == 0)
        {
            if (discardPile.Count > 0)
            {
                Debug.Log("[CardManager] Mazo vacío. Barajando cartas de descarte de nuevo al mazo.");
                currentDeck.AddRange(discardPile);
                discardPile.Clear();
                ShuffleDeck();
            }
            else
            {
                Debug.LogWarning("[CardManager] Mazo y pila de descarte vacíos. No se pueden robar más cartas.");
                return;
            }
        }

        CardData drawnCardData = currentDeck[0];
        currentDeck.RemoveAt(0);

        playerHand.Add(drawnCardData);

        UpdateDeckCountDisplay();
        OnHandCountUpdated?.Invoke(playerHand.Count);
        Debug.Log($"[CardManager] Carta robada: {drawnCardData.cardID}. Mazo restante: {currentDeck.Count}. Cartas en mano: {playerHand.Count}.");
    }

    /// <summary>
    /// Mueve una carta de la mano a la pila de descarte.
    /// Este método es el core de la operación de descarte.
    /// </summary>
    /// <param name="cardToDiscard">La carta a descartar.</param>
    
    private void UpdateDiscardPileCountDisplay()
    {
        if (discardPileCountText != null)
        {
            discardPileCountText.text = $"Mazo de descartes: {discardPile.Count}";
        }
    }

    private void DiscardCardInternal(CardData cardToDiscard)
    {
        if (playerHand.Contains(cardToDiscard))
        {
            playerHand.Remove(cardToDiscard);
            discardPile.Add(cardToDiscard);
            Debug.Log($"[CardManager] Carta '{cardToDiscard.cardID}' descartada. Cartas en mano: {playerHand.Count}. Cartas en descarte: {discardPile.Count}.");
            OnHandCountUpdated?.Invoke(playerHand.Count);
            OnCardDiscarded?.Invoke();
            UpdateDiscardPileCountDisplay();
        }
        else
        {
            Debug.LogWarning($"[CardManager] Intento de descartar carta '{cardToDiscard.cardID}' que no está en la mano.");
        }
    }

    /// <summary>
    /// Descarta la primera carta de la mano del jugador, solo si la mano excede el límite.
    /// Este es el método público para el botón "Descartar Carta" o la barra espaciadora en DiscardPhase.
    /// También es llamado por el TurnManager a través del evento OnRequestDiscardCard.
    /// </summary>
    public void DiscardFirstCardFromHandIfOverLimit()
    {
        if (playerHand.Count > MAX_HAND_SIZE)
        {
            if (playerHand.Count > 0)
            {
                CardData cardToDiscard = playerHand[0];
                DiscardCardInternal(cardToDiscard);
            }
            else
            {
                Debug.LogWarning("[CardManager] No hay cartas en mano para descartar.");
            }
        }
        else
        {
            Debug.LogWarning($"[CardManager] Mano en el límite ({playerHand.Count}/{MAX_HAND_SIZE}). No se puede descartar voluntariamente.");
        }
    }

    /// <summary>
    /// Intenta "jugar" la primera carta de la mano.
    /// Por ahora, simplemente la mueve a la pila de descarte (como si se "gastara").
    /// </summary>
    public void PlayFirstCardFromHand()
    {
        if (playerHand.Count > 0)
        {
            CardData playedCard = playerHand[0];
            playerHand.RemoveAt(0);

            discardPile.Add(playedCard);
            Debug.Log($"[CardManager] Carta '{playedCard.cardID}' jugada y movida a descarte. Cartas en mano: {playerHand.Count}.");

            OnHandCountUpdated?.Invoke(playerHand.Count);
            OnCardPlayed?.Invoke(playedCard);
            UpdateDiscardPileCountDisplay();
        }
        else
        {
            Debug.LogWarning("[CardManager] No hay cartas en mano para jugar.");
        }
    }

    public int GetHandCount()
    {
        return playerHand.Count;
    }

    public IReadOnlyList<CardData> GetPlayerHand()
    {
        return playerHand.AsReadOnly();
    }

    private void UpdateDeckCountDisplay()
    {
        if (deckCountText != null)
        {
            deckCountText.text = $"Mazo: {currentDeck.Count}";
        }
    }
}