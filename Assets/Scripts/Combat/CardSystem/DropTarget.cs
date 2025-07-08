using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class DropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual Feedback")]
    [SerializeField] private Color highlightColor = new Color(0.8f, 1f, 0.8f, 1f);
    private Color normalColor; 
    private Image targetImage;

    [Header("Outline")]
    [SerializeField] private GameObject outlineTargetGO;

    public enum TargetType { Player, Enemy, Discard, Hand }
    public TargetType myTargetType;
    private void Awake()
    {
        targetImage = GetComponent<Image>();
        if (targetImage == null)
        {
            Debug.LogWarning($"DropTarget en {gameObject.name} no tiene un componente Image. No se podrá cambiar el color para la retroalimentación visual al pasar el ratón.");
        }
        else
        {
            normalColor = targetImage.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.ActionPhase)
        {
            if (TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.DiscardPostShot)
            {
                return;
            }
        }

        if (eventData.pointerDrag != null && targetImage != null)
        {
            var cardUI = eventData.pointerDrag.GetComponent<CardUI>();
            var cardBehaviour = eventData.pointerDrag.GetComponent<CardBehaviour2>();
            if (cardUI != null && cardBehaviour != null)
            {
                targetImage.color = highlightColor;
                cardBehaviour.SetDragOverTargetScale(true);


                if (outlineTargetGO != null)
                {
                    var renderer = outlineTargetGO.GetComponent<Renderer>();
                    if (renderer != null && renderer.material.HasProperty("_OutlineEnabled"))
                    {
                        renderer.material.SetFloat("_OutlineEnabled", 1f);
                    }
                }

                if (myTargetType == TargetType.Discard)
                {
                    RectTransform rect = GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.DOComplete();
                        Vector2 originalPos = rect.anchoredPosition;
                        rect.DOShakeAnchorPos(0.15f, 15f, 25, 0, false, false)
                            .OnComplete(() => rect.anchoredPosition = originalPos);
                    }
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetImage != null)
            targetImage.color = normalColor;

        if (outlineTargetGO != null)
        {
            var renderer = outlineTargetGO.GetComponent<Renderer>();
            if (renderer != null && renderer.material.HasProperty("_OutlineEnabled"))
            {
                renderer.material.SetFloat("_OutlineEnabled", 0f);
            }
        }

        if (eventData.pointerDrag != null)
        {
            var cardBehaviour = eventData.pointerDrag.GetComponent<CardBehaviour2>();
            if (cardBehaviour != null)
                cardBehaviour.SetDragOverTargetScale(false);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {

        if (outlineTargetGO != null)
        {
            var renderer = outlineTargetGO.GetComponent<Renderer>();
            if (renderer != null && renderer.material.HasProperty("_OutlineEnabled"))
            {
                renderer.material.SetFloat("_OutlineEnabled", 0f);
            }
        }

        if (TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.ActionPhase &&
            TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.DiscardPostShot)
        {
            return;
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCardDrop();
        }


        if (targetImage != null)
        {
            targetImage.color = normalColor;
        }

        GameObject droppedObject = eventData.pointerDrag;

        if (droppedObject != null)
        {
            CardUI cardUI = droppedObject.GetComponent<CardUI>();

            if (cardUI != null)
            {

                CardData droppedCardData = cardUI.GetCardData();

                if (droppedCardData != null)
                {

                    if (TurnManager.Instance == null)
                    {
                        Debug.LogError("[DropTarget] TurnManager.Instance no encontrado. Asegúrate de que el TurnManager está en la escena y tiene un Singleton configurado.");
                        return;
                    }
                    if (CardManager.Instance == null)
                    {
                        Debug.LogError("[DropTarget] CardManager.Instance no encontrado. Asegúrate de que el CardManager está en la escena y tiene un Singleton configurado.");
                        return;
                    }
                    if (PlayerStats.Instance == null)
                    {
                        Debug.LogError("[DropTarget] PlayerStats.Instance no encontrado. Asegúrate de que PlayerStats está en la escena y tiene un Singleton configurado.");
                        return;
                    }

                    OutlawEnemyAI currentEnemyAI = null;

                    if (TurnManager.Instance.activeEnemy != null)
                    {
                        currentEnemyAI = TurnManager.Instance.activeEnemy.GetComponent<OutlawEnemyAI>();
                    }
                    

                    if (myTargetType == TargetType.Player) 
                    {

                        if (droppedCardData.cardID == "BeerCard" && PlayerStats.Instance.CurrentHealth >= PlayerStats.Instance.maxHealth)
                        {
                            Debug.Log("[DropTarget] No puedes usar Cerveza si la vida está al máximo. El drop se cancela.");
                            return; 
                        }
                        if (droppedCardData.type == CardType.Passive)
                        {
                            Debug.LogWarning($"[DropTarget] Carta '{droppedCardData.cardID}' (Tipo: Pasiva) NO puede ser jugada en la zona del jugador. Volviendo a la mano.");
                            return;
                        }

                        if (TurnManager.Instance.CurrentPhase == TurnManager.TurnPhase.ActionPhase)
                        {
                            if (droppedCardData.type == CardType.Effect && PlayerStats.Instance.HasPlayedEffectCardThisTurn())
                            {
                                AdviceMessageManager.Instance.ShowAdvice($"You have already played an effect card this turn.");
                                Debug.LogWarning($"[DropTarget] Ya has jugado una carta de efecto este turno. No puedes dropear '{droppedCardData.cardID}'.");
                                return; 
                            }
                            if (droppedCardData is DisarmCardData) 
                            {
                                if (currentEnemyAI == null || !currentEnemyAI.HasWeaponEquipped)
                                {
                                    AdviceMessageManager.Instance.ShowAdvice($"The enemy is already disarmed.");
                                    Debug.LogWarning($"[DropTarget] No se puede usar la carta 'Desarmar'. El enemigo ya está desarmado o no hay enemigo activo.");
                                    return;
                                }
                            }
                            if (droppedCardData.type == CardType.Weapon)
                            {
                                if (droppedCardData is RevolverCardData revolverCard)
                                {
                                    if (PlayerStats.Instance.HasWeaponEquipped)
                                    {
                                        AdviceMessageManager.Instance.ShowAdvice($"You already have a weapon equipped.");
                                        Debug.LogWarning($"[DropTarget] No se puede equipar '{revolverCard.cardID}'. El jugador ya tiene un arma equipada. La carta volverá a la mano.");
                                        return;
                                    }
                                    else
                                    {
                                        PlayerStats.Instance.EquipWeapon(revolverCard);
                                        bool played = CardManager.Instance.PlayCard(revolverCard);
                                        if (played)
                                            Debug.Log($"[DropTarget] Revolver '{revolverCard.cardID}' equipado y movido a descarte.");
                                        else
                                            Debug.LogWarning($"[DropTarget] Falló al jugar el Revolver '{revolverCard.cardID}'.");
                                    }
                                }
                                else
                                {
                                    if (PlayerStats.Instance.HasWeaponEquipped)
                                    {
                                        Debug.LogWarning($"[DropTarget] No se puede equipar '{droppedCardData.cardID}'. El jugador ya tiene un arma equipada. La carta volverá a la mano.");
                                        return;
                                    }
                                    else
                                    {
                                        PlayerStats.Instance.EquipWeapon(droppedCardData);
                                        bool played = CardManager.Instance.PlayCard(droppedCardData);
                                        if (played)
                                        {
                                            Debug.Log($"[DropTarget] Arma genérica '{droppedCardData.cardID}' equipada y jugada exitosamente en el Player.");
                                        }
                                        else
                                        {
                                            Debug.LogWarning($"[DropTarget] Falló al jugar la carta de arma genérica '{droppedCardData.cardID}'.");
                                        }
                                    }
                                }
                            }
                            else if (droppedCardData.type == CardType.Effect)
                            {

                                if (PlayerStats.Instance.HasPlayedEffectCardThisTurn())
                                {
                                    Debug.LogWarning($"[DropTarget] Ya has jugado una carta de efecto este turno. No puedes dropear '{droppedCardData.cardID}'.");
                                    return;
                                }

                                Debug.Log($"[DropTarget] Se detectó carta de EFECTO '{droppedCardData.cardID}' soltada en el Player. Ejecutando efecto.");
                                droppedCardData.ExecuteEffect();

                                bool played = CardManager.Instance.PlayCard(droppedCardData);
                                if (played) Debug.Log($"[DropTarget] Carta de Efecto '{droppedCardData.cardID}' jugada exitosamente y movida a descarte.");
                                else Debug.LogWarning($"[DropTarget] Falló al jugar la carta de efecto '{droppedCardData.cardID}'.");
                            }
                            else
                            {
                                Debug.LogWarning($"[DropTarget] Soltada carta '{droppedCardData.cardID}' (ID de instancia: {droppedCardData.instanceID}) en objetivo '{myTargetType}' durante la Fase de Acción. Jugando la carta (tipo: {droppedCardData.type}). Este tipo de carta no tiene lógica de activación específica en DropTarget, CardManager la moverá a descarte.");
                                bool played = CardManager.Instance.PlayCard(droppedCardData);
                                if (played)
                                {
                                    Debug.Log($"[DropTarget] Carta '{droppedCardData.cardID}' jugada exitosamente y movida a descarte por CardManager.");
                                }
                                else
                                {
                                    Debug.LogWarning($"[DropTarget] Falló al jugar la carta '{droppedCardData.cardID}'.");
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[DropTarget] No se puede jugar la carta '{droppedCardData.cardID}' en objetivo '{myTargetType}' durante la fase {TurnManager.Instance.CurrentPhase}. Las cartas solo pueden jugarse en la Fase de Acción.");
                        }
                    }
                    else if (myTargetType == TargetType.Enemy)
                    {
                        if (TurnManager.Instance.CurrentPhase == TurnManager.TurnPhase.ActionPhase)
                        {
                            Debug.Log($"[DropTarget] Soltada carta '{droppedCardData.cardID}' (ID de instancia: {droppedCardData.instanceID}) en objetivo '{myTargetType}' durante la Fase de Acción. Jugando la carta.");

                            if (droppedCardData is DisarmCardData)
                            {
                                if (currentEnemyAI == null || !currentEnemyAI.HasWeaponEquipped)
                                {
                                    Debug.LogWarning($"[DropTarget] No se puede usar la carta 'Desarmar' en el enemigo. Ya está desarmado o no hay enemigo activo.");
                                    return;
                                }

                            }

                            bool played = CardManager.Instance.PlayCard(droppedCardData);

                            if (played)
                            {
                                Debug.Log($"[DropTarget] Carta '{droppedCardData.cardID}' jugada exitosamente y movida a descarte por CardManager.");
                            }
                            else
                            {
                                Debug.LogWarning($"[DropTarget] Falló al jugar la carta '{droppedCardData.cardID}'.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[DropTarget] No se puede jugar la carta '{droppedCardData.cardID}' en objetivo '{myTargetType}' durante la fase {TurnManager.Instance.CurrentPhase}. Las cartas solo pueden jugarse en la Fase de Acción.");
                        }
                    }
                    else if (myTargetType == TargetType.Discard)
                    {
                        bool discarded = CardManager.Instance.AttemptManualDiscard(droppedCardData);
                        if (discarded)
                        {
                            Debug.Log($"[DropTarget] Carta '{droppedCardData.cardID}' descartada exitosamente en la zona de descarte manual.");
                        }
                        else
                        {
                            Debug.LogWarning($"[DropTarget] Intento de descarte manual de '{droppedCardData.cardID}' fallido. (Ver logs anteriores para la razón).");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[DropTarget] Tipo de objetivo '{myTargetType}' no manejado para la carta {droppedCardData.cardID}. La carta regresará a la mano.");
                    }
                }
                else
                {
                    Debug.LogWarning("[DropTarget] El CardUI del objeto soltado no tiene CardData asignado. Asegúrate de que las cartas tienen sus datos correctamente configurados.");
                }
            }
            else
            {
                Debug.LogWarning($"[DropTarget] El objeto arrastrado '{droppedObject.name}' no tiene el script CardUI. Esto no es una carta jugable.");
            }
        }
    }
}