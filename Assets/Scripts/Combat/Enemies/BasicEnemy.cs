using UnityEngine;
using System;
using System.Collections.Generic; // Necesario para List

public class Enemy : MonoBehaviour
{
    // Asigna tu ScriptableObject de EnemyData aquí en el Inspector del Prefab del enemigo.
    [SerializeField] private EnemyData _enemyData;

    // Propiedad pública para acceder a los datos del enemigo.
    public EnemyData Data => _enemyData;

    // La vida actual del enemigo, que corresponde al número de corazones.
    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;

    // --- Configuración para la UI de Corazones ---
    [Header("UI Corazones")]
    [Tooltip("Prefab del cuadrado/corazón visual. Debe tener un SpriteRenderer o Image.")]
    [SerializeField] private GameObject heartUIPrefab;
    [Tooltip("Transform padre donde se instanciarán los corazones. Debe estar encima del enemigo.")]
    [SerializeField] private Transform heartUIParent;
    private List<GameObject> activeHearts = new List<GameObject>(); // Lista para gestionar los GameObjects de los corazones.
    // ------------------------------------------

    // --- Referencia al componente de IA ---
    private IEnemyAI _enemyAI; // Este será el 'cerebro' del enemigo
    // --- FIN NUEVO ---

    // --- Eventos (para que otros scripts puedan reaccionar a la vida del enemigo) ---
    public static event Action<Enemy, int> OnEnemyTookDamage;
    public static event Action<Enemy> OnEnemyDied;
    public static event Action<Enemy, int> OnEnemyHealthChanged;
    // --------------------------------------------------------------------------------

    protected virtual void Awake()
    {
        // Verifica si se ha asignado un EnemyData. Es crucial para que el enemigo funcione.
        if (_enemyData == null)
        {
            Debug.LogError($"El enemigo '{name}' no tiene asignado un EnemyData ScriptableObject en su Inspector.", this);
            return;
        }

        // Inicializa la vida actual con la vida máxima definida en el EnemyData.
        CurrentHealth = _enemyData.maxHealth;
        Debug.Log($"Enemigo '{_enemyData.enemyName}' inicializado con {CurrentHealth} corazón/es de vida.");

        // --- Buscar e Inicializar el componente de IA ---
        _enemyAI = GetComponent<IEnemyAI>(); // Intenta obtener el componente de IA en este GameObject
        if (_enemyAI == null)
        {
            // Si no se encuentra aquí, verifica en sus hijos (por si la IA está en un GameObject anidado)
            _enemyAI = GetComponentInChildren<IEnemyAI>();
        }

        if (_enemyAI == null)
        {
            Debug.LogWarning($"[Enemy] No se encontró ningún componente que implemente IEnemyAI en '{name}' o sus hijos. El enemigo no tendrá comportamiento de IA.", this);
        }
        else
        {
            _enemyAI.Initialize(this); // Pasa una referencia a esta instancia de Enemy a la IA
        }
        // --- FIN ACTUALIZADO ---

        // Inicializa la representación visual de los corazones.
        InitializeHeartUI();
    }

    /// Método para que el enemigo reciba daño. Cada punto de daño quita un corazón.
    public virtual void TakeDamage(int amount)
    {
        int damageTaken = amount;

        if (CurrentHealth - damageTaken < 0)
        {
            damageTaken = CurrentHealth;
        }

        CurrentHealth -= damageTaken;

        Debug.Log($"{_enemyData.enemyName} recibió {damageTaken} de daño. HP restante: {CurrentHealth} corazón/es.");
        OnEnemyTookDamage?.Invoke(this, damageTaken); 

        UpdateHeartUI();
        OnEnemyHealthChanged?.Invoke(this, CurrentHealth); 

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    /// Lógica cuando el enemigo es derrotado.
    protected virtual void Die()
    {
        CurrentHealth = 0; 
        Debug.LogWarning($"{_enemyData.enemyName} HA SIDO DERROTADO.");
        OnEnemyDied?.Invoke(this); 

        // Deshabilitar el componente de IA cuando el enemigo muere
        if (_enemyAI is MonoBehaviour aiMonoBehaviour) 
        {
            aiMonoBehaviour.enabled = false;
        }
    }

    /// Método para que el enemigo realice su acción de turno.
    /// Delega la lógica de comportamiento al componente de IA.
    public virtual void PerformTurnAction() // Ya no recibe CardManager ni PlayerStats directamente aquí
    {
        if (_enemyAI != null && (_enemyAI is MonoBehaviour aiMonoBehaviour && aiMonoBehaviour.enabled))
        {
            _enemyAI.PerformTurnAction(); // Llama al método de la IA
        }
        else
        {
            Debug.LogWarning($"[{_enemyData.enemyName}] No hay comportamiento de IA válido o está deshabilitado. No se realizará ninguna acción de IA.");
        }
    }

    /// Instancia los GameObjects de los corazones basándose en la vida máxima.
    private void InitializeHeartUI()
    {
        if (heartUIPrefab == null || heartUIParent == null)
        {
            Debug.LogWarning($"[Enemy] Heart UI Prefab o Heart UI Parent no asignado en el Inspector de '{name}'. Los corazones de vida no se mostrarán.", this);
            return;
        }

        foreach (var heart in activeHearts)
        {
            Destroy(heart);
        }
        activeHearts.Clear();

        for (int i = 0; i < _enemyData.maxHealth; i++)
        {
            GameObject newHeart = Instantiate(heartUIPrefab, heartUIParent);
            newHeart.name = $"Heart_{i}";
            activeHearts.Add(newHeart);
        }
    }

    /// Actualiza el color de los corazones según la vida actual del enemigo.
    private void UpdateHeartUI()
    {
        for (int i = 0; i < activeHearts.Count; i++)
        {
            if (activeHearts[i] != null)
            {
                bool isFull = i < CurrentHealth;
                SpriteRenderer sr = activeHearts[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = isFull ? Color.white : Color.gray;
                }

                UnityEngine.UI.Image img = activeHearts[i].GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    img.color = isFull ? Color.white : Color.gray;
                }
            }
        }
    }

    /// Intenta disparar al jugador. Este método es llamado por la IA.
    public virtual void TryShootPlayer()
    {
        if (PlayerStats.Instance != null && PlayerStats.Instance.CanBeDamaged())
        {
            PlayerStats.Instance.TakeDamage(1);
            Debug.Log($"[{Data.enemyName}] Disparó al jugador (pierde 1 vida).");
        }
        else
        {
            Debug.Log($"[{Data.enemyName}] Intentó disparar, pero el jugador no puede ser dañado o PlayerStats no encontrado.");
        }
    }

    public void Heal(int amount)
    {
        if (_enemyData == null)
        {
            Debug.LogError($"[Enemy] The enemy '{name}' has no EnemyData assigned. Cannot heal.", this);
            return;
        }

        if (CurrentHealth < _enemyData.maxHealth)
        {
            int healAmount = Mathf.Min(amount, _enemyData.maxHealth - CurrentHealth);
            CurrentHealth += healAmount;

            Debug.Log($"[{_enemyData.enemyName}] healed for {healAmount} health point(s). Current HP: {CurrentHealth} heart(s).");
            UpdateHeartUI();
            OnEnemyHealthChanged?.Invoke(this, CurrentHealth);
        }
        else
        {
            Debug.Log($"[{_enemyData.enemyName}] already has max health ({_enemyData.maxHealth}/{_enemyData.maxHealth}). Cannot heal further.");
        }
    }
    
}