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

        if (TurnManager.Instance != null && TurnManager.Instance.activeEnemy != null)
        {
            OutlawEnemyAI enemyAI = TurnManager.Instance.activeEnemy.GetComponent<OutlawEnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.PlayerDisarmedEnemyWeapon();
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

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ActivateEffect();
        }
    }
}