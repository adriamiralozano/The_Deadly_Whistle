using UnityEngine;

public class PlayerVisualManager : MonoBehaviour
{
    [Header("Sprites del Jugador")]
    [SerializeField] private Sprite idlePlayerSprite;
    [SerializeField] private Sprite playerRevolverEquippedSprite;
    [SerializeField] private Sprite playerRevolverShotSprite;
    [SerializeField] private Sprite playerShotedSprite;

    private SpriteRenderer playerSpriteRenderer;

    private void Awake()
    {
        playerSpriteRenderer = GetComponent<SpriteRenderer>();

        if (playerSpriteRenderer == null)
        {
            Debug.LogError("PlayerVisualManager: No se encontró un SpriteRenderer en este GameObject. ¡Asegúrate de que 'PlayerCharacter' tiene un SpriteRenderer!");
            return;
        }

        SetIdleSprite();
    }

    // ---  Suscripción y Desuscripción a Eventos ---
    private void OnEnable()
    {
        PlayerStats.OnWeaponEquippedStatusChanged += OnWeaponStatusChanged;
        Debug.Log("[PlayerVisualManager] Suscrito a PlayerStats.OnWeaponEquippedStatusChanged.");
    }

    private void OnDisable()
    {
        PlayerStats.OnWeaponEquippedStatusChanged -= OnWeaponStatusChanged;
        Debug.Log("[PlayerVisualManager] Desuscrito de PlayerStats.OnWeaponEquippedStatusChanged.");
    }

    private void OnWeaponStatusChanged(bool hasWeapon)
    {
        if (hasWeapon)
        {
            SetRevolverEquippedSprite();
            Debug.Log("[PlayerVisualManager] Recibido evento: Arma EQUipada. Cambiando sprite.");

            // --- Reproducir el sonido del arma equipada ---
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
        }
    }
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
    
    public void SetPlayerShotedSprite()
    {
        if (playerSpriteRenderer != null && playerShotedSprite != null)
            playerSpriteRenderer.sprite = playerShotedSprite;
        else
            Debug.LogWarning("[PlayerVisualManager] playerShotedSprite no asignado.");
    }
}