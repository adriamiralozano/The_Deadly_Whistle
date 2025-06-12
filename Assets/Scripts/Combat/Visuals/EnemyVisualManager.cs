// EnemyVisualManager.cs
using UnityEngine;
using System; // Necesario para Action

public class EnemyVisualManager : MonoBehaviour
{
    [Header("Sprite References")]
    [SerializeField] private Sprite idleEnemySprite;
    [SerializeField] private Sprite equippedEnemySprite;
    [SerializeField] private Sprite shotedEnemySprite;
    [SerializeField] private Sprite enemyRevolverShotSprite;

    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("[EnemyVisualManager] No se encontró SpriteRenderer en este GameObject. Por favor, añade uno.");
            enabled = false; // Deshabilita el script si no hay SpriteRenderer
            return;
        }

        // Establecer el sprite inicial (opcional, el evento de la IA lo sobrescribirá)
        SetIdleEnemySprite();
    }

    void OnEnable()
    {
        // Suscribirse al evento cuando este GameObject esté activo
        OutlawEnemyAI.OnEnemyWeaponStatusChanged += OnEnemyWeaponStatusChanged;
        Debug.Log("[EnemyVisualManager] Suscrito a OnEnemyWeaponStatusChanged.");
    }

    void OnDisable()
    {
        // Desuscribirse del evento cuando este GameObject se desactive para evitar errores
        OutlawEnemyAI.OnEnemyWeaponStatusChanged -= OnEnemyWeaponStatusChanged;
        Debug.Log("[EnemyVisualManager] Desuscrito de OnEnemyWeaponStatusChanged.");
    }
    /// Manejador del evento que se dispara cuando el estado del arma equipada del enemigo cambia.
    private void OnEnemyWeaponStatusChanged(bool hasWeapon)
    {
        if (hasWeapon)
        {
            SetEquippedEnemySprite();
            Debug.Log("[EnemyVisualManager] Recibido evento: Enemigo Arma EQUipada. Cambiando sprite.");


            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyWeaponEquipped(); // Necesitarás crear este método en AudioManager
            }

        }
        else
        {
            SetIdleEnemySprite();
            Debug.Log("[EnemyVisualManager] Recibido evento: Enemigo Arma DESEQUipada. Cambiando sprite.");

            // Opcional: Reproducir sonido de desequipar arma del enemigo aquí
        }
    }
    /// Establece el sprite del enemigo a su versión "idle" (sin arma).
    public void SetIdleEnemySprite()
    {
        if (spriteRenderer != null && idleEnemySprite != null)
        {
            spriteRenderer.sprite = idleEnemySprite;
        }
        else if (idleEnemySprite == null)
        {
            Debug.LogWarning("[EnemyVisualManager] Sprite 'Idle Enemy' no asignado en el Inspector.");
        }
    }
    /// Establece el sprite del enemigo a su versión "equipada" (con arma).
    public void SetEquippedEnemySprite()
    {
        if (spriteRenderer != null && equippedEnemySprite != null)
        {
            spriteRenderer.sprite = equippedEnemySprite;
        }
        else if (equippedEnemySprite == null)
        {
            Debug.LogWarning("[EnemyVisualManager] Sprite 'Equipped Enemy' no asignado en el Inspector.");
        }
    }

    public void SetShotedEnemySprite()
    {
        if (spriteRenderer != null && shotedEnemySprite != null)
            spriteRenderer.sprite = shotedEnemySprite;
        else
            Debug.LogWarning("[EnemyVisualManager] shotedEnemySprite no asignado.");
    }

    public void SetEnemyRevolverShotSprite()
    {
        if (spriteRenderer != null && enemyRevolverShotSprite != null)
            spriteRenderer.sprite = enemyRevolverShotSprite;
        else
            Debug.LogWarning("[EnemyVisualManager] enemyRevolverShotSprite no asignado.");
    }
    
    public void RestoreOriginalSprite()
    {
        if (spriteRenderer != null && originalSprite != null)
            spriteRenderer.sprite = originalSprite;
    }
    // Puedes añadir métodos para animaciones, efectos, etc. aquí en el futuro.
}