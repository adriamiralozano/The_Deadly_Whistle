using UnityEngine;

public class PlayerVisualManager : MonoBehaviour
{
    [Header("Sprites del Jugador")]
    [SerializeField] private Sprite idlePlayerSprite;
    [SerializeField] private Sprite playerRevolverEquippedSprite;
    [SerializeField] private Sprite playerRevolverShotSprite;

    private SpriteRenderer playerSpriteRenderer;

    private void Awake()
    {
        playerSpriteRenderer = GetComponent<SpriteRenderer>();

        if (playerSpriteRenderer == null)
        {
            Debug.LogError("PlayerVisualManager: No se encontró un SpriteRenderer en este GameObject. ¡Asegúrate de que 'PlayerCharacter' tiene un SpriteRenderer!");
            return;
        }

        SetIdleSprite();    // Establece el sprite inicial del jugador a "idle"
    }

    // --- NUEVO: Suscripción y Desuscripción a Eventos ---
    private void OnEnable()
    {
        // Nos suscribimos al evento de PlayerStats
        PlayerStats.OnWeaponEquippedStatusChanged += OnWeaponStatusChanged;
        Debug.Log("[PlayerVisualManager] Suscrito a PlayerStats.OnWeaponEquippedStatusChanged.");
    }

    private void OnDisable()
    {
        // Es crucial desuscribirse cuando el GameObject se desactiva o destruye
        PlayerStats.OnWeaponEquippedStatusChanged -= OnWeaponStatusChanged;
        Debug.Log("[PlayerVisualManager] Desuscrito de PlayerStats.OnWeaponEquippedStatusChanged.");
    }

    /// Manejador del evento que se dispara cuando el estado del arma equipada del jugador cambia.

    private void OnWeaponStatusChanged(bool hasWeapon)
    {
        if (hasWeapon)
        {
            SetRevolverEquippedSprite();
            Debug.Log("[PlayerVisualManager] Recibido evento: Arma EQUipada. Cambiando sprite.");

            // --- NUEVO: Reproducir el sonido del arma equipada ---
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWeaponEquipped();
                Debug.Log("[PlayerVisualManager] Reproduciendo sonido de arma equipada.");
            }
            else
            {
                Debug.LogWarning("[PlayerVisualManager] AudioManager.Instance es nulo. No se puede reproducir el sonido de arma equipada.");
            }
            // ----------------------------------------------------
        }
        else
        {
            SetIdleSprite();
            Debug.Log("[PlayerVisualManager] Recibido evento: Arma DESEQUipada. Cambiando sprite.");
            // Opcional: Podrías añadir un sonido para desequipar aquí si lo deseas
        }
    }
    /// Establece el sprite del jugador a la versión "idle" (sin arma).
    public void SetIdleSprite()
    {
        if (playerSpriteRenderer != null && idlePlayerSprite != null)
        {
            playerSpriteRenderer.sprite = idlePlayerSprite;
        }
        else if (playerSpriteRenderer != null)
        {
            Debug.LogWarning("PlayerVisualManager: El sprite 'Idle Player' no está asignado. No se puede mostrar.");
        }
    }

    /// Establece el sprite del jugador a la versión con el revólver equipado.
    public void SetRevolverEquippedSprite()
    {
        if (playerSpriteRenderer != null && playerRevolverEquippedSprite != null)
        {
            playerSpriteRenderer.sprite = playerRevolverEquippedSprite;
        }
        else if (playerSpriteRenderer != null)
        {
            Debug.LogWarning("PlayerVisualManager: El sprite 'Player Revolver Equipped' no está asignado. No se puede mostrar.");
        }
    }
    
    public void SetRevolverShotSprite()
    {
        if (playerSpriteRenderer != null && playerRevolverShotSprite != null)
        {
            playerSpriteRenderer.sprite = playerRevolverShotSprite;
        }
        else if (playerSpriteRenderer != null)
        {
            Debug.LogWarning("PlayerVisualManager: El sprite 'Player Revolver Shot' no está asignado. No se puede mostrar.");
        }
    }
}