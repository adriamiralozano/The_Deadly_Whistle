// DropTarget.cs
using UnityEngine;
using UnityEngine.EventSystems; // Necesario para las interfaces IDropHandler, IPointerEnterHandler, IPointerExitHandler
using UnityEngine.UI; // Necesario para el componente Image
using DG.Tweening;

public class DropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual Feedback")]
    [SerializeField] private Color highlightColor = new Color(0.8f, 1f, 0.8f, 1f); // Color verde claro para resaltar el objetivo
    private Color normalColor; // Color original del Image del DropTarget
    private Image targetImage; // Referencia al componente Image de este GameObject

    [Header("Outline")]
    [SerializeField] private GameObject outlineTargetGO;

    public enum TargetType { Player, Enemy, Discard, Hand } // Tipos de objetivos a los que se puede dropear una carta
    public TargetType myTargetType; // El tipo de este DropTarget específico (asignar en el Inspector)

    private void Awake()
    {
        targetImage = GetComponent<Image>(); // Intenta obtener el componente Image
        if (targetImage == null)
        {
            Debug.LogWarning($"DropTarget en {gameObject.name} no tiene un componente Image. No se podrá cambiar el color para la retroalimentación visual al pasar el ratón.");
        }
        else
        {
            normalColor = targetImage.color; // Guarda el color original
        }
    }

    // Se llama cuando el puntero (con un objeto dragueable) entra en el área de este DropTarget
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
                        renderer.material.SetFloat("_OutlineEnabled", 1f); // Activa el outline
                    }
                }

                // Si este DropTarget es de tipo Discard, haz un shake
                if (myTargetType == TargetType.Discard)
                {
                    RectTransform rect = GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.DOComplete(); // Detén shakes previos si los hay
                        Vector2 originalPos = rect.anchoredPosition;
                        rect.DOShakeAnchorPos(0.15f, 15f, 25, 0, false, false)
                            .OnComplete(() => rect.anchoredPosition = originalPos);
                    }
                }
            }
        }
    }

    // Se llama cuando el puntero (con un objeto dragueable) sale del área de este DropTarget
    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetImage != null)
            targetImage.color = normalColor;

        if (outlineTargetGO != null)
        {
            var renderer = outlineTargetGO.GetComponent<Renderer>();
            if (renderer != null && renderer.material.HasProperty("_OutlineEnabled"))
            {
                renderer.material.SetFloat("_OutlineEnabled", 0f); // Desactiva el outline
            }
        }

        if (eventData.pointerDrag != null)
        {
            var cardBehaviour = eventData.pointerDrag.GetComponent<CardBehaviour2>();
            if (cardBehaviour != null)
                cardBehaviour.SetDragOverTargetScale(false);
        }
    }
    // Se llama cuando un objeto dragueable se suelta en el área de este DropTarget
    public void OnDrop(PointerEventData eventData)
    {

        if (outlineTargetGO != null)
        {
            var renderer = outlineTargetGO.GetComponent<Renderer>();
            if (renderer != null && renderer.material.HasProperty("_OutlineEnabled"))
            {
                renderer.material.SetFloat("_OutlineEnabled", 0f); // Desactiva el outline
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
        //Debug.Log($"Carta soltada en {gameObject.name} (Tipo de objetivo: {myTargetType})");

        if (targetImage != null)
        {
            targetImage.color = normalColor; // Restaura el color del objetivo
        }

        GameObject droppedObject = eventData.pointerDrag; // El GameObject que fue arrastrado

        if (droppedObject != null)
        {
            // Intentamos obtener el script CardUI del objeto arrastrado
            CardUI cardUI = droppedObject.GetComponent<CardUI>();

            if (cardUI != null)
            {
                // Obtenemos los datos de la carta desde el CardUI
                CardData droppedCardData = cardUI.GetCardData();

                if (droppedCardData != null)
                {
/*                     Debug.Log($"[DEBUG - DropTarget] La carta '{droppedCardData.cardID}' (ID de instancia: {droppedCardData.instanceID}) dropeada es de TIPO: {droppedCardData.type}"); */

                    // --- Validaciones de Managers (asegurarse de que los Singletons están accesibles) ---
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
                    
                    // --- Lógica de Manejo de Cartas Basada en el Tipo de Objetivo ---

                    if (myTargetType == TargetType.Player) // Si la carta se dropea en la zona del jugador (zona de "juego")
                    {

                        if (droppedCardData.cardID == "BeerCard" && PlayerStats.Instance.CurrentHealth >= PlayerStats.Instance.maxHealth)
                        {
                            Debug.Log("[DropTarget] No puedes usar Cerveza si la vida está al máximo. El drop se cancela.");
                            // Aquí puedes poner feedback visual si quieres
                            return; // Cancela el drop, la carta vuelve a la mano
                        }
                        // --- LÓGICA CLAVE: IMPEDIR JUGAR CARTAS PASIVAS EN ZONA DEL JUGADOR ---
                        if (droppedCardData.type == CardType.Passive)
                        {
                            Debug.LogWarning($"[DropTarget] Carta '{droppedCardData.cardID}' (Tipo: Pasiva) NO puede ser jugada en la zona del jugador. Volviendo a la mano.");
                            // IMPORTANTE: No re-parentamos la carta ni llamamos a CardManager.PlayCard().
                            // El script CardBehaviour2.cs adjunto a la carta detectará en su OnEndDrag()
                            // que el padre de la carta no ha cambiado (sigue siendo el root Canvas)
                            // y activará ReturnToOriginalPosition() automáticamente, devolviéndola a la mano.
                            return; // Salimos de la función OnDrop para que no se procese como una jugada.
                        }
                        // -----------------------------------------------------------------------
                        // Si la carta es de tipo WEAPON, verificamos si ya hay un arma equipada


                        // Si la carta NO es pasiva, o si la fase de turno no es la de acción,
                        // el juego procede con la lógica normal de juego de cartas.
                        if (TurnManager.Instance.CurrentPhase == TurnManager.TurnPhase.ActionPhase)
                        {
                            if (droppedCardData.type == CardType.Effect && PlayerStats.Instance.HasPlayedEffectCardThisTurn())
                            {
                                AdviceMessageManager.Instance.ShowAdvice($"You have already played an effect card this turn.");
                                Debug.LogWarning($"[DropTarget] Ya has jugado una carta de efecto este turno. No puedes dropear '{droppedCardData.cardID}'.");
                                return; // Cancela el drop, la carta regresa a la mano.
                            }
                            if (droppedCardData is DisarmCardData) // Comprueba si la carta es una DisarmCardData
                            {
                                // La carta DisarmCardData solo se puede jugar si hay un enemigo Y tiene un arma equipada.
                                if (currentEnemyAI == null || !currentEnemyAI.HasWeaponEquipped)
                                {
                                    AdviceMessageManager.Instance.ShowAdvice($"The enemy is already disarmed.");
                                    Debug.LogWarning($"[DropTarget] No se puede usar la carta 'Desarmar'. El enemigo ya está desarmado o no hay enemigo activo.");
                                    return; // Cancela el drop si el enemigo ya está desarmado
                                }
                                // Si llegamos aquí, la carta SÍ se puede jugar, continuará la ejecución normal del efecto.
                            }
                            // Lógica para cartas de tipo WEAPON
                            if (droppedCardData.type == CardType.Weapon)
                            {
                                // AHORA: Intentamos castear a RevolverCardData para aplicar su lógica específica
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
                                    // NO dispares aquí, solo equipa.
                                }
                                else // Para armas genéricas
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
                            // Lógica para cartas de tipo EFFECT
                            else if (droppedCardData.type == CardType.Effect)
                            {

                                if (PlayerStats.Instance.HasPlayedEffectCardThisTurn())
                                {
                                    Debug.LogWarning($"[DropTarget] Ya has jugado una carta de efecto este turno. No puedes dropear '{droppedCardData.cardID}'.");
                                    return; // Cancela el drop, la carta regresa a la mano.
                                }
                                // Aquí es donde el efecto se ejecuta y HasPlayedEffectCardThisTurn se pone a true.
                                Debug.Log($"[DropTarget] Se detectó carta de EFECTO '{droppedCardData.cardID}' soltada en el Player. Ejecutando efecto.");
                                droppedCardData.ExecuteEffect();

                                bool played = CardManager.Instance.PlayCard(droppedCardData);
                                if (played) Debug.Log($"[DropTarget] Carta de Efecto '{droppedCardData.cardID}' jugada exitosamente y movida a descarte.");
                                else Debug.LogWarning($"[DropTarget] Falló al jugar la carta de efecto '{droppedCardData.cardID}'.");
                            }
                            // Si en el futuro tienes otros tipos de cartas jugables (no pasivas)
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
                            // La carta regresará a su posición original automáticamente.
                        }
                    }
                    // Si el objetivo es 'Enemy' (o cualquier otra zona de juego que no sea del jugador)
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
                                    return; // Cancela el drop si el enemigo ya está desarmado
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
                            // La carta regresará a su posición original automáticamente.
                        }
                    }
                    // Si el objetivo es 'Discard' (para descarte manual de cartas)
                    else if (myTargetType == TargetType.Discard)
                    {
                        // Las cartas pasivas (y todas las demás) SÍ pueden ser descartadas aquí.
                        /*                         Debug.Log($"[DropTarget] Soltada carta '{droppedCardData.cardID}' (ID de instancia: {droppedCardData.instanceID}) en objetivo 'Discard'. Intentando descarte manual."); */

                        // Usamos AttemptManualDiscard de CardManager, que ya tiene las validaciones de fase y límite de mano.
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
                    // Si el tipo de objetivo no está definido o manejado explícitamente en este script
                    else
                    {
                        Debug.LogWarning($"[DropTarget] Tipo de objetivo '{myTargetType}' no manejado para la carta {droppedCardData.cardID}. La carta regresará a la mano.");
                        // La carta regresará a su posición original automáticamente por CardBehaviour2.OnEndDrag()
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