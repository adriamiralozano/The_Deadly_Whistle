// PlayerStats.cs
using UnityEngine;
using UnityEngine.UI; // Necesario para Image y Text
using TMPro; // Necesario si usas TextMeshProUGUI
using System.Collections.Generic; // Necesario para List<T>

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }
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
    [SerializeField] private GameObject effectActiveIndicator;   // Asigna tu cuadrado naranja aquí (para el efecto)
    [SerializeField] private TextMeshProUGUI effectCountText;    // Asigna aquí un componente TextMeshProUGUI para el contador de efectos

    public bool HasWeaponEquipped
    {
        get { return _hasWeaponEquipped; }
        private set
        {
            _hasWeaponEquipped = value;
            if (weaponEquippedIndicator != null)
            {
                weaponEquippedIndicator.SetActive(_hasWeaponEquipped);
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
        Instance = this;/* 
        CurrentHealth = maxHealth;
        InitializeHeartUI();
        UpdateHeartUI(); */
    }

    void OnDisable()
    {
        // Desuscribirse del evento para evitar errores cuando el objeto se desactiva o destruye
        if (TurnManager.Instance != null)
        {
            TurnManager.OnTurnStart -= OnTurnStartHandler;
        }
    }

    private void Start()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.OnTurnStart += OnTurnStartHandler;
            /*             Debug.Log("[PlayerStats] Suscrito a TurnManager.OnTurnStart en Start()."); */
        }
        else
        {
            Debug.LogError("[PlayerStats] TurnManager.Instance es null en Start(). La suscripción al evento de inicio de turno no ocurrirá.");
        }

        CurrentHealth = maxHealth;
        InitializeHeartUI();
        UpdateHeartUI(); 

        // Inicializa el estado al comienzo del juego
        HasWeaponEquipped = false; // Asegura que no hay arma equipada al inicio
        CurrentEquippedWeapon = null; //limpiar la referencia del arma
        _activeEffectCount = 0; // Asegura que el contador de efectos está en 0
        UpdateEffectDisplay(); // Actualiza la UI de efectos (ocultando el indicador y poniendo el contador a 0)
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            TakeDamage(1);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("[Test] Intentando curar al jugador desde Update con Heal(1)");
            Heal(1);
        }
    }


    /// Manejador para el evento OnTurnStart de TurnManager.
    /// Reinicia los efectos activos al inicio de cada turno.
    private void OnTurnStartHandler(int turnNumber)
    {
        // ...
        ResetRevolverFired();
        ClearAllEffects();
        _hasPlayedEffectCardThisTurn = false;
        Debug.Log($"[PlayerStats] OnTurnStartHandler: HasPlayedEffectCardThisTurn se ha reseteado a FALSE. Estado actual: {_hasPlayedEffectCardThisTurn}");
        Debug.Log($"[PlayerStats] Inicio de turno {turnNumber}. Efectos reiniciados. Se puede jugar una carta de efecto.");
    }

    /// Establece que el jugador tiene un arma equipada y almacena la CardData del arma.
    /// Este método es llamado por DropTarget cuando se juega una carta de tipo Weapon.
    public void EquipWeapon(CardData weaponCard) //Ahora recibe CardData
    {
        if (weaponCard == null)
        {
            Debug.LogError("[PlayerStats] Intentando equipar un arma nula.");
            return;
        }

        if (!HasWeaponEquipped)
        {
            HasWeaponEquipped = true; // Activa el indicador de arma
            CurrentEquippedWeapon = weaponCard; //Guarda la CardData del arma
            Debug.Log($"[PlayerStats] Arma '{weaponCard.cardID}' equipada.");
        }
        else
        {
            Debug.LogWarning($"[PlayerStats] Intentando equipar arma '{weaponCard.cardID}', pero el jugador ya tiene equipada: {CurrentEquippedWeapon.cardID}");
        }
    }

    /// Desequipa el arma y limpia la referencia a su CardData.
    public void UnequipWeapon()
    {
        if (HasWeaponEquipped)
        {
            HasWeaponEquipped = false; // Desactiva el indicador de arma
            CurrentEquippedWeapon = null; //Limpia la referencia al arma
            Debug.Log("[PlayerStats] Arma desequipada.");
        }
        else
        {
            Debug.LogWarning("[PlayerStats] Intentando desequipar arma, pero el jugador no tiene ninguna equipada.");
        }
    }

    /// Incrementa el contador de efectos activos y actualiza la UI.
    /// Este método es llamado desde DropTarget cuando una carta de efecto sea jugada.
    public void ActivateEffect()
    {
        _hasPlayedEffectCardThisTurn = true; 

        Debug.Log($"[PlayerStats] HasPlayedEffectCardThisTurn se ha puesto a TRUE después de activar un efecto. Estado actual: {_hasPlayedEffectCardThisTurn}");

        _activeEffectCount++; // Incrementa el contador
        Debug.Log($"[PlayerStats] Efecto activado. Contador de efectos: {_activeEffectCount}");
        UpdateEffectDisplay(); // Actualiza la UI para mostrar el nuevo conteo y la visibilidad.
    }

    /// Disminuye el contador de efectos activos y actualiza la UI.
    /// (Puedes llamar a esto si un efecto tiene una duración limitada o se consume)
    public void DeactivateEffect()
    {
        if (_activeEffectCount > 0)
        {
            _activeEffectCount--; // Decrementa el contador
            Debug.Log($"[PlayerStats] Efecto desactivado. Contador de efectos: {_activeEffectCount}");
            UpdateEffectDisplay(); // Actualiza la UI
        }
        else
        {
            Debug.LogWarning("[PlayerStats] Intentando desactivar efecto, pero el contador ya está en cero.");
        }
    }

    /// Reinicia el contador de efectos activos a cero y actualiza la UI.
    /// Este método es llamado por TurnManager al inicio de cada turno, y también se puede llamar manualmente.
    public void ClearAllEffects()
    {
        if (_activeEffectCount > 0) // Solo hacer algo si hay efectos activos que limpiar
        {
            Debug.Log($"[PlayerStats] Limpiando {_activeEffectCount} efectos activos.");
            _activeEffectCount = 0; // Reinicia el contador a cero
        }
        /*         else
                {
                    Debug.Log("[PlayerStats] Se intentó limpiar efectos, pero no había ninguno activo.");
                } */
        UpdateEffectDisplay(); // Asegura que la UI refleje el contador en cero y oculte el indicador.
    }

    /// Actualiza la visibilidad del indicador de efecto y el texto del contador.
    private void UpdateEffectDisplay()
    {
        // El indicador de efecto (cuadrado naranja) solo se activa si hay al menos 1 efecto.
        if (effectActiveIndicator != null)
        {
            bool shouldBeActive = _activeEffectCount > 0;
            if (effectActiveIndicator.activeSelf != shouldBeActive) // Solo cambiar si la visibilidad es diferente
            {
                effectActiveIndicator.SetActive(shouldBeActive);
                Debug.Log($"[PlayerStats] Cuadradito naranja: Se {(shouldBeActive ? "activó" : "desactivó")} visualmente.");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerStats] El indicador de efecto (GameObject) NO ESTÁ ASIGNADO en el Inspector del PlayerStats.");
        }

        // Actualiza el texto del contador de efectos
        if (effectCountText != null)
        {
            effectCountText.text = _activeEffectCount.ToString();
            /*             Debug.Log($"[PlayerStats] Texto del contador de efectos actualizado a: {_activeEffectCount}"); */
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
        
        // Se intenta usar la Biblia si está en la mano del jugador y el CardManager la procesa.
        if (CardManager.Instance != null && CardManager.Instance.AttemptUseBibleCard())
        {
            Debug.Log("[PlayerStats] ¡La Biblia te ha salvado! Recuperando vida.");
            UpdateHeartUI(); // Asegura que la UI se actualice inmediatamente.
            return; // ¡Importante! Si la Biblia salva, el jugador NO muere, así que salimos del método.
        }
        

        // Si la Biblia no estaba en la mano o no se pudo usar, entonces el jugador muere.
        CurrentHealth = 0; // Asegura que la vida se establezca a 0 si no revivió.
        Debug.LogWarning("[PlayerStats] ¡El jugador ha muerto!");
        // Aquí puedes poner lógica de Game Over, reinicio, etc.
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

                // Para UI (Image)
                Image img = activeHearts[i].GetComponent<Image>();
                if (img != null)
                    img.color = isFull ? Color.red : Color.gray;

                // Para mundo (SpriteRenderer)
                SpriteRenderer sr = activeHearts[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = isFull ? Color.red : Color.gray;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (CardManager.Instance != null && CardManager.Instance.AttemptUseCoverCard())
        {
            Debug.Log("[PlayerStats] ¡Daño bloqueado por la carta Cover!");
            // Si la carta Cover se usó, el daño NO se aplica, y salimos del método.
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
        return true; // De momento, siempre puede ser dañado.
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

/*     public void HealWithBeer()
    {
        Debug.Log("[PlayerStats] Se ha usado una Cerveza. Intentando curar 1 vida.");
        Heal(1);
    }
 */
}