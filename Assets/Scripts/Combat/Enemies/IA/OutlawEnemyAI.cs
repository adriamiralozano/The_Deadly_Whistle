using UnityEngine;
using Random = UnityEngine.Random;
using System;
using System.Collections;

public class OutlawEnemyAI : MonoBehaviour, IEnemyAI
{
    private Enemy _enemyInstance; 
    public static event Action<bool> OnEnemyWeaponStatusChanged; 
    public static event Action OnEnemyTurnCompleted; 

    [Header("Visual Feedback")]
    public EnemyCardFeedback cardFeedback;
    public Sprite healCardSprite; 
    public Sprite disarmCardSprite;
    public Sprite weaponCardSprite;


    [Header("AI Settings")]
    [SerializeField] private int equipWeaponChancePercentage = 70; // Porcentaje de probabilidad de que el enemigo equipe un arma al inicio del turno
    [SerializeField] private int disarmSuccessChancePercentage = 5; // Porcentaje de probabilidad de que el enemigo desarme al jugador
    [SerializeField] private int disarmPlayerCooldown = 2; // Número de turnos que debe esperar el enemigo antes de intentar desarmar al jugador nuevamente

    [Header("Healing Settings")]
    [SerializeField] private int healChancePercentage = 45; // Probabilidad de curarse si la vida está baja
    [SerializeField] private int healthThresholdForHealing = 3; // Umbral de vida para decidir curarse (por ejemplo, si la vida actual es menor o igual a este valor)
    [SerializeField] private int healAmount = 1; // Cantidad de vida que se cura
    
    private int shootMissChancePercentage = 45; // Porcentaje de probabilidad de que el disparo falle
    private int lastShotsHit = 0; // Últimos disparos exitosos del enemigo, para controlar la probabilidad de fallo
    private bool weaponEquipped = false; // Indica si el enemigo tiene un arma equipada
    private int disarmPlayerCounter = 0; // Contador para el número de desarmes al jugador
    private int shotsPerTurn = 3;   // Número de disparos que el enemigo puede hacer por turno
    private bool moreThanOneShot = false; // Indica si el enemigo ha fallado más de un disparo en el turno
    private bool EnemyHasBeenDisarmed = false; // Indica si el enemigo ha sido desarmado por el jugador
    private int healCooldown = 0; // Contador para el cooldown de curación, si se implementa
    private bool EnemyEffectCardUsed = false;

    // flags para los yields de las acciones del enemigo

    private bool DisarmSuccesful = false; // Indica si el enemigo ha desarmado al jugador  
    private bool HealSuccessful = false; // Indica si el enemigo ha usado su carta de efecto para curarse
    private bool ShotSuccessful = false; // Indica si el enemigo ha disparado exitosamente al jugador
    private bool EquipWeaponSuccessful = false; // Indica si el enemigo ha equipado un arma exitosamente
    


    public bool HasWeaponEquipped => weaponEquipped;

    public void Initialize(Enemy enemyInstance)
    {
        _enemyInstance = enemyInstance;
        Debug.Log($"[OutlawEnemyAI] IA inicializada para: {_enemyInstance.Data.enemyName}");

        OnEnemyWeaponStatusChanged?.Invoke(weaponEquipped);
        Debug.Log($"[OutlawEnemyAI] Disparando evento de estado de arma al inicializar: {weaponEquipped}");
    }

    public void PerformTurnAction()
    {
        if (_enemyInstance != null && _enemyInstance.IsAlive)
        {
            StartCoroutine(PerformTurnActionCoroutine());
        }
        else
        {
            Debug.LogWarning($"[OutlawEnemyAI] No se puede realizar la acción de turno. Enemigo nulo o no vivo.");
        }
    }
    private IEnumerator PerformTurnActionCoroutine()
    {
        // 1. Equipar arma
        if (!weaponEquipped)
        {
            yield return StartCoroutine(EquipWeaponCoroutine());
        }

        // 2. Curarse
        if (ShouldHeal() && EnemyEffectCardUsed == false)
        {
            yield return StartCoroutine(HealCoroutine());
        }

        // 3. Desarmar
        if (disarmPlayerCounter < disarmPlayerCooldown && EnemyEffectCardUsed == false)
        {
            yield return StartCoroutine(DisarmCoroutine());
        }

        // 4. Disparar
        if (weaponEquipped)
        {
            yield return StartCoroutine(ShootCoroutine());
        }

        // 5. Resetear
        DisarmSuccesful = false;
        EnemyEffectCardUsed = false;
        Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} ha terminado su turno.");

        OnEnemyTurnCompleted?.Invoke(); 
    }

    private IEnumerator EquipWeaponCoroutine()
    {
        Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} va a intentar equipar un arma.");
        TryEquipWeapon();
        
        if (EquipWeaponSuccessful)
        {
            if (cardFeedback != null && weaponCardSprite != null)
            {
                yield return StartCoroutine(cardFeedback.ShowCardFeedbackCoroutine(weaponCardSprite, _enemyInstance.transform.position));
            }
            
            weaponEquipped = true;
            OnEnemyWeaponStatusChanged?.Invoke(weaponEquipped); 
            Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} ha equipado un arma después de la animación!");
            
            yield return new WaitForSeconds(0.5f);
            EquipWeaponSuccessful = false;
        }
    }
    private IEnumerator HealCoroutine()
    {
        Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} ha decidido curarse.");
        DecidesToHeal();

        if (HealSuccessful)
        {
            if (cardFeedback != null && healCardSprite != null)
            {
                yield return StartCoroutine(cardFeedback.ShowCardFeedbackCoroutine(healCardSprite, _enemyInstance.transform.position));
            }

            _enemyInstance.Heal(healAmount);
            Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} se ha curado!");
            yield return new WaitForSeconds(0.5f);
            HealSuccessful = false;

        }
    }

    private IEnumerator DisarmCoroutine()
    {
        Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} va a intentar desarmar al jugador.");
        TryDisarmPlayer();
        
        if (DisarmSuccesful)
        {
            if (cardFeedback != null && disarmCardSprite != null)
            {
                yield return StartCoroutine(cardFeedback.ShowCardFeedbackCoroutine(disarmCardSprite, _enemyInstance.transform.position));
            }
            
            PlayerStats.Instance.UnequipWeapon();
            Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} ha desarmado al jugador!");
            
            yield return new WaitForSeconds(0.5f);
            DisarmSuccesful = false;
        }
    }

    private IEnumerator ShootCoroutine()
    {
        Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} tiene un arma equipada y va a disparar.");
        yield return new WaitForSeconds(1f);
        DecidesToShoot();
        if (TurnManager.Instance != null && lastShotsHit > 0)
            yield return TurnManager.Instance.StartCoroutine(TurnManager.Instance.EnemyShotFeedback(lastShotsHit));        
        if (ShotSuccessful)
        {
            yield return new WaitForSeconds(1f);
            ShotSuccessful = false;
        }
    }

    //-------------------ACCIONES DE LA IA -------------------//
    private bool ShouldMissShot()
    {
        return GetRandomNum() < shootMissChancePercentage;

    }

    private void DecidesToShoot()
    {
        lastShotsHit = 0;
        if (moreThanOneShot == false)
        {
            for (int i = 0; i < shotsPerTurn; i++)
            {
                if (i == 0 && ShouldMissShot() == false)
                {
                    _enemyInstance.TryShootPlayer();
                    shootMissChancePercentage = 75;
                    ShotSuccessful = true;
                    lastShotsHit++;
                }
                else if (i == 1 && ShouldMissShot() == false)
                {
                    _enemyInstance.TryShootPlayer();
                    shootMissChancePercentage = 90;
                    moreThanOneShot = true;
                    lastShotsHit++;
                }
                else if (i == 2 && ShouldMissShot() == false)
                {
                    _enemyInstance.TryShootPlayer();
                    shootMissChancePercentage = 45;
                    lastShotsHit++;
                    return;
                }
                else
                {
                    Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} ha fallado su disparo.");
                    shootMissChancePercentage = 45;
                    return;
                }
            }
        }
        else
        {
            if (ShouldMissShot() == false)
            {
                _enemyInstance.TryShootPlayer();
                lastShotsHit++;
            }
            moreThanOneShot = false;
        }
    }

    private bool ShouldHeal()
    {
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
        if (healCooldown == 0)
        {
            if (_enemyInstance != null && GetRandomNum() < healChancePercentage)
            {
                Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} decide usar una carta de curación!");
                EnemyEffectCardUsed = true;
                healCooldown = 1;
                HealSuccessful = true;
            }
            else
            {
                healCooldown = 0;
                Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} no tiene Cerveza y por lo tanto no se puede curar.");
            }
        }
        else if (healCooldown == 1)
        {
            if (_enemyInstance != null && GetRandomNum() < 15)
            {
                Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} decide usar una carta de curación!");
                EnemyEffectCardUsed = true;
                healCooldown = 2;
                HealSuccessful = true;
            }
            else
            {
                healCooldown = 0;
                Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} no tiene Cerveza y por lo tanto no se puede curar.");
            }
        }
        else if(healCooldown == 2)
        {
            healCooldown = 0;
            Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} no tiene Cerveza y por lo tanto no se puede curar.");
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
        
        if (!weaponEquipped)
        {
            Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} decide equipar un arma!");
            EquipWeaponSuccessful = true;
        }
    }
    private void TryDisarmPlayer()
    {
        if (PlayerStats.Instance != null && PlayerStats.Instance.HasWeaponEquipped)
        {
            if (GetRandomNum() < disarmSuccessChancePercentage)
            {
                Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} decide usar carta de desarme!");
                disarmPlayerCounter++;
                
                EnemyEffectCardUsed = true; 
                DisarmSuccesful = true; 
                
                Debug.Log($"[OutlawEnemyAI] Desarme programado para después de la animación.");
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
        weaponEquipped = false;
        EnemyHasBeenDisarmed = true;
        Debug.Log($"[OutlawEnemyAI] El enemigo {_enemyInstance.Data.enemyName} ha sido desarmado por el jugador.");
        
        OnEnemyWeaponStatusChanged?.Invoke(weaponEquipped);
    }

    //-------------------FIN ACCIONES DE LA IA ---------------//

    private int GetRandomNum()
    {
        return Random.Range(0, 100);
    }
}