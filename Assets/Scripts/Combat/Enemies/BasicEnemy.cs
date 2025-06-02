// Scripts/Enemy/Enemy.cs
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

        // Inicializa la representación visual de los corazones.
        InitializeHeartUI();
    }

    /// <summary>
    /// Método para que el enemigo reciba daño. Cada punto de daño quita un corazón.
    /// </summary>
    /// <param name="amount">Cantidad de daño a infligir (cada punto de daño quita un corazón).</param>
    public virtual void TakeDamage(int amount)
    {
        // En este sistema, cada punto de daño se traduce directamente en la pérdida de un corazón.
        int damageTaken = amount;

        // Asegura que no se quite más vida de la que queda.
        if (CurrentHealth - damageTaken < 0)
        {
            damageTaken = CurrentHealth;
        }

        CurrentHealth -= damageTaken;

        Debug.Log($"{_enemyData.enemyName} recibió {damageTaken} de daño. HP restante: {CurrentHealth} corazón/es.");
        OnEnemyTookDamage?.Invoke(this, damageTaken); // Dispara el evento de daño.

        // Actualiza el color de los corazones en la UI.
        UpdateHeartUI();
        OnEnemyHealthChanged?.Invoke(this, CurrentHealth); // Dispara el evento de cambio de vida para la UI.

        // Si la vida llega a 0 o menos, el enemigo "muere".
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Lógica cuando el enemigo es derrotado.
    /// Actualmente, solo lo loguea en consola y no destruye el GameObject.
    /// </summary>
    protected virtual void Die()
    {
        CurrentHealth = 0; // Asegura que la salud no sea negativa.
        // Log para indicar que el enemigo debería haber muerto.
        Debug.LogWarning($"{_enemyData.enemyName} HA SIDO DERROTADO (pero el GameObject NO se destruye por ahora para propósitos de prueba).");
        OnEnemyDied?.Invoke(this); // Dispara el evento de muerte.

        // Para hacer que el enemigo "desaparezca" visualmente sin destruir el GameObject:
        // gameObject.SetActive(false); // Esto desactiva el GameObject completo.
        // Si tienes un SpriteRenderer y un Collider, podrías desactivarlos individualmente:
        // SpriteRenderer sr = GetComponent<SpriteRenderer>();
        // if (sr != null) sr.enabled = false;
        // Collider2D col = GetComponent<Collider2D>(); // O Collider para 3D
        // if (col != null) col.enabled = false;
    }

    /// <summary>
    /// Método para que el enemigo realice su acción en su turno.
    /// Las clases derivadas pueden sobrescribirlo para comportamientos específicos.
    /// </summary>
    public virtual void PerformTurnAction()
    {
        Debug.Log($"{_enemyData.enemyName} está realizando su acción de turno base.");
        // Aquí iría la lógica de comportamiento por defecto del enemigo, si la tiene.
        // Por ejemplo, un simple ataque al jugador si está en rango, o moverse.
    }

    /// <summary>
    /// Instancia los GameObjects de los corazones basándose en la vida máxima.
    /// </summary>
    private void InitializeHeartUI()
    {
        if (heartUIPrefab == null || heartUIParent == null)
        {
            Debug.LogWarning($"[Enemy] Heart UI Prefab o Heart UI Parent no asignado en el Inspector de '{name}'. Los corazones de vida no se mostrarán.", this);
            return;
        }

        // Limpia cualquier corazón previo si el método se llama de nuevo (útil para pruebas).
        foreach (var heart in activeHearts)
        {
            Destroy(heart);
        }
        activeHearts.Clear();

        // Instancia un GameObject de corazón por cada punto de vida máxima.
        for (int i = 0; i < _enemyData.maxHealth; i++)
        {
            GameObject newHeart = Instantiate(heartUIPrefab, heartUIParent);
            newHeart.name = $"Heart_{i}"; // Nombra los corazones para fácil identificación.
            activeHearts.Add(newHeart);
            // Por defecto, todos los corazones inician visibles y "llenos" (rojos).
        }
    }

    /// <summary>
    /// Actualiza el color de los corazones según la vida actual del enemigo.
    /// </summary>
    private void UpdateHeartUI()
    {
        // Itera sobre los corazones instanciados.
        for (int i = 0; i < activeHearts.Count; i++)
        {
            if (activeHearts[i] != null)
            {
                // Un corazón está "lleno" (rojo) si su índice es menor que la vida actual.
                // Un corazón está "vacío" (gris) si su índice es igual o mayor que la vida actual.
                bool isFull = i < CurrentHealth;

                // Intenta obtener el SpriteRenderer (para cuadrados 2D en el mundo).
                SpriteRenderer sr = activeHearts[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = isFull ? Color.red : Color.gray; // Cambia el color a rojo o gris.
                }

                // Intenta obtener el Image (para elementos UI en un Canvas).
                // Asegúrate de tener 'using UnityEngine.UI;' si usas esto.
                UnityEngine.UI.Image img = activeHearts[i].GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    img.color = isFull ? Color.red : Color.gray; // Cambia el color a rojo o gris.
                }
            }
        }
    }
    
    public virtual void TryShootPlayer()
    {
        if (PlayerStats.Instance != null && PlayerStats.Instance.CanBeDamaged())
        {
            PlayerStats.Instance.TakeDamage(1);
            Debug.Log($"[{Data.enemyName}] Disparó al jugador (pierde 1 vida).");
        }
        else
        {
            Debug.Log($"[{Data.enemyName}] Intentó disparar, pero el jugador no puede ser dañado.");
        }
    }
}