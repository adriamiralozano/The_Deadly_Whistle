using UnityEngine;
using System; 

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
            enabled = false; 
            return;
        }
        SetIdleEnemySprite();
    }

    void OnEnable()
    {
        OutlawEnemyAI.OnEnemyWeaponStatusChanged += OnEnemyWeaponStatusChanged;
        Debug.Log("[EnemyVisualManager] Suscrito a OnEnemyWeaponStatusChanged.");
    }

    void OnDisable()
    {
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
                AudioManager.Instance.PlayEnemyWeaponEquipped();
            }

        }
        else
        {
            SetIdleEnemySprite();
            Debug.Log("[EnemyVisualManager] Recibido evento: Enemigo Arma DESEQUipada. Cambiando sprite.");

        }
    }
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
}