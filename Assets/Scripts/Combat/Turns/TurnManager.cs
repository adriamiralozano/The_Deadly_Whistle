// TurnManager.cs
using UnityEngine;
using System; // Para Action y Func
using TMPro; // Para TextMeshProUGUI
using System.Collections; // Para Coroutines
using UnityEngine.UI;


public class TurnManager : MonoBehaviour
{
    // --- Referencias UI ---
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI turnPhaseText; // Para el texto de fase de turno
    [SerializeField] private TextMeshProUGUI handCountText; // Para el conteo de la mano
    [SerializeField] private GameObject enemyTurnBanner; // Para mostrar un banner durante el turno del enemigo

    // --- Singleton ---
    public static TurnManager Instance { get; private set; }

    [Header("Game References")]

    private PlayerStats playerStats;
    private CardManager cardManager;

    [SerializeField] public Enemy activeEnemy; // Referencia al enemigo actual en la escena. ¡Asigna esto en el Inspector!
    [SerializeField] private GameObject backgroundGO;
    [SerializeField] private GameObject zoomBackgroundGO; // Asigna el sprite para el zoom en el inspector
    private Sprite originalSprite;


    [Header("Animación de Disparo")]
    [SerializeField] private int lateralDurationLevel = 1;
    [SerializeField] private GameObject[] playerShotEffects;


    // --- Enumeración de Fases de Turno ---
    public enum TurnPhase
    {
        None,           // Estado inicial o entre turnos
        Preparation,    // Fase de preparación
        DrawPhase,      // Fase de robo: robar una carta.
        ActionPhase,    // Fase de acción: jugar cartas.
        ShotPhase,      // Fase de disparo: disparar el revolver.
        DiscardPostShot, // Fase de descarte después de disparar.
        EndTurn,        // Fase final del turno: limpieza y preparación para el siguiente.
        EnemyTurn       // Fase de turno del enemigo 
    }

    // --- Variables de Estado del Turno ---
    public int currentTurnNumber { get; private set; } = 0;
    private const int MAX_HAND_SIZE = 5;

    // --- Inicialización del Singleton ---
    private TurnPhase currentTurnPhase = TurnPhase.None;
    public TurnPhase CurrentPhase { get { return currentTurnPhase; } }

    // Para optimizar actualizaciones de UI
    private string lastTurnPhaseText = "";
    private string lastHandCountText = "";

    // --- Eventos de Turno ---
    public static event Action<int> OnTurnStart;        // Se dispara al inicio de un nuevo turno
    public static event Action<TurnPhase> OnPhaseChange; // Se dispara cada vez que la fase de turno cambia
    public static event Action OnPlayerTurnEnded;       // Se dispara cuando el turno del jugador termina (antes de iniciar el siguiente)

    // --- Eventos para comunicación con CardManager ---
    public static event Action OnRequestDrawCard;       // Solicita al CardManager que robe una carta
    public static event Func<int> OnRequestHandCount;   // Solicita al CardManager el conteo de la mano
    private bool enemyTurnCompleted = false;


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

        // --- Obtener referencias de componentes en el mismo GameObject ---
        playerStats = GetComponent<PlayerStats>();
        cardManager = GetComponent<CardManager>();

        // Validaciones mejoradas:
        if (playerStats == null)
            Debug.LogError("[TurnManager] PlayerStats no encontrado en este GameObject. Asegúrate de que PlayerStats.cs esté en el mismo GameObject que TurnManager.cs.", this);
        if (cardManager == null)
            Debug.LogError("[TurnManager] CardManager no encontrado en este GameObject. Asegúrate de que CardManager.cs esté en el mismo GameObject que TurnManager.cs.", this);

        if (activeEnemy == null)
            Debug.LogWarning("[TurnManager] No hay un enemigo activo asignado en el Inspector.", this);
    }

    // --- Suscripción/Desuscripción a Eventos ---
    void OnEnable()
    {
        CardManager.OnHandCountUpdated += UpdateHandCountDisplay;
        OutlawEnemyAI.OnEnemyTurnCompleted += OnEnemyTurnCompleted; // Suscribirse al evento
    }

    void OnDisable()
    {
        CardManager.OnHandCountUpdated -= UpdateHandCountDisplay;
        OutlawEnemyAI.OnEnemyTurnCompleted -= OnEnemyTurnCompleted; // Desuscribirse del evento
    }

    // --- Métodos de Inicio ---
    void Start()
    {
        StartGame();
    }

    /// Inicia el juego y el primer turno del jugador.
    public void StartGame()
    {
        // currentTurnNumber se mantiene en 0 aquí. Será incrementado a 1 en StartPlayerTurn() la primera vez.
        // --- ¡CAMBIO CRUCIAL AQUÍ! ELIMINAMOS currentTurnNumber++; ---
        Debug.Log("Juego iniciado. Preparando la fase de preparación inicial.");
        SetPhase(TurnPhase.Preparation); // Solo aquí se usa Preparation
    }

    /// Inicia el turno del jugador y avanza a la primera fase (Robo).
    public void StartPlayerTurn()
    {
        currentTurnNumber++; // Esto asegura que el primer turno sea el Turno 1
        Debug.Log($"--- INICIO DEL TURNO {currentTurnNumber} DEL JUGADOR ---");

        OnTurnStart?.Invoke(currentTurnNumber);

        if (playerStats != null)
            playerStats.ClearAllEffects();
        else
            Debug.LogError("[TurnManager] PlayerStats es null al inicio del turno en StartPlayerTurn().");

        SetPhase(TurnPhase.DrawPhase);
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
            case TurnPhase.ShotPhase:
                HandleShotPhase();
                break;
            case TurnPhase.DiscardPostShot:
                Debug.Log("Fase de descarte después del disparo (ShotPhase). Aquí puedes implementar la lógica de descarte.");
                break;
            case TurnPhase.EndTurn:
                StartCoroutine(HandleEndTurnPhaseRoutine());
                break;
            case TurnPhase.EnemyTurn:
                Debug.Log("[TurnManager] ---¡ENTRANDO EN EL TURNO DEL ENEMIGO!---");
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
                if (CheckIfHandExceedsLimit())
                {
                    Debug.LogWarning($"Aún tienes {GetHandCount()} cartas en mano. Debes descartar hasta tener {MAX_HAND_SIZE} para pasar de turno.");
                    SetPhase(TurnPhase.DiscardPostShot);
                }
                else
                {
                    SetPhase(TurnPhase.EndTurn);
                }
                break;
            case TurnPhase.DiscardPostShot:
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
        if (currentTurnPhase != TurnPhase.ActionPhase)
        {
            if (currentTurnPhase == TurnPhase.DiscardPostShot)
            {
                Debug.LogWarning("Terminar la fase");
            }
            else
            {
                Debug.LogWarning("No puedes terminar el turno fuera de la Fase de Acción.");
                return; // Sale del método sin avanzar la fase.
            }
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

    private void OnEnemyTurnCompleted()
    {
        enemyTurnCompleted = true;
        Debug.Log("[TurnManager] El enemigo ha completado su turno.");
    }

    private IEnumerator HandleEnemyTurnPhaseRoutine()
    {
        if (turnPhaseText != null)
            turnPhaseText.text = "Turno del enemigo";

        if (enemyTurnBanner != null)
            enemyTurnBanner.SetActive(true);

        Debug.Log("Turno del enemigo: esperando 1 segundo antes de la acción...");
        yield return new WaitForSeconds(1f);

        enemyTurnCompleted = false; // Resetea la bandera

        if (activeEnemy != null && activeEnemy.IsAlive)
        {
            Debug.Log($"Turno del enemigo: {activeEnemy.Data.enemyName} realizando su acción...");
            activeEnemy.PerformTurnAction();

            // NUEVO: Espera hasta que el enemigo complete todas sus acciones
            yield return new WaitUntil(() => enemyTurnCompleted);
        }
        else
        {
            Debug.LogWarning("[TurnManager] No hay un enemigo activo válido para realizar el turno.");
        }

        Debug.Log("Turno del enemigo: esperando 1 segundo después de completar las acciones...");
        yield return new WaitForSeconds(1f);

        if (enemyTurnBanner != null)
            enemyTurnBanner.SetActive(false);

        StartPlayerTurn();
    }

    private IEnumerator DrawCardsAndLogRevolverRoutine()
    {
        // currentTurnNumber es el turno que *acaba* de empezar, por lo que para el primer robo (Turno 1), queremos robar todas.
        // Si currentTurnNumber es 0 (estado inicial), deberíamos robar 5 cartas para la mano inicial.
        // El primer turno de juego es el número 1. Entonces, si currentTurnNumber es 1, se roban todas las cartas.
        // Si currentTurnNumber es > 1, se roba 1 carta.

        int cardsToDraw = 1; // Por defecto, robar 1 carta
        if (currentTurnNumber == 1) // Si estamos en el primer turno de juego
        {
            cardsToDraw = MAX_HAND_SIZE; // Robar 5 cartas para la mano inicial
        }

        Debug.Log($"[TurnManager] Robando {cardsToDraw} carta(s).");
        for (int i = 0; i < cardsToDraw; i++)
        {
            OnRequestDrawCard?.Invoke();
            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitUntil(() => GetHandCount() >= cardsToDraw); // Espera hasta que se hayan robado las cartas


        if (cardManager != null)
        {
            cardManager.RequestRevolverStatusUpdate();
        }
        else
        {
            Debug.LogError("[TurnManager] CardManager es null. No se puede actualizar el estado del Revolver después de robar.");
        }

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
        if (cardManager == null)
        {
            Debug.LogError("[TurnManager] CardManager es null. No se puede intentar el disparo del Revolver.");
            return;
        }

        bool shotSuccessful = cardManager.AttemptRevolverShot();

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
        Debug.Log("Iniciando Fase de Disparo (ShotPhase). Mostrando panel QTE...");

        // Mostrar el panel QTE al entrar en ShotPhase
        CombosManager combosManager = FindObjectOfType<CombosManager>();
        if (combosManager != null)
        {
            combosManager.ShowQTEPanel();
        }
    }

    public IEnumerator ShotFeedback()
    {
        var playerVisual = FindObjectOfType<PlayerVisualManager>();
        var enemyVisual = FindObjectOfType<EnemyVisualManager>();

        // Guarda el sprite original del enemigo
        Sprite enemyOriginalSprite = null;
        Sprite playerOriginalSprite = null;
        SpriteRenderer enemySpriteRenderer = null;
        SpriteRenderer playerSpriteRenderer = null;

        if (playerShotEffects != null)
        {
            // Desactiva todos primero
            foreach (var go in playerShotEffects)
                if (go != null) go.SetActive(false);

        }

        if (enemyVisual != null)
        {
            enemySpriteRenderer = enemyVisual.GetComponent<SpriteRenderer>();
            if (enemySpriteRenderer != null)
                enemyOriginalSprite = enemySpriteRenderer.sprite;
            enemyVisual.SetShotedEnemySprite();
        }

        if (playerVisual != null)
        {
            playerSpriteRenderer = playerVisual.GetComponent<SpriteRenderer>();
            if (playerSpriteRenderer != null)
                playerOriginalSprite = playerSpriteRenderer.sprite;
            playerVisual.SetRevolverShotSprite();
        }

        // Ambos fondos activos SIEMPRE
        backgroundGO.SetActive(true);
        zoomBackgroundGO.SetActive(true);

        // Referencias
        var bgRenderer = backgroundGO.GetComponent<SpriteRenderer>();
        var zoomBgRenderer = zoomBackgroundGO.GetComponent<SpriteRenderer>();

        // Sorting orders originales
        int bgOrder = bgRenderer.sortingOrder;
        int zoomOrder = zoomBgRenderer.sortingOrder;

        // Poner el fondo de zoom delante
        zoomBgRenderer.sortingOrder = bgOrder + 1;

        // Transforms
        Transform bgTransform = backgroundGO.transform;
        Transform zoomBgTransform = zoomBackgroundGO.transform;
        Transform playerTransform = playerVisual != null ? playerVisual.transform : null;
        Transform enemyTransform = enemyVisual != null ? enemyVisual.transform : null;

        Vector3 bgOriginal = bgTransform.localScale;
        Vector3 bgTarget = bgOriginal * 1.2f;

        Vector3 playerOriginal = playerTransform != null ? playerTransform.localScale : Vector3.one;
        Vector3 playerTarget = playerOriginal * 1.2f;

        Vector3 enemyOriginal = enemyTransform != null ? enemyTransform.localScale : Vector3.one;
        Vector3 enemyTarget = enemyOriginal * 1.2f;

        Vector3 playerPosOriginal = playerTransform != null ? playerTransform.position : Vector3.zero;
        Vector3 enemyPosOriginal = enemyTransform != null ? enemyTransform.position : Vector3.zero;

        Vector3 centerWorld = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, playerPosOriginal.z - Camera.main.transform.position.z));
        Vector3 playerTargetPos = centerWorld + Vector3.left * 3f;
        Vector3 enemyTargetPos = centerWorld + Vector3.right * 3f;

        float approachDuration = 0.2f;
        float lateralDuration = 1.2f;
        switch (lateralDurationLevel)
        {
            case 1:
                lateralDuration = 1.2f;
                break;
            case 2:
                lateralDuration = 1.8f;
                break;
            case 3:
                lateralDuration = 2.4f;
                break;
            default:
                lateralDuration = 1.2f;
                break;
        }
        float returnDuration = 0.12f;
        float lateralOffset = 1.5f;

        // --- FASE 1: Zoom y acercamiento al centro (ambos fondos y personajes) ---
        float elapsed = 0f;
        while (elapsed < approachDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / approachDuration;

            // Ambos fondos hacen zoom
            bgTransform.localScale = Vector3.Lerp(bgOriginal, bgTarget, t);
            zoomBgTransform.localScale = Vector3.Lerp(bgOriginal, bgTarget, t);

            // Personajes
            if (playerTransform != null)
            {
                playerTransform.localScale = Vector3.Lerp(playerOriginal, playerTarget, t);
                playerTransform.position = Vector3.Lerp(playerPosOriginal, playerTargetPos, t);
            }
            if (enemyTransform != null)
            {
                enemyTransform.localScale = Vector3.Lerp(enemyOriginal, enemyTarget, t);
                enemyTransform.position = Vector3.Lerp(enemyPosOriginal, enemyTargetPos, t);
            }

            yield return null;
        }
        bgTransform.localScale = bgTarget;
        zoomBgTransform.localScale = bgTarget;
        if (playerTransform != null)
        {
            playerTransform.localScale = playerTarget;
            playerTransform.position = playerTargetPos;
        }
        if (enemyTransform != null)
        {
            enemyTransform.localScale = enemyTarget;
            enemyTransform.position = enemyTargetPos;
        }

        // --- FASE 2: Alejamiento lateral de personajes (fondos estáticos) ---
        elapsed = 0f;
        Vector3 playerLateralTarget = playerTargetPos + Vector3.left * lateralOffset;
        Vector3 enemyLateralTarget = enemyTargetPos + Vector3.right * lateralOffset;

        // Guarda la rotación original
        Quaternion bgOriginalRot = bgTransform.rotation;
        Quaternion zoomBgOriginalRot = zoomBgTransform.rotation;
        Quaternion bgTargetRot = Quaternion.Euler(0, 0, -6f);
        Quaternion zoomBgTargetRot = Quaternion.Euler(0, 0, -6f);

        // Variables para disparos secuenciales
        float shotInterval = lateralDuration / Mathf.Max(1, lateralDurationLevel); // Espaciado entre disparos
        float nextShotTime = shotInterval;
        int shotsActivated = 0;

        // Desactiva todos los efectos antes de empezar Fase 2
        if (playerShotEffects != null)
        {
            foreach (var go in playerShotEffects)
                if (go != null) go.SetActive(false);
        }

        while (elapsed < lateralDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lateralDuration;

            // Activa disparos uno a uno según el tiempo
            if (playerShotEffects != null && shotsActivated < lateralDurationLevel && elapsed >= nextShotTime - shotInterval / 2f)
            {
                if (playerShotEffects[shotsActivated] != null)
                    playerShotEffects[shotsActivated].SetActive(true);
                shotsActivated++;
                nextShotTime += shotInterval;
            }

            // Personajes
            if (playerTransform != null)
                playerTransform.position = Vector3.Lerp(playerTargetPos, playerLateralTarget, t);
            if (enemyTransform != null)
                enemyTransform.position = Vector3.Lerp(enemyTargetPos, enemyLateralTarget, t);

            // Fondos rotan hacia la derecha
            bgTransform.rotation = Quaternion.Lerp(bgOriginalRot, bgTargetRot, t);
            zoomBgTransform.rotation = Quaternion.Lerp(zoomBgOriginalRot, zoomBgTargetRot, t);

            yield return null;
        }
        if (playerTransform != null)
            playerTransform.position = playerLateralTarget;
        if (enemyTransform != null)
            enemyTransform.position = enemyLateralTarget;

        // Asegura rotación final de fase 2
        bgTransform.rotation = bgTargetRot;
        zoomBgTransform.rotation = zoomBgTargetRot;

        // Desactiva todos los efectos al acabar Fase 2
        if (playerShotEffects != null)
        {
            foreach (var go in playerShotEffects)
                if (go != null) go.SetActive(false);
        }

        yield return new WaitForSeconds(0.1f);

        // --- FASE 3: Regreso sincronizado de personajes y fondos ---
        elapsed = 0f;
        Vector3 playerStartPos = playerTransform != null ? playerTransform.position : Vector3.zero;
        Vector3 enemyStartPos = enemyTransform != null ? enemyTransform.position : Vector3.zero;
        Vector3 playerStartScale = playerTransform != null ? playerTransform.localScale : Vector3.one;
        Vector3 enemyStartScale = enemyTransform != null ? enemyTransform.localScale : Vector3.one;

        zoomBgRenderer.color = new Color(1f, 1f, 1f, 1f);
        bgRenderer.color = new Color(1f, 1f, 1f, 1f);

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / returnDuration);

            // Ambos fondos hacen zoom out juntos y regresan la rotación
            bgTransform.localScale = Vector3.Lerp(bgTarget, bgOriginal, t);
            zoomBgTransform.localScale = Vector3.Lerp(bgTarget, bgOriginal, t);

            bgTransform.rotation = Quaternion.Lerp(bgTargetRot, bgOriginalRot, t);
            zoomBgTransform.rotation = Quaternion.Lerp(zoomBgTargetRot, zoomBgOriginalRot, t);

            // Crossfade y personajes (igual que antes)
            zoomBgRenderer.color = new Color(1f, 1f, 1f, 1f - t);

            if (playerTransform != null)
            {
                playerTransform.localScale = Vector3.Lerp(playerTarget, playerOriginal, t);
                playerTransform.position = Vector3.Lerp(playerStartPos, playerPosOriginal, t);
            }
            if (enemyTransform != null)
            {
                enemyTransform.localScale = Vector3.Lerp(enemyTarget, enemyOriginal, t);
                enemyTransform.position = Vector3.Lerp(enemyStartPos, enemyPosOriginal, t);
            }

            yield return null;
        }
        // Asegura valores finales
        bgTransform.localScale = bgOriginal;
        zoomBgTransform.localScale = bgOriginal;
        bgTransform.rotation = bgOriginalRot;
        zoomBgTransform.rotation = zoomBgOriginalRot;
        zoomBgRenderer.color = new Color(1f, 1f, 1f, 0f);
        bgRenderer.color = new Color(1f, 1f, 1f, 1f);

        if (playerTransform != null)
        {
            playerTransform.localScale = playerOriginal;
            playerTransform.position = playerPosOriginal;
        }
        if (enemyTransform != null)
        {
            enemyTransform.localScale = enemyOriginal;
            enemyTransform.position = enemyPosOriginal;
        }

        // Restaurar sorting order original
        zoomBgRenderer.sortingOrder = zoomOrder;

        zoomBgRenderer.color = new Color(1f, 1f, 1f, 1f);

        yield return new WaitForSeconds(0.2f);

        if (enemySpriteRenderer != null && enemyOriginalSprite != null)
            enemySpriteRenderer.sprite = enemyOriginalSprite;

        // Restaurar sprite original del player
        if (playerSpriteRenderer != null && playerOriginalSprite != null)
            playerSpriteRenderer.sprite = playerOriginalSprite;

        zoomBackgroundGO.SetActive(false);
    }

}