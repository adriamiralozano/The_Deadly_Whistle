using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DisarmCard", menuName = "Card System/Cards/Desarmar")]
public class DisarmCardData : CardData
{
    private void OnEnable()
    {
        cardID = "DisarmCard";
        type = CardType.Effect;
    }

    public override void ExecuteEffect()
    {
        DisarmEnemy();
    }

    private void DisarmEnemy()
    {
        Debug.Log("[DisarmCardData] Ejecutando efecto: desarmar al enemigo.");

        // Necesitamos una referencia al enemigo activo.
        // Asumiendo que TurnManager tiene una referencia al enemigo actual.
        // O podrías tener un EnemyManager o GameManager que gestione esto.
        if (TurnManager.Instance != null && TurnManager.Instance.activeEnemy != null)
        {
            // Obtener el componente OutlawEnemyAI del enemigo activo
            OutlawEnemyAI enemyAI = TurnManager.Instance.activeEnemy.GetComponent<OutlawEnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.PlayerDisarmedEnemyWeapon(); // Llama al método público en OutlawEnemyAI
                Debug.Log($"[DisarmCardData] El jugador ha desarmado a {TurnManager.Instance.activeEnemy.Data.enemyName}.");
            }
            else
            {
                Debug.LogWarning("[DisarmCardData] El enemigo activo no tiene un componente OutlawEnemyAI.");
            }
        }
        else
        {
            Debug.LogWarning("[DisarmCardData] No hay un enemigo activo en el TurnManager para desarmar.");
        }

        // Marcar que el jugador ha jugado una carta de efecto este turno
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ActivateEffect();
        }
    }
}