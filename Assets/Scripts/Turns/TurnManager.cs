// TurnManager.cs
using UnityEngine;
using System;
using TMPro;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI turnPhaseText;
    [SerializeField] private TextMeshProUGUI handCountText;

    public enum TurnPhase
    {
        None,
        DrawPhase,
        ActionPhase,
        DiscardPhase,
        EndTurn
    }

    private TurnPhase currentTurnPhase = TurnPhase.None;
    private int currentTurnNumber = 0;
    private const int MAX_HAND_SIZE = 5;

    private string lastTurnPhaseText = "";
    private string lastHandCountText = "";

    public static event Action<int> OnTurnStart;
    public static event Action<TurnPhase> OnPhaseChange;
    public static event Action OnPlayerTurnEnded;

    public static event Action OnRequestDrawCard;
    public static event Func<int> OnRequestHandCount;
    public static event Action OnRequestDiscardCard;


    void OnEnable()
    {
        CardManager.OnHandCountUpdated += UpdateHandCountDisplay;
    }

    void OnDisable()
    {
        CardManager.OnHandCountUpdated -= UpdateHandCountDisplay;
    }

    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        currentTurnNumber = 0;
        Debug.Log("Juego iniciado. Preparando el primer turno del jugador.");
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        currentTurnNumber++;
        Debug.Log($"--- Inicio del Turno {currentTurnNumber} del Jugador ---");
        OnTurnStart?.Invoke(currentTurnNumber);

        SetPhase(TurnPhase.DrawPhase);
    }

    private void SetPhase(TurnPhase newPhase)
    {
        if (currentTurnPhase == newPhase) return;

        currentTurnPhase = newPhase;
        OnPhaseChange?.Invoke(currentTurnPhase);
        UpdateTurnPhaseDisplay();
        Debug.Log($"[TurnManager] Cambiando a: {currentTurnPhase.ToString()} (Turno {currentTurnNumber}).");

        switch (currentTurnPhase)
        {
            case TurnPhase.DrawPhase:
                HandleDrawPhase();
                break;
            case TurnPhase.ActionPhase:
                HandleActionPhase();
                break;
            case TurnPhase.DiscardPhase:
                HandleDiscardPhase();
                break;
            case TurnPhase.EndTurn:
                StartCoroutine(HandleEndTurnPhaseRoutine());
                break;
            case TurnPhase.None:
                break;
        }
    }

    public void AdvancePhase()
    {
        switch (currentTurnPhase)
        {
            case TurnPhase.None:
            case TurnPhase.EndTurn:
                SetPhase(TurnPhase.DrawPhase);
                break;

            case TurnPhase.DrawPhase:
                SetPhase(TurnPhase.ActionPhase);
                break;

            case TurnPhase.ActionPhase:
                if (CheckIfHandExceedsLimit())
                {
                    SetPhase(TurnPhase.DiscardPhase);
                }
                else
                {
                    SetPhase(TurnPhase.EndTurn);
                }
                break;

            case TurnPhase.DiscardPhase:
                if (!CheckIfHandExceedsLimit())
                {
                    SetPhase(TurnPhase.EndTurn);
                }
                else
                {
                    Debug.LogWarning($"Aún tienes {GetHandCount()} cartas en mano. Debes descartar hasta tener {MAX_HAND_SIZE} para pasar de turno.");
                }
                break;
        }
    }

    private void HandleDrawPhase()
    {
        Debug.Log("Iniciando Fase de Robo...");
        if (currentTurnNumber == 1) // Es el primer turno
        {
            Debug.Log($"[TurnManager] Es el primer turno. Robando {MAX_HAND_SIZE} cartas iniciales.");
            for (int i = 0; i < MAX_HAND_SIZE; i++)
            {
                OnRequestDrawCard?.Invoke(); // Pide al CardManager que robe una carta.
            }
        }
        else // Es un turno posterior
        {
            Debug.Log("[TurnManager] Robando una carta normal.");
            OnRequestDrawCard?.Invoke(); // Pide al CardManager que robe una carta.
        }
        AdvancePhase(); // Avanza inmediatamente a la fase de acción después de robar.
    }

    private void HandleActionPhase()
    {
        Debug.Log("Iniciando Fase de Acción: Realiza tus jugadas.");
    }

    private void HandleDiscardPhase()
    {
        Debug.Log($"Iniciando Fase de Descarte: Tu mano tiene {GetHandCount()} cartas. Debes descartar hasta tener {MAX_HAND_SIZE}.");
    }

    private IEnumerator HandleEndTurnPhaseRoutine()
    {
        Debug.Log("Iniciando Fase de Fin de Turno: Limpieza y preparación...");
        OnPlayerTurnEnded?.Invoke();

        yield return null;

        Debug.Log("Preparando el siguiente turno...");
        currentTurnPhase = TurnPhase.None; // Resetear la fase para que StartPlayerTurn comience desde None.
        StartPlayerTurn();
    }

    public void EndPlayerTurnButton()
    {
        if (currentTurnPhase == TurnPhase.DiscardPhase && CheckIfHandExceedsLimit())
        {
            Debug.LogWarning("No puedes terminar el turno. Debes descartar cartas para reducir tu mano al límite.");
            return;
        }

        Debug.Log("Solicitud de finalizar turno del jugador.");
        AdvancePhase();
    }

    private bool CheckIfHandExceedsLimit()
    {
        if (OnRequestHandCount != null)
        {
            return OnRequestHandCount.Invoke() > MAX_HAND_SIZE;
        }
        Debug.LogError("OnRequestHandCount es nulo. CardManager no está suscrito para proveer el conteo de la mano.");
        return false;
    }

    private int GetHandCount()
    {
        if (OnRequestHandCount != null)
        {
            return OnRequestHandCount.Invoke();
        }
        return 0;
    }

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"[DEBUG] Barra espaciadora presionada en fase: {currentTurnPhase}");
            if (currentTurnPhase == TurnPhase.DiscardPhase)
            {
                Debug.Log("[DEBUG] Solicitando descarte de carta desde TurnManager.");
                OnRequestDiscardCard?.Invoke();
            }
            else
            {
                EndPlayerTurnButton();
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentTurnPhase == TurnPhase.ActionPhase)
            {
                Debug.Log("[DEBUG] Robando una carta extra con 'R' (Solo para pruebas en fase de Acción).");
                OnRequestDrawCard?.Invoke();
            }
            else
            {
                Debug.LogWarning("[DEBUG] La tecla 'R' (Robar extra) solo funciona en la Fase de Acción.");
            }
        }
    }
}