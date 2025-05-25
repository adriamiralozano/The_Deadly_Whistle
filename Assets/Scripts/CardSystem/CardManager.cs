// CardManager.cs
using UnityEngine;
using System.Collections.Generic;
using TMPro; // Necesario para deckCountText y discardPileCountText
using System;
using System.Linq; // Necesario para .FirstOrDefault() si lo usas en el futuro, aunque no directamente aquí
using UnityEngine.UI; // ¡IMPORTANTE! Necesario para LayoutRebuilder

public class CardManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private DeckData playerStartingDeckData;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI deckCountText;
    [SerializeField] private TextMeshProUGUI discardPileCountText;

    // --- REFERENCIAS DE UI DE CARTA ---
    [Header("Card UI References")]
    [SerializeField] private GameObject cardUIPrefab; // Asigna tu CardUI_Prefab aquí en el Inspector
    [SerializeField] private Transform handContainer; // Asigna tu HandContainer aquí en el Inspector

    private List<CardData> currentDeck = new List<CardData>();
    private List<CardData> playerHand = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();
    private List<CardData> playedCardsThisTurn = new List<CardData>(); // Para futuras expansiones si las cartas "jugadas" no van al descarte inmediatamente

    // Mapea el instanceID de la CardData (string) a su GameObject de UI correspondiente.
    // Esto es clave para manejar múltiples copias de la misma CardData.
    private Dictionary<string, GameObject> handUIInstances = new Dictionary<string, GameObject>(); // <-- ¡CAMBIO AQUÍ! (Tipo de clave)

    private const int MAX_HAND_SIZE = 5;

    public static event Action<int> OnHandCountUpdated;
    public static event Action OnCardDiscarded;
    public static event Action<CardData> OnCardPlayed;


    void OnEnable()
    {
        TurnManager.OnRequestDrawCard += DrawCard;
        TurnManager.OnRequestHandCount += GetHandCount;
        TurnManager.OnRequestDiscardCard += DiscardFirstCardFromHandIfOverLimit;
        TurnManager.OnRequestPlayFirstCard += PlayFirstCardFromHand;
    }

    void OnDisable()
    {
        TurnManager.OnRequestDrawCard -= DrawCard;
        TurnManager.OnRequestHandCount -= GetHandCount;
        TurnManager.OnRequestDiscardCard -= DiscardFirstCardFromHandIfOverLimit;
        TurnManager.OnRequestPlayFirstCard -= PlayFirstCardFromHand;
    }

    void Start()
    {
        InitializeCombatDeck();
        UpdateDeckCountDisplay();
        UpdateDiscardPileCountDisplay();
        UpdateHandVisuals(); // Llama a esto al inicio para asegurar que la mano se muestre (si hay cartas iniciales)
    }

    public void InitializeCombatDeck()
    {
        currentDeck.Clear();
        playerHand.Clear();
        discardPile.Clear();
        playedCardsThisTurn.Clear();

        // Limpia las instancias UI existentes en la mano al inicializar el mazo
        foreach (var uiInstance in handUIInstances.Values)
        {
            Destroy(uiInstance);
        }
        handUIInstances.Clear(); // <-- ¡CAMBIO AQUÍ! También limpia el diccionario.


        if (playerStartingDeckData == null)
        {
            Debug.LogError("¡ERROR! No se ha asignado un 'Player Starting Deck Data' en el CardManager. Por favor, asigna tu asset 'PlayerStartingDeck' en el Inspector!", this);
            return;
        }

        // --- ¡CAMBIO CLAVE AQUÍ! CLONAMOS LAS CARTAS ---
        currentDeck.Clear(); // Asegúrate de que el mazo esté vacío antes de añadir clones
        foreach (CardData card in playerStartingDeckData.InitialCards)
        {
            currentDeck.Add(card.Clone()); // Cada carta en el mazo es ahora una INSTANCIA ÚNICA
        }
        // --- FIN CAMBIO CLAVE ---

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
                UpdateDiscardPileCountDisplay(); // Actualiza display de descarte
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

        // --- Lógica de UI para la carta robada ---
        // Instanciamos el prefab de la carta UI en el contenedor de la mano.
        GameObject cardUI = Instantiate(cardUIPrefab, handContainer);
        CardUI uiScript = cardUI.GetComponent<CardUI>();
        if (uiScript != null)
        {
            uiScript.SetCardData(drawnCardData); // Pasamos los datos para que la instancia de UI los conozca.
            // USAR INSTANCEID COMO CLAVE
            handUIInstances.Add(drawnCardData.instanceID, cardUI); // <-- ¡CAMBIO AQUÍ! Guardamos la referencia por su instanceID.
        }
        else
        {
            Debug.LogError($"[CardManager] El prefab {cardUIPrefab.name} no tiene el script CardUI. Asegúrate de que CardUI.cs está adjunto al prefab de la carta.");
        }
        // --- Fin Lógica de UI ---

        UpdateDeckCountDisplay();
        OnHandCountUpdated?.Invoke(playerHand.Count);
        // Añadido para debuggear el instanceID
        Debug.Log($"[CardManager] Carta robada: {drawnCardData.cardID} (Instance ID: {drawnCardData.instanceID}). Mazo restante: {currentDeck.Count}. Cartas en mano: {playerHand.Count}."); // <-- ¡CAMBIO AQUÍ!
        UpdateHandVisuals(); // Asegura que la mano se visualice correctamente después de robar.
    }

    private void DiscardCardInternal(CardData cardToDiscard)
    {
        if (playerHand.Contains(cardToDiscard))
        {
            playerHand.Remove(cardToDiscard);
            discardPile.Add(cardToDiscard);

            // --- Lógica de UI para la carta descartada ---
            // USAR INSTANCEID COMO CLAVE
            if (handUIInstances.ContainsKey(cardToDiscard.instanceID)) // <-- ¡CAMBIO AQUÍ!
            {
                Destroy(handUIInstances[cardToDiscard.instanceID]); // Destruye la instancia visual de la carta.
                handUIInstances.Remove(cardToDiscard.instanceID); // Remueve la entrada del diccionario.
            }
            // --- Fin Lógica de UI ---

            // Añadido para debuggear el instanceID
            Debug.Log($"[CardManager] Carta '{cardToDiscard.cardID}' (Instance ID: {cardToDiscard.instanceID}) descartada. Cartas en mano: {playerHand.Count}. Cartas en descarte: {discardPile.Count}."); // <-- ¡CAMBIO AQUÍ!
            OnHandCountUpdated?.Invoke(playerHand.Count);
            OnCardDiscarded?.Invoke();
            UpdateDiscardPileCountDisplay(); // Actualiza display de descarte
            UpdateHandVisuals(); // Asegura que la mano se visualice correctamente después de descartar.
        }
        else
        {
            Debug.LogWarning($"[CardManager] Intento de descartar carta '{cardToDiscard.cardID}' que no está en la mano.");
        }
    }

    public void PlayFirstCardFromHand()
    {
        if (playerHand.Count > 0)
        {
            CardData playedCard = playerHand[0];
            playerHand.RemoveAt(0);

            discardPile.Add(playedCard); // Por ahora, las cartas jugadas van al descarte.

            // --- Lógica de UI para la carta jugada ---
            // USAR INSTANCEID COMO CLAVE
            if (handUIInstances.ContainsKey(playedCard.instanceID)) // <-- ¡CAMBIO AQUÍ!
            {
                Destroy(handUIInstances[playedCard.instanceID]); // Destruye la instancia visual de la carta jugada.
                handUIInstances.Remove(playedCard.instanceID); // Remueve la entrada del diccionario.
            }
            // --- Fin Lógica de UI ---

            // Añadido para debuggear el instanceID
            Debug.Log($"[CardManager] Carta '{playedCard.cardID}' (Instance ID: {playedCard.instanceID}) jugada y movida a descarte. Cartas en mano: {playerHand.Count}."); // <-- ¡CAMBIO AQUÍ!

            OnHandCountUpdated?.Invoke(playerHand.Count);
            OnCardPlayed?.Invoke(playedCard);
            UpdateDiscardPileCountDisplay();
            UpdateHandVisuals(); // Asegura que la mano se visualice correctamente después de jugar.
        }
        else
        {
            Debug.LogWarning("[CardManager] No hay cartas en mano para jugar.");
        }
    }

    /// <summary>
    /// Este método sincroniza las representaciones visuales de las cartas en la mano
    /// con la lista 'playerHand'. Asegura que se muestren solo las cartas que realmente tienes.
    /// </summary>
    private void UpdateHandVisuals()
    {
        // Limpiamos handUIInstances y luego la reconstruimos basándonos en playerHand.
        // Esto es menos eficiente para muchos cambios, pero más robusto para este caso
        // y para garantizar la sincronización con cartas repetidas.

        // Destruir todos los elementos UI existentes
        foreach (var uiInstance in handUIInstances.Values)
        {
            Destroy(uiInstance);
        }
        handUIInstances.Clear(); // Limpiar el diccionario de instancias UI

        // Recrear las instancias UI para cada carta en playerHand
        foreach (var cardData in playerHand)
        {
            // No necesitamos comprobar ContainsKey aquí porque acabamos de limpiar el diccionario
            GameObject cardUI = Instantiate(cardUIPrefab, handContainer);
            CardUI uiScript = cardUI.GetComponent<CardUI>();
            if (uiScript != null)
            {
                uiScript.SetCardData(cardData);
                handUIInstances.Add(cardData.instanceID, cardUI); // <-- ¡CAMBIO AQUÍ! Usamos instanceID como clave
            }
            else
            {
                Debug.LogError($"[CardManager] El prefab {cardUIPrefab.name} no tiene el script CardUI.");
            }
        }

        // Forzar al Layout Group a reorganizar los elementos.
        if (handContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(handContainer.GetComponent<RectTransform>());
        }
        else
        {
            Debug.LogError("[CardManager] El HandContainer no está asignado. Asegúrate de arrastrarlo en el Inspector.");
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
                // Aquí, si necesitas un descarte específico (ej. no el primero), necesitarías una UI interactiva
                // para que el jugador elija qué carta descartar. Por ahora, es la primera.
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