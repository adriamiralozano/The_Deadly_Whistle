// CardManager.cs
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private DeckData playerStartingDeckData;

    [Header("UI References")]
    // CORRECCIÓN: TextMeshProUGUI (la 'I' al final, no 'U')
    [SerializeField] private TextMeshProUGUI deckCountText; 
    [SerializeField] private TextMeshProUGUI discardPileCountText;

    // --- REFERENCIAS DE UI DE CARTA ---
    [Header("Card UI References")]
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private Transform handContainer;

    private List<CardData> currentDeck = new List<CardData>();
    private List<CardData> playerHand = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();
    private List<CardData> playedCardsThisTurn = new List<CardData>();

    // Mapea el instanceID de la CardData (string) a su GameObject de UI correspondiente.
    private Dictionary<string, GameObject> handUIInstances = new Dictionary<string, GameObject>();

    private const int MAX_HAND_SIZE = 5;

    public static event Action<int> OnHandCountUpdated;
    public static event Action OnCardDiscarded;
    public static event Action<CardData> OnCardPlayed;

    // Singleton instance
    public static CardManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void OnEnable()
    {
        TurnManager.OnRequestDrawCard += DrawCard;
        TurnManager.OnRequestHandCount += GetHandCount;
        //TurnManager.OnRequestPlayFirstCard += PlayFirstCardFromHand;
    }

    void OnDisable()
    {
        TurnManager.OnRequestDrawCard -= DrawCard;
        TurnManager.OnRequestHandCount -= GetHandCount;
        //TurnManager.OnRequestPlayFirstCard -= PlayFirstCardFromHand;
    }

    void Start()
    {
        InitializeCombatDeck();
        UpdateDeckCountDisplay();
        UpdateDiscardPileCountDisplay();
        UpdateHandVisuals();
    }

    public void InitializeCombatDeck()
    {
        currentDeck.Clear();
        playerHand.Clear();
        discardPile.Clear();
        playedCardsThisTurn.Clear();

        foreach (var uiInstance in handUIInstances.Values)
        {
            Destroy(uiInstance);
        }
        handUIInstances.Clear();

        if (playerStartingDeckData == null)
        {
            Debug.LogError("¡ERROR! No se ha asignado un 'Player Starting Deck Data' en el CardManager. Por favor, asigna tu asset 'PlayerStartingDeck' en el Inspector!", this);
            return;
        }

        currentDeck.Clear();
        foreach (CardData card in playerStartingDeckData.InitialCards)
        {
            currentDeck.Add(card.Clone());
        }

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
                UpdateDiscardPileCountDisplay();
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

        GameObject cardUI = Instantiate(cardUIPrefab, handContainer);
        CardUI uiScript = cardUI.GetComponent<CardUI>();
        if (uiScript != null)
        {
            uiScript.SetCardData(drawnCardData);
            handUIInstances.Add(drawnCardData.instanceID, cardUI);
        }
        else
        {
            Debug.LogError($"[CardManager] El prefab {cardUIPrefab.name} no tiene el script CardUI. Asegúrate de que CardUI.cs está adjunto al prefab de la carta.");
        }

        UpdateDeckCountDisplay();
        OnHandCountUpdated?.Invoke(playerHand.Count);
        Debug.Log($"[CardManager] Carta robada: {drawnCardData.cardID} (Instance ID: {drawnCardData.instanceID}). Mazo restante: {currentDeck.Count}. Cartas en mano: {playerHand.Count}.");
        UpdateHandVisuals();
    }

    public void DiscardCardInternal(CardData cardToDiscard)
    {
        if (playerHand.Contains(cardToDiscard))
        {
            playerHand.Remove(cardToDiscard);
            discardPile.Add(cardToDiscard);

            if (handUIInstances.ContainsKey(cardToDiscard.instanceID))
            {
                Destroy(handUIInstances[cardToDiscard.instanceID]);
                handUIInstances.Remove(cardToDiscard.instanceID);
            }
            else
            {
                Debug.LogWarning($"[CardManager] No se encontró la instancia UI para la carta '{cardToDiscard.cardID}' (Instance ID: {cardToDiscard.instanceID}) en handUIInstances al descartar. Esto puede indicar un problema de sincronización.");
            }

            Debug.Log($"[CardManager] Carta '{cardToDiscard.cardID}' (Instance ID: {cardToDiscard.instanceID}) descartada. Cartas en mano: {playerHand.Count}. Cartas en descarte: {discardPile.Count}.");
            OnHandCountUpdated?.Invoke(playerHand.Count);
            OnCardDiscarded?.Invoke();
            UpdateDiscardPileCountDisplay();
            UpdateHandVisuals();
        }
        else
        {
            Debug.LogWarning($"[CardManager] Intento de descartar carta '{cardToDiscard.cardID}' que no está en la mano. Descarte no válido.");
        }
    }

/*     public void PlayFirstCardFromHand()
    {
        if (playerHand.Count > 0)
        {
            CardData playedCard = playerHand[0];
            playerHand.RemoveAt(0);

            discardPile.Add(playedCard);

            if (handUIInstances.ContainsKey(playedCard.instanceID))
            {
                Destroy(handUIInstances[playedCard.instanceID]);
                handUIInstances.Remove(playedCard.instanceID);
            }

            Debug.Log($"[CardManager] Carta '{playedCard.cardID}' (Instance ID: {playedCard.instanceID}) jugada y movida a descarte. Cartas en mano: {playerHand.Count}.");

            OnHandCountUpdated?.Invoke(playerHand.Count);
            OnCardPlayed?.Invoke(playedCard);
            UpdateDiscardPileCountDisplay();
            UpdateHandVisuals();
        }
        else
        {
            Debug.LogWarning("[CardManager] No hay cartas en mano para jugar.");
        }
    } */

    public bool PlayCard(CardData cardToPlay)
    {
        if (!playerHand.Contains(cardToPlay))
        {
            Debug.LogWarning($"[CardManager] Intentando jugar carta '{cardToPlay.cardID}' (Instance ID: {cardToPlay.instanceID}), pero no está en la mano del jugador.");
            return false;
        }

        playerHand.Remove(cardToPlay);
        discardPile.Add(cardToPlay);

        if (handUIInstances.ContainsKey(cardToPlay.instanceID))
        {
            GameObject uiInstance = handUIInstances[cardToPlay.instanceID];
            handUIInstances.Remove(cardToPlay.instanceID);
            Destroy(uiInstance); 
            Debug.Log($"[CardManager] Instancia UI de '{cardToPlay.cardID}' destruida (carta jugada y movida a descarte lógico).");
        }
        else
        {
            Debug.LogWarning($"[CardManager] No se encontró la instancia UI para la carta '{cardToPlay.cardID}' (Instance ID: {cardToPlay.instanceID}) en handUIInstances al jugar. Esto puede indicar un problema de sincronización.");
        }

        OnHandCountUpdated?.Invoke(playerHand.Count);
        OnCardPlayed?.Invoke(cardToPlay);
        UpdateDiscardPileCountDisplay();
        UpdateHandVisuals();

        Debug.Log($"[CardManager] Carta '{cardToPlay.cardID}' (Instance ID: {cardToPlay.instanceID}) JUGADA y movida a descarte. Cartas restantes en mano: {playerHand.Count}. Cartas en descarte: {discardPile.Count}.");
        return true; 
    }

    /// <summary>
    /// Intenta descartar una carta específica de la mano del jugador.
    /// Aplica las reglas de la Fase de Descarte y el límite de mano.
    /// Esta es la función centralizada para el botón de descarte y el DropTarget.
    /// </summary>

    /// <returns>True si la carta fue descartada exitosamente, false en caso contrario.</returns>
    public bool AttemptManualDiscard(CardData cardToDiscard) // <--- ¡Asegúrate de que este método está en tu archivo!
    {
        // 1. Asegurarse de que la carta esté en la mano
        if (!playerHand.Contains(cardToDiscard))
        {
            Debug.LogWarning($"[CardManager] Falló el descarte: La carta '{cardToDiscard.cardID}' (Instance ID: {cardToDiscard.instanceID}) no se encuentra en la mano del jugador.");
            return false;
        }

        // 2. Comprobar que estemos en la Fase de Descarte
        if (TurnManager.Instance == null)
        {
            Debug.LogError("[CardManager] TurnManager.Instance no encontrado. No se puede verificar la fase para el descarte.");
            return false;
        }

        if (TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.ActionPhase)
        {
            Debug.LogWarning($"[CardManager] Falló el descarte: Solo se puede descartar en la Fase de Acción. Fase actual: {TurnManager.Instance.CurrentPhase}.");
            return false;
        }

        // 3. Comprobar que la mano supere el límite de cartas
        if (playerHand.Count <= MAX_HAND_SIZE)
        {
            Debug.LogWarning($"[CardManager] Falló el descarte: La mano ({playerHand.Count} cartas) no supera el límite de {MAX_HAND_SIZE} cartas.");
            return false;
        }

        // Si todas las condiciones se cumplen, proceder con el descarte interno
        Debug.Log($"[CardManager] Intentando descartar carta '{cardToDiscard.cardID}' (Instance ID: {cardToDiscard.instanceID}). Mano actual antes: {playerHand.Count}.");
        
        // Llamada a la función interna que hace el trabajo real de mover la carta y destruir la UI
        DiscardCardInternal(cardToDiscard); 
        return true;
    }


    private void UpdateHandVisuals()
    {
        foreach (var uiInstance in handUIInstances.Values)
        {
            Destroy(uiInstance);
        }
        handUIInstances.Clear();

        foreach (var cardData in playerHand)
        {
            GameObject cardUI = Instantiate(cardUIPrefab, handContainer);
            CardUI uiScript = cardUI.GetComponent<CardUI>();
            if (uiScript != null)
            {
                uiScript.SetCardData(cardData);
                handUIInstances.Add(cardData.instanceID, cardUI);
            }
            else
            {
                Debug.LogError($"[CardManager] El prefab {cardUIPrefab.name} no tiene el script CardUI.");
            }
        }

        if (handContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(handContainer.GetComponent<RectTransform>());
        }
        else
        {
            Debug.LogError("[CardManager] El HandContainer no está asignado. Asegúrate de arrastrarlo en el Inspector.");
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

    private void UpdateDiscardPileCountDisplay()
    {
        if (discardPileCountText != null)
        {
            discardPileCountText.text = $"Descarte: {discardPile.Count}";
        }
    }
}