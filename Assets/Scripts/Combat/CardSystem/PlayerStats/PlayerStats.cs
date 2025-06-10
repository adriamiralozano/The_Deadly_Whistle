// PlayerStats.cs
using UnityEngine;
using UnityEngine.UI; // Necesario para Image y Text
using TMPro; // Necesario si usas TextMeshProUGUI
using System.Collections.Generic; // Necesario para List<T>
using System; 


public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }
    public static event Action<bool> OnWeaponEquippedStatusChanged;
    public bool HasFiredRevolverThisTurn { get; private set; }


    // --- Indicador de Arma Equipada ---
    private bool _hasWeaponEquipped = false;

    // --- Indicador de Carta de Efecto Jugada ---
    private bool _hasPlayedEffectCardThisTurn = false;

    // NUEVO: Propiedad para almacenar la CardData del arma actualmente equipada
    public CardData CurrentEquippedWeapon { get; private set; }


    [Header("Player Health")]
    public int maxHealth = 5;
    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;

    [Header("UI Corazones")]
    [SerializeField] private GameObject heartUIPrefab;
    [SerializeField] private Transform heartUIParent;
    private List<GameObject> activeHearts = new List<GameObject>();

    [Header("UI References")]
    [SerializeField] private GameObject weaponEquippedIndicator; // Asigna tu cuadrado rojo aquí (para el arma)
    [SerializeField] private GameObject effectActiveIndicator; // Asigna tu cuadrado naranja aquí (para el efecto)
    [SerializeField] private TextMeshProUGUI effectCountText; // Asigna aquí un componente TextMeshProUGUI para el contador de efectos

    public bool HasWeaponEquipped
    {
        get { return _hasWeaponEquipped; }
        private set
        {
            // Solo actualizamos si el valor realmente ha cambiado
            if (_hasWeaponEquipped != value)
            {
                _hasWeaponEquipped = value;

                // Actualizar el indicador UI (si está asignado)
                if (weaponEquippedIndicator != null)
                {
                    weaponEquippedIndicator.SetActive(_hasWeaponEquipped);
                }

                // --- Disparar el evento ---
                OnWeaponEquippedStatusChanged?.Invoke(_hasWeaponEquipped);
                Debug.Log($"[PlayerStats] Evento OnWeaponEquippedStatusChanged disparado: {_hasWeaponEquipped}");

            }
        }
    }

    // --- Contador de Efectos Activos ---
    private int _activeEffectCount = 0; // Contador de efectos activos
    public int ActiveEffectCount => _activeEffectCount; // Propiedad de solo lectura para el contador

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


    void OnDisable()
    {
        // Desuscribirse del evento para evitar errores cuando el objeto se desactiva o destruye
        if (TurnManager.Instance != null)
        {
            TurnManager.OnTurnStart -= OnTurnStartHandler;
            Debug.Log("[PlayerStats] Desuscrito de TurnManager.OnTurnStart en OnDisable().");
        }
    }

    private void Start() 
    {
        // Suscripción al evento de TurnManager
        if (TurnManager.Instance != null)
        {
            TurnManager.OnTurnStart += OnTurnStartHandler;
            Debug.Log("[PlayerStats] Suscrito a TurnManager.OnTurnStart en Start().");
        }
        else
        {
            Debug.LogError("[PlayerStats] TurnManager.Instance es null en Start(). La suscripción al evento de inicio de turno no ocurrirá. Asegúrate de que TurnManager se inicialice primero.");
        }

        CurrentHealth = maxHealth;
        InitializeHeartUI();
        UpdateHeartUI();

        // Inicializa el estado al comienzo del juego
        HasWeaponEquipped = false;
        CurrentEquippedWeapon = null;
        _activeEffectCount = 0;
        UpdateEffectDisplay();
    }

    /// Manejador para el evento OnTurnStart de TurnManager.
    /// Reinicia los efectos activos al inicio de cada turno.
    private void OnTurnStartHandler(int turnNumber)
    {
        ResetRevolverFired();
        ClearAllEffects();
        _hasPlayedEffectCardThisTurn = false;
        Debug.Log($"[PlayerStats] OnTurnStartHandler: HasPlayedEffectCardThisTurn se ha reseteado a FALSE. Estado actual: {_hasPlayedEffectCardThisTurn}");
        Debug.Log($"[PlayerStats] Inicio de turno {turnNumber}. Efectos reiniciados. Se puede jugar una carta de efecto.");
    }

    // (El resto de tu script PlayerStats.cs sigue igual)
    // ...
    public void EquipWeapon(CardData weaponCard)
    {
        if (weaponCard == null)
        {
            Debug.LogError("[PlayerStats] Intentando equipar un arma nula.");
            return;
        }

        if (!HasWeaponEquipped)
        {
            HasWeaponEquipped = true;
            CurrentEquippedWeapon = weaponCard;
            Debug.Log($"[PlayerStats] Arma '{weaponCard.cardID}' equipada.");
        }
        else
        {
            Debug.LogWarning($"[PlayerStats] Intentando equipar arma '{weaponCard.cardID}', pero el jugador ya tiene equipada: {CurrentEquippedWeapon.cardID}");
        }
    }

    public void UnequipWeapon()
    {
        if (HasWeaponEquipped)
        {
            HasWeaponEquipped = false;
            CurrentEquippedWeapon = null;
            Debug.Log("[PlayerStats] Arma desequipada.");
        }
        else
        {
            Debug.LogWarning("[PlayerStats] Intentando desequipar arma, pero el jugador no tiene ninguna equipada.");
        }
    }

    public void ActivateEffect()
    {
        _hasPlayedEffectCardThisTurn = true;

        Debug.Log($"[PlayerStats] HasPlayedEffectCardThisTurn se ha puesto a TRUE después de activar un efecto. Estado actual: {_hasPlayedEffectCardThisTurn}");

        _activeEffectCount++;
        Debug.Log($"[PlayerStats] Efecto activado. Contador de efectos: {_activeEffectCount}");
        UpdateEffectDisplay();
    }

    public void DeactivateEffect()
    {
        if (_activeEffectCount > 0)
        {
            _activeEffectCount--;
            Debug.Log($"[PlayerStats] Efecto desactivado. Contador de efectos: {_activeEffectCount}");
            UpdateEffectDisplay();
        }
        else
        {
            Debug.LogWarning("[PlayerStats] Intentando desactivar efecto, pero el contador ya está en cero.");
        }
    }

    public void ClearAllEffects()
    {
        if (_activeEffectCount > 0)
        {
            Debug.Log($"[PlayerStats] Limpiando {_activeEffectCount} efectos activos.");
            _activeEffectCount = 0;
        }
        UpdateEffectDisplay();
    }

    private void UpdateEffectDisplay()
    {
        if (effectActiveIndicator != null)
        {
            bool shouldBeActive = _activeEffectCount > 0;
            if (effectActiveIndicator.activeSelf != shouldBeActive)
            {
                effectActiveIndicator.SetActive(shouldBeActive);
                Debug.Log($"[PlayerStats] Cuadradito naranja: Se {(shouldBeActive ? "activó" : "desactivó")} visualmente.");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerStats] El indicador de efecto (GameObject) NO ESTÁ ASIGNADO en el Inspector del PlayerStats.");
        }

        if (effectCountText != null)
        {
            effectCountText.text = _activeEffectCount.ToString();
        }
        else
        {
            Debug.LogWarning("[PlayerStats] El texto del contador de efectos (TextMeshProUGUI) NO ESTÁ ASIGNADO en el Inspector del PlayerStats.");
        }
    }
    public void MarkRevolverFired()
    {
        HasFiredRevolverThisTurn = true;
    }

    public void ResetRevolverFired()
    {
        HasFiredRevolverThisTurn = false;
    }

    private void Die()
    {
        if (CardManager.Instance != null && CardManager.Instance.AttemptUseBibleCard())
        {
            Debug.Log("[PlayerStats] ¡La Biblia te ha salvado! Recuperando vida.");
            UpdateHeartUI();
            return;
        }

        CurrentHealth = 0;
        Debug.LogWarning("[PlayerStats] ¡El jugador ha muerto!");
    }

    private void InitializeHeartUI()
    {
        if (heartUIPrefab == null || heartUIParent == null)
        {
            Debug.LogWarning("[PlayerStats] Heart UI Prefab o Heart UI Parent no asignado.");
            return;
        }

        foreach (var heart in activeHearts)
            Destroy(heart);
        activeHearts.Clear();

        for (int i = 0; i < maxHealth; i++)
        {
            GameObject newHeart = Instantiate(heartUIPrefab, heartUIParent);
            newHeart.name = $"PlayerHeart_{i}";
            activeHearts.Add(newHeart);
        }
    }

    private void UpdateHeartUI()
    {
        for (int i = 0; i < activeHearts.Count; i++)
        {
            if (activeHearts[i] != null)
            {
                bool isFull = i < CurrentHealth;

                Image img = activeHearts[i].GetComponent<Image>();
                if (img != null)
                    img.color = isFull ? Color.red : Color.gray;

                SpriteRenderer sr = activeHearts[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = isFull ? Color.red : Color.gray;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (CardManager.Instance != null && CanBeDamaged() == false)
        {
            return;
        }

        int damageTaken = Mathf.Min(amount, CurrentHealth);
        CurrentHealth -= damageTaken;
        Debug.Log($"[PlayerStats] El jugador recibió {damageTaken} de daño. HP restante: {CurrentHealth}");

        UpdateHeartUI();

        if (CurrentHealth <= 0) Die();
    }

    public bool CanBeDamaged()
    {
        if (CardManager.Instance != null && CardManager.Instance.AttemptUseCoverCard())
        {
            Debug.Log("[PlayerStats] ¡Daño bloqueado por la carta Cover!");
            return false;
        }

        return true;
    }

    public bool HasPlayedEffectCardThisTurn()
    {
        return _hasPlayedEffectCardThisTurn;
    }

    public void Heal(int amount)
    {
        if (CurrentHealth < maxHealth)
        {
            int healAmount = Mathf.Min(amount, maxHealth - CurrentHealth);
            CurrentHealth += healAmount;
            Debug.Log($"[PlayerStats] El jugador se curó {healAmount} punto(s) de vida. HP actual: {CurrentHealth}");
            UpdateHeartUI();
        }
        else
        {
            Debug.Log("[PlayerStats] El jugador ya tiene la vida al máximo. No se puede curar más.");
        }
    }
    

}