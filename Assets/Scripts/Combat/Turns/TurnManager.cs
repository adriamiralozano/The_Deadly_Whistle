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
    [SerializeField] private GameObject enemyTurnBanner; // Para mostrar un banner durante el turno del enemigo

    // --- Singleton ---
    public static TurnManager Instance { get; private set; }


    // --- Enumeración de Fases de Turno ---
    public enum TurnPhase
    {
        None,           // Estado inicial o entre turnos
        Preparation,    // Fase de preparación
        DrawPhase,      // Fase de robo: robar una carta.
        ActionPhase,    // Fase de acción: jugar cartas.
        ShotPhase,        // Fase de disparo: disparar el revolver.
        EndTurn,         // Fase final del turno: limpieza y preparación para el siguiente.
        EnemyTurn       // Fase de turno del enemigo 
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
        currentTurnNumber = 0;
        Debug.Log("Juego iniciado. Preparando la fase de preparación inicial.");
        SetPhase(TurnPhase.Preparation); // Solo aquí se usa Preparation
    }

    /// Inicia el turno del jugador y avanza a la primera fase (Robo).
    public void StartPlayerTurn()
    {
        currentTurnNumber++;
        Debug.Log($"--- Inicio del Turno {currentTurnNumber} del Jugador ---");

        OnTurnStart?.Invoke(currentTurnNumber);

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.ClearAllEffects();
        else
            Debug.LogError("[TurnManager] PlayerStats.Instance es null al inicio del turno en StartPlayerTurn().");

        SetPhase(TurnPhase.DrawPhase); // Ya NO pasa por Preparation
    }

    /// Cambia la fase actual del turno y maneja la lógica asociada a cada fase.
    private void SetPhase(TurnPhase newPhase)
    {
        if (currentTurnPhase == newPhase) return;

        currentTurnPhase = newPhase;
        OnPhaseChange?.Invoke(currentTurnPhase);
        UpdateTurnPhaseDisplay();

        switch (currentTurnPhase)
        {
            case TurnPhase.Preparation:
                StartCoroutine(HandlePreparationPhaseRoutine());
                break;
            case TurnPhase.DrawPhase:
                HandleDrawPhase();
                break;
            case TurnPhase.ActionPhase:
                HandleActionPhase();
                break;
            case TurnPhase.ShotPhase: // <--- Añade este bloque
                HandleShotPhase();
                break;
            case TurnPhase.EndTurn:
                StartCoroutine(HandleEndTurnPhaseRoutine());
                break;
            case TurnPhase.EnemyTurn:
                Debug.Log("[TurnManager] ¡Entrando en la fase EnemyTurn!");
                StartCoroutine(HandleEnemyTurnPhaseRoutine());
                break;
            case TurnPhase.None:
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
                SetPhase(TurnPhase.ActionPhase);
                break;

            case TurnPhase.ActionPhase:
                if (CheckIfHandExceedsLimit())
                {
                    Debug.LogWarning($"Aún tienes {GetHandCount()} cartas en mano. Debes descartar hasta tener {MAX_HAND_SIZE} para pasar de turno.");
                }
                else
                {
                    SetPhase(TurnPhase.EndTurn);
                }
                break;

            case TurnPhase.ShotPhase:
                SetPhase(TurnPhase.EndTurn);
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

        SetPhase(TurnPhase.EnemyTurn);
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

    private IEnumerator HandleEnemyTurnPhaseRoutine()
    {
        if (turnPhaseText != null)
            turnPhaseText.text = "Turno del enemigo";

        if (enemyTurnBanner != null)
            enemyTurnBanner.SetActive(true);

        Debug.Log("Turno del enemigo: esperando 5 segundos...");
        Debug.Log("Turno del enemigo: el enemigo intenta disparar...");
        yield return new WaitForSeconds(1f);

        Enemy enemy = FindObjectOfType<Enemy>();
        if (enemy != null)
            enemy.TryShootPlayer();
        else
            Debug.LogWarning("No se encontró un BasicEnemy en la escena.");

        yield return new WaitForSeconds(4f);
        if (enemyTurnBanner != null)
            enemyTurnBanner.SetActive(false);

        // Al acabar, inicia el siguiente turno del jugador
        StartPlayerTurn();
    }

    private IEnumerator DrawCardsAndLogRevolverRoutine()
    {
        int cardsToDraw = (currentTurnNumber == 1) ? MAX_HAND_SIZE : 1;

        Debug.Log($"[TurnManager] Robando {cardsToDraw} carta(s).");
        for (int i = 0; i < cardsToDraw; i++)
        {
            OnRequestDrawCard?.Invoke();
            yield return new WaitForSeconds(0.5f);
        }
        // Espera a que la mano tenga el tamaño correcto (por seguridad)
        yield return new WaitUntil(() => GetHandCount() >= cardsToDraw);

        if (CardManager.Instance != null)
        {
            CardManager.Instance.RequestRevolverStatusUpdate();
        }
        else
        {
            Debug.LogError("[TurnManager] CardManager.Instance es null. No se puede actualizar el estado del Revolver después de robar.");
        }

        // Elimina la espera fija de 5 segundos
        // yield return new WaitForSeconds(5f);

        AdvancePhase();
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
            SetPhase(TurnPhase.ShotPhase);
        }
        else
        {
            Debug.Log("[TurnManager] Falló el intento de disparo del Revolver (ver logs para más detalles).");
        }
    }


    private IEnumerator HandlePreparationPhaseRoutine()
    {
        Debug.Log("Fase de preparación inicial...");
        yield return new WaitForSeconds(1f); // Espera 1 segundo
        StartPlayerTurn(); // Ahora sí, inicia el primer turno normalmente
    }
    
    private void HandleShotPhase()
    {
        Debug.Log("Iniciando Fase de Disparo (ShotPhase). Aquí va la lógica de disparo especial.");
        // Aquí puedes poner la lógica específica de la ShotPhase
    }

}
