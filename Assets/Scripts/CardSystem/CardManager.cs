// CardManager.cs
using UnityEngine;
using System.Collections.Generic; // Para List
using TMPro; // Para TextMeshProUGUI
using System; // Para Action y Func

public class CardManager : MonoBehaviour
{
    // --- Configuración General ---
    [Header("Configuration")]
    [SerializeField] private DeckData playerStartingDeckData;

    // --- UI References (CardManager ahora SOLO necesita actualizar su propio Deck Count) ---
    [SerializeField] private TextMeshProUGUI deckCountText;

    // --- Lógica de las pilas de cartas ---
    private List<CardData> currentDeck = new List<CardData>();
    private List<CardData> playerHand = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();

    // El número máximo de cartas que el jugador puede tener en la mano (para su lógica interna).
    private const int MAX_HAND_SIZE = 5;

    // --- Eventos (para notificar a otros scripts) ---
    // Este evento será disparado por CardManager cuando la mano cambie.
    public static event Action<int> OnHandCountUpdated;
    // Este evento será disparado por CardManager cuando se descarta una carta.
    public static event Action OnCardDiscarded;


    void OnEnable()
    {
        // Suscribirse a los eventos del TurnManager
        TurnManager.OnRequestDrawCard += DrawCard; // TurnManager pide que CardManager robe una carta.
        TurnManager.OnRequestHandCount += GetHandCount; // TurnManager pide el conteo de la mano.
        TurnManager.OnRequestDiscardCard += DiscardRandomCardFromHand; // Temporal: TurnManager pide descartar (para prueba)
    }

    void OnDisable()
    {
        // Desuscribirse para evitar errores cuando los objetos se destruyen.
        TurnManager.OnRequestDrawCard -= DrawCard;
        TurnManager.OnRequestHandCount -= GetHandCount;
        TurnManager.OnRequestDiscardCard -= DiscardRandomCardFromHand;
    }

    void Start()
    {
        InitializeCombatDeck();
        UpdateDeckCountDisplay();
        OnHandCountUpdated?.Invoke(playerHand.Count); // Dispara el evento al inicio para que la UI se actualice.
    }

    /// <summary>
    /// Inicializa el mazo del jugador para el combate usando el DeckData predefinido.
    /// </summary>
    public void InitializeCombatDeck()
    {
        currentDeck.Clear();
        playerHand.Clear();
        discardPile.Clear();

        if (playerStartingDeckData == null)
        {
            Debug.LogError("¡ERROR! No se ha asignado un 'Player Starting Deck Data' en el CardManager. Por favor, asigna tu asset 'PlayerStartingDeck' en el Inspector!", this);
            return;
        }

        currentDeck.AddRange(playerStartingDeckData.InitialCards);
        ShuffleDeck();
        Debug.Log($"[CardManager] Mazo de combate inicializado con {currentDeck.Count} cartas y barajado.");
    }

    /// <summary>
    /// Baraja las cartas en el mazo actual del jugador (Algoritmo Fisher-Yates).
    /// </summary>
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

    /// <summary>
    /// Roba una carta del mazo y la añade a la mano lógica del jugador.
    /// Llamado por TurnManager.
    /// </summary>
    /// <returns>La CardData de la carta robada, o null si no se pudo robar.</returns>
    public void DrawCard()
    {
        // CardManager no necesita preocuparse por el MAX_HAND_SIZE aquí,
        // ya que la lógica de "si la mano está llena, no robes"
        // se manejará a nivel del TurnManager o una capa superior.
        // Aquí solo nos preocupamos por si hay cartas en el mazo.

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
        OnHandCountUpdated?.Invoke(playerHand.Count); // Dispara el evento de actualización de la mano.
        Debug.Log($"[CardManager] Carta robada: {drawnCardData.cardID}. Mazo restante: {currentDeck.Count}. Cartas en mano: {playerHand.Count}.");
    }

    /// <summary>
    /// Descarta una carta específica de la mano lógica a la pila de descarte lógica.
    /// Este será el método real para descartar por interacción del jugador.
    /// </summary>
    /// <param name="cardToDiscard">La CardData de la carta a descartar.</param>
    public void DiscardCard(CardData cardToDiscard)
    {
        if (playerHand.Contains(cardToDiscard))
        {
            playerHand.Remove(cardToDiscard);
            discardPile.Add(cardToDiscard);
            Debug.Log($"[CardManager] Carta '{cardToDiscard.cardID}' descartada. Cartas en mano: {playerHand.Count}. Cartas en descarte: {discardPile.Count}.");
            OnHandCountUpdated?.Invoke(playerHand.Count); // Actualiza la UI de la mano.
            OnCardDiscarded?.Invoke(); // Notifica que una carta ha sido descartada.
        }
        else
        {
            Debug.LogWarning($"[CardManager] Intento de descartar carta '{cardToDiscard.cardID}' que no está en la mano.");
        }
    }

    /// <summary>
    /// TEMPORAL: Descarta la primera carta de la mano para pruebas de la fase de descarte.
    /// Este método será reemplazado por la interacción real del jugador.
    /// Llamado por TurnManager.
    /// </summary>
    public void DiscardRandomCardFromHand()
    {
        if (playerHand.Count > 0)
        {
            CardData cardToDiscard = playerHand[0]; // Descartamos la primera carta
            DiscardCard(cardToDiscard); // Llama al método DiscardCard real
        }
        else
        {
            Debug.LogWarning("[CardManager] No hay cartas en mano para descartar (intento de descarte aleatorio).");
        }
    }


    /// <summary>
    /// Obtiene el conteo actual de cartas en la mano.
    /// Llamado por TurnManager (a través del evento OnRequestHandCount).
    /// </summary>
    /// <returns>El número de cartas en la mano.</returns>
    public int GetHandCount()
    {
        return playerHand.Count;
    }

    /// <summary>
    /// Obtiene una copia de la lista de cartas en la mano (para inspección externa).
    /// </summary>
    public IReadOnlyList<CardData> GetPlayerHand()
    {
        return playerHand.AsReadOnly();
    }

    // --- Métodos de Actualización de UI Interna ---
    private void UpdateDeckCountDisplay()
    {
        if (deckCountText != null)
        {
            deckCountText.text = $"Mazo: {currentDeck.Count}";
        }
    }

    // Quitamos el Update() de aquí ya que TurnManager manejará las entradas
    // void Update()
    // {
    //     // Esto se moverá a TurnManager o a un InputManager.
    // }
}