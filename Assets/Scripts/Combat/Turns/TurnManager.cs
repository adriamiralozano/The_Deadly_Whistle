// TurnManager.cs
using UnityEngine;
using System; // Para Action y Func
using TMPro; // Para TextMeshProUGUI
using System.Collections; // Para Coroutines

public class TurnManager : MonoBehaviour
{
    // --- Referencias UI ---
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI turnPhaseText; // Para el texto de fase de turno
    [SerializeField] private TextMeshProUGUI handCountText; // Para el conteo de la mano

    // --- Singleton ---
    public static TurnManager Instance { get; private set; }


    // --- Enumeración de Fases de Turno ---
    public enum TurnPhase
    {
        None,           // Estado inicial o entre turnos
        DrawPhase,      // Fase de robo: robar una carta.
        ActionPhase,    // Fase de acción: jugar cartas.
        EndTurn         // Fase final del turno: limpieza y preparación para el siguiente.
    }

    // --- Variables de Estado del Turno ---
    public int currentTurnNumber { get; private set; } = 0;
    private const int MAX_HAND_SIZE = 5;

    // --- Inicialización del Singleton ---
    private TurnPhase currentTurnPhase = TurnPhase.None;
    // Asegura que solo haya una instancia de TurnManager en la escena.
    public TurnPhase CurrentPhase { get { return currentTurnPhase; } }

    // Para optimizar actualizaciones de UI
    private string lastTurnPhaseText = "";
    private string lastHandCountText = "";

    // --- Eventos de Turno ---
    public static event Action<int> OnTurnStart;        // Se dispara al inicio de un nuevo turno
    public static event Action<TurnPhase> OnPhaseChange; // Se dispara cada vez que la fase de turno cambia
    public static event Action OnPlayerTurnEnded;      // Se dispara cuando el turno del jugador termina (antes de iniciar el siguiente)

    // --- Eventos para comunicación con CardManager ---
    public static event Action OnRequestDrawCard;        // Solicita al CardManager que robe una carta
    public static event Func<int> OnRequestHandCount;    // Solicita al CardManager el conteo de la mano



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

    // --- Suscripción/Desuscripción a Eventos ---
    void OnEnable()
    {
        // Suscripción al evento de CardManager para actualizar la UI del conteo de mano
        CardManager.OnHandCountUpdated += UpdateHandCountDisplay;
    }

    void OnDisable()
    {
        // Desuscripción para evitar errores cuando el objeto se desactiva/destruye
        CardManager.OnHandCountUpdated -= UpdateHandCountDisplay;
    }

    // --- Métodos de Inicio ---
    void Start()
    {
        StartGame();
    }


    /// Inicia el juego y el primer turno del jugador.
    public void StartGame()
    {
        currentTurnNumber = 0; // Resetea el contador de turnos al inicio del juego.
        Debug.Log("Juego iniciado. Preparando el primer turno del jugador.");
        StartPlayerTurn(); // Llama al inicio del primer turno del jugador.
    }

    /// Inicia el turno del jugador y avanza a la primera fase (Robo).
    public void StartPlayerTurn()
    {
        currentTurnNumber++; // Incrementa el contador para el nuevo turno.
        Debug.Log($"--- Inicio del Turno {currentTurnNumber} del Jugador ---");

        // --- ¡IMPORTANTE! Notifica a los suscriptores (como CardManager) que el turno ha comenzado. ---
        OnTurnStart?.Invoke(currentTurnNumber);

        // --- LÓGICA DE LIMPIEZA DE EFECTOS AL INICIO DEL TURNO ---
        // Esto asegura que, al comienzo de cada turno, se intenten limpiar los efectos visuales anteriores.
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ClearAllEffects(); // Llama al método de PlayerStats para ocultar el cuadradito
        }
        else
        {
            Debug.LogError("[TurnManager] PlayerStats.Instance es null al inicio del turno en StartPlayerTurn(). Asegúrate de que PlayerStats está en la escena y tiene un Singleton configurado.");
        }
        // --------------------------------------------------

        // Inicia la primera fase del turno.
        SetPhase(TurnPhase.DrawPhase);
    }

    /// Establece la fase actual y ejecuta su manejador. Centraliza la lógica de cambio de fase.
    /// <param name="newPhase">La nueva fase a establecer.</param>
    private void SetPhase(TurnPhase newPhase)
    {
        if (currentTurnPhase == newPhase) return; // Evita re-entrar en la misma fase si no es necesario.

        currentTurnPhase = newPhase;
        OnPhaseChange?.Invoke(currentTurnPhase); // Notifica a los suscriptores sobre el cambio de fase.
        UpdateTurnPhaseDisplay(); // Actualiza el texto de la fase en la UI.
        /*         Debug.Log($"[TurnManager] Cambiando a: {currentTurnPhase.ToString()} (Turno {currentTurnNumber})."); */

        // Llama al manejador de la nueva fase.
        switch (currentTurnPhase)
        {
            case TurnPhase.DrawPhase:
                HandleDrawPhase();
                break;
            case TurnPhase.ActionPhase:
                HandleActionPhase();
                break;
            case TurnPhase.EndTurn:
                StartCoroutine(HandleEndTurnPhaseRoutine()); // Usamos una corrutina para el fin de turno.
                break;
            case TurnPhase.None:
                // No debería ocurrir en un flujo normal, pero para seguridad.
                break;
        }
    }

    /// Avanza a la siguiente fase del turno del jugador, basada en la fase actual.
    public void AdvancePhase()
    {
        switch (currentTurnPhase)
        {
            case TurnPhase.None:
            case TurnPhase.EndTurn:
                // Después de EndTurn o None, StartPlayerTurn ya se encarga de iniciar la siguiente fase (DrawPhase).
                // No llamamos a SetPhase aquí para evitar doble inicio.
                Debug.LogWarning("[TurnManager] AdvancePhase llamado desde None/EndTurn. Se espera que StartPlayerTurn ya inicie el siguiente ciclo.");
                break;

            case TurnPhase.DrawPhase:
                SetPhase(TurnPhase.ActionPhase); // Después de DrawPhase, siempre se va a ActionPhase
                break;

            case TurnPhase.ActionPhase:
                // Si la mano excede el límite después de la ActionPhase, se va a DiscardPhase.
                // De lo contrario, pasa directamente a EndTurn.
                if (CheckIfHandExceedsLimit())
                {
                    Debug.LogWarning($"Aún tienes {GetHandCount()} cartas en mano. Debes descartar hasta tener {MAX_HAND_SIZE} para pasar de turno.");
                }
                else
                {
                    SetPhase(TurnPhase.EndTurn);
                }
                break;
        }
    }
    // --- Métodos de Manejo de Fases Específicas ---
    private void HandleDrawPhase()
    {
        Debug.Log("Iniciando Fase de Robo...");
        // ¡Esta es la ÚNICA línea que debe quedar aquí!
        StartCoroutine(DrawCardsAndLogRevolverRoutine());
    }

    private void HandleActionPhase()
    {
        Debug.Log("Iniciando Fase de Acción: Realiza tus jugadas.");
        // El jugador debe usar el botón "End Turn" (o barra espaciadora) para terminar esta fase.
    }

    /// Corrutina para manejar la fase de fin de turno. Permite limpieza y prepara el siguiente turno.
    private IEnumerator HandleEndTurnPhaseRoutine()
    {
        Debug.Log("Iniciando Fase de Fin de Turno: Limpieza y preparación...");
        OnPlayerTurnEnded?.Invoke(); // Notifica que el turno del jugador ha terminado.

        // Puedes añadir un pequeño retraso visual aquí si lo deseas:
        // yield return new WaitForSeconds(1.0f);

        Debug.Log("Preparando el siguiente turno...");
        currentTurnPhase = TurnPhase.None; // Resetea la fase para que StartPlayerTurn comience desde None.

        yield return null; // Un frame de espera para asegurar que todo se procese.

        StartPlayerTurn(); // Inicia el siguiente turno solo DESPUÉS de la fase de fin de turno.
    }

    // --- Interacción con el Jugador (desde UI o input) ---
    /// Método llamado por el botón "End Turn" para intentar avanzar la fase.
    public void EndPlayerTurnButton()
    {
        // Si estamos en la fase de acción y la mano aún excede el límite, no se permite terminar el turno.
        if (currentTurnPhase == TurnPhase.ActionPhase && CheckIfHandExceedsLimit())
        {
            Debug.LogWarning("No puedes terminar el turno. Debes descartar cartas para reducir tu mano al límite.");
            return; // Sale del método sin avanzar la fase.
        }

        Debug.Log("Solicitud de finalizar turno del jugador.");
        AdvancePhase(); // Si las condiciones son adecuadas, avanza a la siguiente fase.
    }


    // --- Métodos Auxiliares ---

    /// Comprueba si la mano del jugador excede el tamaño máximo permitido.
    private bool CheckIfHandExceedsLimit()
    {
        if (OnRequestHandCount != null)
        {
            return OnRequestHandCount.Invoke() > MAX_HAND_SIZE;
        }
        Debug.LogError("OnRequestHandCount es nulo. CardManager no está suscrito para proveer el conteo de la mano.");
        return false;
    }

    /// Obtiene el conteo actual de cartas en la mano del jugador.
    private int GetHandCount()
    {
        if (OnRequestHandCount != null)
        {
            return OnRequestHandCount.Invoke();
        }
        return 0;
    }

    // --- Métodos de Actualización de UI ---

    /// Actualiza el texto de la fase de turno en la UI.
    private void UpdateTurnPhaseDisplay()
    {
        if (turnPhaseText != null)
        {
            string newText = $"Fase: {currentTurnPhase.ToString().Replace("Phase", "")}\nTurno: {currentTurnNumber}";
            // Optimización: solo actualiza si el texto ha cambiado
            if (newText != lastTurnPhaseText)
            {
                turnPhaseText.text = newText;
                lastTurnPhaseText = newText;
            }
        }
        else
        {
            Debug.LogWarning("[TurnManager] turnPhaseText no asignado en el Inspector.");
        }
    }

    /// <summary>
    /// Actualiza el texto del conteo de la mano en la UI.
    /// Es público porque es llamado por el evento OnHandCountUpdated de CardManager.
    /// </summary>
    public void UpdateHandCountDisplay(int handCount)
    {
        if (handCountText != null)
        {
            string newText = $"Mano: {handCount}/{MAX_HAND_SIZE}";
            // Optimización: solo actualiza si el texto ha cambiado
            if (newText != lastHandCountText)
            {
                handCountText.text = newText;
                lastHandCountText = newText;
            }
        }
        else
        {
            Debug.LogWarning("[TurnManager] handCountText no asignado en el Inspector.");
        }
    }

    private IEnumerator DrawCardsAndLogRevolverRoutine()
    {
        // Lógica de robo de cartas (movida desde el antiguo HandleDrawPhase)
        if (currentTurnNumber == 1) // Si es el primer turno del juego
        {
            Debug.Log($"[TurnManager] Es el primer turno. Robando {MAX_HAND_SIZE} cartas iniciales.");
            for (int i = 0; i < MAX_HAND_SIZE; i++)
            {
                OnRequestDrawCard?.Invoke(); // Pide al CardManager que robe una carta.
                // Opcional: añadir un pequeño retraso visual entre cada carta robada
                // yield return new WaitForSeconds(0.05f); 
            }
            // Espera hasta que la mano tenga el tamaño completo de MAX_HAND_SIZE.
            // Esto asegura que todas las cartas iniciales se hayan procesado en CardManager.
            yield return new WaitUntil(() => GetHandCount() == MAX_HAND_SIZE); 
        }
        else // Es un turno posterior, robar una carta normal
        {
            OnRequestDrawCard?.Invoke(); // Pide al CardManager que robe una carta.
            // Espera un frame para que CardManager procese la solicitud y añada la carta.
            yield return null; 
            // O, una espera más robusta:
            // yield return new WaitUntil(() => GetHandCount() > (MAX_HAND_SIZE - 1) || currentTurnNumber == 0); // Asume que la mano tiene 4 o menos cartas antes de robar 1.
        }

        // --- Este es el punto donde la corrutina espera a que el robo termine ---
        // y luego solicita la actualización del log del Revolver.
        if (CardManager.Instance != null)
        {
            CardManager.Instance.RequestRevolverStatusUpdate();
        }
        else
        {
            Debug.LogError("[TurnManager] CardManager.Instance es null. No se puede actualizar el estado del Revolver después de robar.");
        }

        AdvancePhase(); // Avanza la fase SOLO después de que el robo y el log del Revolver se han actualizado.
    }
    /// Método llamado por el botón "Fire Revolver" para intentar disparar el Revolver.
        public void OnFireRevolverButtonPressed()
    {
        // 1. Verificar la fase actual del turno
        if (CurrentPhase != TurnPhase.ActionPhase)
        {
            Debug.LogWarning("[TurnManager] No puedes disparar el Revolver fuera de la Fase de Acción.");
            return;
        }

        // 2. Intentar el disparo a través de CardManager
        if (CardManager.Instance == null)
        {
            Debug.LogError("[TurnManager] CardManager.Instance es null. No se puede intentar el disparo del Revolver.");
            return;
        }

        bool shotSuccessful = CardManager.Instance.AttemptRevolverShot();

        if (shotSuccessful)
        {
            Debug.Log("[TurnManager] ¡Disparo de Revolver exitoso!");
            // Si disparar el revolver consume una acción o un punto de energía,
            // lo gestionarías aquí. Por ahora, solo se descartan las balas.
            // Si el disparo también avanza la fase o termina el turno, lo harías aquí.
            // Por ejemplo: AdvancePhase();
        }
        else
        {
            Debug.Log("[TurnManager] Falló el intento de disparo del Revolver (ver logs para más detalles).");
        }
    }
    void Update() { }
    

    
}
