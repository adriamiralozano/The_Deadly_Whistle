using UnityEngine;
using Random = UnityEngine.Random; // Para evitar conflictos si se usa System.Random

// Importante: Este script DEBE implementar la interfaz IEnemyAI
public class OutlawEnemyAI : MonoBehaviour, IEnemyAI
{
    private Enemy _enemyInstance; // Una referencia al componente Enemy en el mismo GameObject

    [Header("AI Settings")]
    [Range(0, 100)] // Asegura que el valor en el Inspector esté entre 0 y 100
    [SerializeField] private int shootMissChancePercentage = 20; // Porcentaje de probabilidad de que el disparo falle

    [Header("Healing Settings")]
    [Range(0, 100)]
    [SerializeField] private int healChancePercentage = 50; // Probabilidad de curarse si la vida está baja
    [SerializeField] private int healthThresholdForHealing = 3; // Umbral de vida para decidir curarse (por ejemplo, si la vida actual es menor o igual a este valor)
    [SerializeField] private int healAmount = 1; // Cantidad de vida que se cura

    public void Initialize(Enemy enemyInstance)
    {
        _enemyInstance = enemyInstance;
        Debug.Log($"[OutlawEnemyAI] IA inicializada para: {_enemyInstance.Data.enemyName}");
    }

    // Este método se llamará al inicio del turno del enemigo.
    // Aquí es donde la IA evalúa el estado del juego y decide qué hacer.
    public void PerformTurnAction()
    {
        Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} está decidiendo su acción...");

        if (_enemyInstance != null && _enemyInstance.IsAlive)
        {
            // Decidir si curarse basado en el número absoluto de vidas
            // Si la vida actual es MENOR o IGUAL al umbral Y no está a vida máxima Y pasa la probabilidad de curación
            if (ShouldHeal())
            {
                Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} ha decidido curarse.");
                DecidesToHeal();
            }

            DecidesToShoot();

        }
        else
        {
            Debug.LogWarning($"[OutlawEnemyAI] No se puede realizar la acción de turno. Enemigo nulo o no vivo.");
        }
    }

    //-------------------ACCIONES DE LA IA -------------------//
    private bool ShouldMissShot()
    {
        return Random.Range(0, 100) < shootMissChancePercentage;
    }

    private void DecidesToShoot()
    {
        if (ShouldMissShot())
        {
            Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} HA FALLADO su disparo (probabilidad de fallo: {shootMissChancePercentage}%)!");
            // Aquí podrías añadir alguna lógica para un disparo fallido (animación, sonido, etc.).
        }
        else
        {
            Debug.Log($"[OutlawEnemyAI] {_enemyInstance.Data.enemyName} va a disparar!");
            _enemyInstance.TryShootPlayer();
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
        else{
            Debug.Log($"El enemigo no debe curarse.");
            return false;
            
        }
    }

    private void DecidesToHeal()
    {
        if (_enemyInstance != null && Random.Range(0, 100) > healChancePercentage)
        {
            Debug.Log($"[OutlawEnemyAI]  {_enemyInstance.Data.enemyName}Tiene Cerveza y decide curarse {healAmount} de vida!");
            _enemyInstance.Heal(healAmount); // Asume que el script Enemy tiene un método Heal(int amount)
        }
        else
        {
            Debug.Log($"[OutlawEnemyAI]  {_enemyInstance.Data.enemyName}No tiene Cerveza y por lo tanto no se puede curar.");   
        }
    }
    //-------------------FIN ACCIONES DE LA IA ---------------//
}