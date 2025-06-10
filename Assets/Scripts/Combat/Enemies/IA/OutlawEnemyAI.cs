using UnityEngine;
using Random = UnityEngine.Random; // Para evitar conflictos si se usa System.Random
using System;

// Importante: Este script DEBE implementar la interfaz IEnemyAI
public class OutlawEnemyAI : MonoBehaviour, IEnemyAI
{
    private Enemy _enemyInstance; // Una referencia al componente Enemy en el mismo GameObject
    public static event Action<bool> OnEnemyWeaponStatusChanged; // Evento para notificar el estado del arma del enemigo

    [Header("AI Settings")]
     // Porcentaje de probabilidad de que el disparo falle
    [SerializeField] private int equipWeaponChancePercentage = 70; // Porcentaje de probabilidad de que el enemigo equipe un arma al inicio del turno
    [SerializeField] private int disarmSuccessChancePercentage = 5; // Porcentaje de probabilidad de que el enemigo desarme al jugador
    [SerializeField] private int disarmPlayerCooldown = 2; // Número de turnos que debe esperar el enemigo antes de intentar desarmar al jugador nuevamente

    [Header("Healing Settings")]
    [SerializeField] private int healChancePercentage = 40; // Probabilidad de curarse si la vida está baja
    [SerializeField] private int healthThresholdForHealing = 3; // Umbral de vida para decidir curarse (por ejemplo, si la vida actual es menor o igual a este valor)
    [SerializeField] private int healAmount = 1; // Cantidad de vida que se cura
    
    private int shootMissChancePercentage = 20; // Porcentaje de probabilidad de que el disparo falle
    private bool weaponEquipped = false; // Indica si el enemigo tiene un arma equipada
    private int disarmPlayerCounter = 0; // Contador para el número de desarmes al jugador
    private int shotsPerTurn = 3;   // Número de disparos que el enemigo puede hacer por turno
    private bool moreThanOneShot = false; // Indica si el enemigo ha fallado más de un disparo en el turno
    private bool EnemyHasBeenDisarmed = false; // Indica si el enemigo ha sido desarmado por el jugador
    private bool EnemyEffectCardUsed = false;   


    public bool HasWeaponEquipped => weaponEquipped;

    public void Initialize(Enemy enemyInstance)
    {
        _enemyInstance = enemyInstance;
        Debug.Log($"[OutlawEnemyAI] IA inicializada para: {_enemyInstance.Data.enemyName}");

        OnEnemyWeaponStatusChanged?.Invoke(weaponEquipped);
        Debug.Log($"[OutlawEnemyAI] Disparando evento de estado de arma al inicializar: {weaponEquipped}");
    }

    // Este método se llamará al inicio del turno del enemigo.
    // Aquí es donde la IA evalúa el estado del juego y decide qué hacer.
    public void PerformTurnAction()
    {
        if (_enemyInstance != null && _enemyInstance.IsAlive)
        {

            if (!weaponEquipped)
            {
                Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} va a intentar equipar un arma.");
                TryEquipWeapon();
            }


            if (ShouldHeal() && EnemyEffectCardUsed == false)
            {
                Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} ha decidido curarse.");
                DecidesToHeal();
            }

            if (disarmPlayerCounter < disarmPlayerCooldown)
            {
                Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} va a intentar desarmar al jugador.");
                TryDisarmPlayer();
            }

            if (weaponEquipped)
            {
                Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} tiene un arma equipada y va a disparar.");
                DecidesToShoot();
            }

            EnemyEffectCardUsed = false; // Resetea la marca de uso de carta de efecto al final del turno
        }
        else
        {
            Debug.LogWarning($"[OutlawEnemyAI] No se puede realizar la acción de turno. Enemigo nulo o no vivo.");
        }
    }

    //-------------------ACCIONES DE LA IA -------------------//
    private bool ShouldMissShot()
    {
        return GetRandomNum() < shootMissChancePercentage;

    }

    private void DecidesToShoot()
    {
        if (moreThanOneShot == false)
        {
            for (int i = 0; i < shotsPerTurn; i++)
            {
                if (i == 0 && ShouldMissShot() == false)
                {
                    _enemyInstance.TryShootPlayer();
                    shootMissChancePercentage = 70;
                }
                else if (i == 1 && ShouldMissShot() == false)
                {
                    _enemyInstance.TryShootPlayer();
                    shootMissChancePercentage = 90;
                    moreThanOneShot = true; // Marca que el enemigo ha disparado más de una vez en este turno
                }
                else if (i == 2 && ShouldMissShot() == false)
                {
                    _enemyInstance.TryShootPlayer();
                    shootMissChancePercentage = 20;
                    return;
                }
                else
                {
                    Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} ha fallado su disparo.");
                    shootMissChancePercentage = 20; // Resetea la probabilidad de fallo al valor base
                    return; // Si falla un disparo, no hace más disparos en este turno.
                }
            }
        }
        else
        {
            if (ShouldMissShot() == false) _enemyInstance.TryShootPlayer();
            moreThanOneShot = false; // Resetea la marca de disparo múltiple al final del turno
        }
    }

    private bool ShouldHeal()
    {
        // La comprobación de vida máxima ya se hace en PerformTurnAction para evitar llamar a esto si no es necesario.
        Debug.Log($"El enemigo va a comprobar si debe curarse.");
        if (_enemyInstance.CurrentHealth <= healthThresholdForHealing)
        {
            Debug.Log($"El enemigo debe curarse.");
            return true;
        }
        else
        {
            Debug.Log($"El enemigo no debe curarse.");
            return false;

        }
    }

    private void DecidesToHeal()
    {
        if (_enemyInstance != null && GetRandomNum() > healChancePercentage)
        {
            Debug.Log($"[OutlawEnemyAI]  {_enemyInstance.Data.enemyName}Tiene Cerveza y decide curarse {healAmount} de vida!");
            _enemyInstance.Heal(healAmount); // Asume que el script Enemy tiene un método Heal(int amount)
            EnemyEffectCardUsed = true; // Marca que se ha usado la carta de efecto del enemigo
        }
        else
        {
            Debug.Log($"[OutlawEnemyAI]  {_enemyInstance.Data.enemyName}No tiene Cerveza y por lo tanto no se puede curar.");
        }
    }
    
    private void TryEquipWeapon()
    {
        if (EnemyHasBeenDisarmed)
        {
            Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} no puede equipar un arma porque ha sido desarmado por el jugador.");
            EnemyHasBeenDisarmed = false;
            return;
        }
        if (GetRandomNum() > equipWeaponChancePercentage)
        {
            Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} no ha equipado un arma (probabilidad de equipar: {equipWeaponChancePercentage}%).");
            return;
        }

        // --- Solo si el arma NO estaba equipada y AHORA se va a equipar ---
        if (!weaponEquipped) // Verifica si el estado real va a cambiar
        {
            weaponEquipped = true; // Simulamos que el enemigo tiene un arma equipada.
            Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} ha equipado un arma.");
            OnEnemyWeaponStatusChanged?.Invoke(weaponEquipped); // Dispara el evento (true)
            //Debug.Log("[OutlawEnemyAI] Evento OnEnemyWeaponStatusChanged disparado: TRUE");
        }
    }
    private void TryDisarmPlayer()
    {
        // Solo intenta desarmar si el jugador realmente tiene un arma equipada.
        if (PlayerStats.Instance != null && PlayerStats.Instance.HasWeaponEquipped)
        {
            if (GetRandomNum() < disarmSuccessChancePercentage)
            {
                Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} ¡ha desarmado al jugador!");
                disarmPlayerCounter++; // Incrementa el contador de desarmes
                PlayerStats.Instance.UnequipWeapon(); // Llama al método en PlayerStats
                EnemyEffectCardUsed = true; // Marca que se ha usado la carta de efecto del enemigo
            }
            else
            {
                Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} ha fallado en intentar desarmar al jugador.");
            }
        }
        else
        {
            Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} intentó desarmar, pero el jugador no tiene arma equipada.");
        }
    }

    public void PlayerDisarmedEnemyWeapon()
    {
        weaponEquipped = false; // El enemigo pierde su arma
        EnemyHasBeenDisarmed = true; // Marca que el enemigo ha sido desarmado
        Debug.Log($"[OutlawEnemyAI] El enemigo {_enemyInstance.Data.enemyName} ha sido desarmado por el jugador.");
        
        OnEnemyWeaponStatusChanged?.Invoke(weaponEquipped); // Dispara el evento (false) para indicar que el arma ya no está equipada.
        
        // Podrías añadir un cooldown para el enemigo aquí también si quieres que no se reequipe inmediatamente.
        // Por ejemplo, equipWeaponChancePercentage = 0 para el siguiente turno.
    }

    //-------------------FIN ACCIONES DE LA IA ---------------//

    private int GetRandomNum()
    {
        return Random.Range(0, 100);
    }
}