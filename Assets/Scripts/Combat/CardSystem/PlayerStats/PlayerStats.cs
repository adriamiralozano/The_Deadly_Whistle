// PlayerStats.cs
using UnityEngine;
using UnityEngine.UI; // Necesario para Image y Text
using TMPro; // Necesario si usas TextMeshProUGUI

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    // --- Indicador de Arma Equipada ---
    private bool _hasWeaponEquipped = false;

    // NUEVO: Propiedad para almacenar la CardData del arma actualmente equipada
    public CardData CurrentEquippedWeapon { get; private set; } // <--- AÑADE ESTA LÍNEA AQUÍ

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

        // Inicializa el estado al comienzo del juego
        HasWeaponEquipped = false; // Asegura que no hay arma equipada al inicio
        CurrentEquippedWeapon = null; // <--- AÑADE ESTA LÍNEA para limpiar la referencia del arma
        _activeEffectCount = 0; // Asegura que el contador de efectos está en 0
        UpdateEffectDisplay(); // Actualiza la UI de efectos (ocultando el indicador y poniendo el contador a 0)
    }

    /// <summary>
    /// Manejador para el evento OnTurnStart de TurnManager.
    /// Reinicia los efectos activos al inicio de cada turno.
    /// </summary>
    /// <param name="turnNumber">El número del turno que comienza.</param>
    private void OnTurnStartHandler(int turnNumber)
    {
        // Al final de cada turno, reiniciamos el contador de efectos.
        ClearAllEffects();
/*         Debug.Log($"[PlayerStats] Turno {turnNumber} iniciado. Contador de efectos reiniciado."); */
    }

    /// <summary>
    /// Establece que el jugador tiene un arma equipada y almacena la CardData del arma.
    /// Este método es llamado por DropTarget cuando se juega una carta de tipo Weapon.
    /// </summary>
    public void EquipWeapon(CardData weaponCard) // <--- MODIFICADO: Ahora recibe CardData
    {
        if (weaponCard == null)
        {
            Debug.LogError("[PlayerStats] Intentando equipar un arma nula.");
            return;
        }

        if (!HasWeaponEquipped)
        {
            HasWeaponEquipped = true; // Activa el indicador de arma
            CurrentEquippedWeapon = weaponCard; // <--- AÑADIDO: Guarda la CardData del arma
            Debug.Log($"[PlayerStats] Arma '{weaponCard.cardID}' equipada.");
        }
        else
        {
            Debug.LogWarning($"[PlayerStats] Intentando equipar arma '{weaponCard.cardID}', pero el jugador ya tiene equipada: {CurrentEquippedWeapon.cardID}");
            // Opcional: Aquí podrías añadir lógica para reemplazar el arma si se desea
        }
    }

    /// <summary>
    /// Desequipa el arma y limpia la referencia a su CardData.
    /// </summary>
    public void UnequipWeapon()
    {
        if (HasWeaponEquipped)
        {
            HasWeaponEquipped = false; // Desactiva el indicador de arma
            CurrentEquippedWeapon = null; // <--- AÑADIDO: Limpia la referencia al arma
            Debug.Log("[PlayerStats] Arma desequipada.");
        }
        else
        {
            Debug.LogWarning("[PlayerStats] Intentando desequipar arma, pero el jugador no tiene ninguna equipada.");
        }
    }

    /// <summary>
    /// Incrementa el contador de efectos activos y actualiza la UI.
    /// Este método es llamado desde DropTarget cuando una carta de efecto sea jugada.
    /// </summary>
    public void ActivateEffect()
    {
        _activeEffectCount++; // Incrementa el contador
        Debug.Log($"[PlayerStats] Efecto activado. Contador de efectos: {_activeEffectCount}");
        UpdateEffectDisplay(); // Actualiza la UI para mostrar el nuevo conteo y la visibilidad.
    }

    /// <summary>
    /// Disminuye el contador de efectos activos y actualiza la UI.
    /// (Puedes llamar a esto si un efecto tiene una duración limitada o se consume)
    /// </summary>
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

    /// <summary>
    /// Reinicia el contador de efectos activos a cero y actualiza la UI.
    /// Este método es llamado por TurnManager al inicio de cada turno, y también se puede llamar manualmente.
    /// </summary>
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

    /// <summary>
    /// Actualiza la visibilidad del indicador de efecto y el texto del contador.
    /// </summary>
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
}