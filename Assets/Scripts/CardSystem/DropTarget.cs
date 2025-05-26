using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual Feedback")]
    [SerializeField] private Color highlightColor = new Color(0.8f, 1f, 0.8f, 1f); // Verde claro para resaltar
    private Color normalColor; 
    private Image targetImage; // Referencia al componente Image de este GameObject

    // Define los tipos de objetivos a los que se puede soltar una carta
    public enum TargetType { Player, Enemy, Discard, Hand }
    public TargetType myTargetType; // El tipo específico de este DropTarget

    private void Awake()
    {
        // Intenta obtener el componente Image al inicio.
        // Se usa para cambiar el color y dar retroalimentación visual.
        targetImage = GetComponent<Image>();
        if (targetImage == null)
        {
            Debug.LogWarning($"DropTarget en {gameObject.name} no tiene un componente Image. No se podrá cambiar el color para la retroalimentación visual.");
        }
        else
        {
            // Guarda el color original para poder restaurarlo
            normalColor = targetImage.color; 
        }
    }

    // Se llama cuando el puntero del ratón (con un objeto arrastrado) entra en este DropTarget
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Solo cambia el color si hay un objeto arrastrado y ese objeto es una carta (tiene CardUI)
        if (eventData.pointerDrag != null && targetImage != null)
        {
            if (eventData.pointerDrag.GetComponent<CardUI>() != null) 
            {
                targetImage.color = highlightColor; // Resalta el objetivo
            }
        }
    }

    // Se llama cuando el puntero del ratón (con un objeto arrastrado) sale de este DropTarget
    public void OnPointerExit(PointerEventData eventData)
    {
        // Restaura el color normal del objetivo cuando la carta sale
        if (targetImage != null)
        {
            targetImage.color = normalColor;
        }
    }

    // Se llama cuando se suelta un objeto arrastrado sobre este DropTarget
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"Carta soltada en {gameObject.name} (Tipo de objetivo: {myTargetType})");

        // Restaura el color normal inmediatamente después de soltar
        if (targetImage != null)
        {
            targetImage.color = normalColor;
        }

        // Obtiene el GameObject que fue arrastrado
        GameObject droppedObject = eventData.pointerDrag;

        if (droppedObject != null)
        {
            // Intenta obtener el script CardUI del objeto arrastrado
            CardUI cardUI = droppedObject.GetComponent<CardUI>();

            if (cardUI != null)
            {
                // Obtiene los datos de la carta del script CardUI
                CardData droppedCardData = cardUI.GetCardData();

                if (droppedCardData != null)
                {
                    // --- Validaciones de Managers ---
                    if (TurnManager.Instance == null)
                    {
                        Debug.LogError("[DropTarget] TurnManager.Instance no encontrado. Asegúrate de que el TurnManager está en la escena y tiene un Singleton.");
                        return; 
                    }
                    if (CardManager.Instance == null)
                    {
                        Debug.LogError("[DropTarget] CardManager.Instance no encontrado. Asegúrate de que el CardManager está en la escena y tiene un Singleton.");
                        return; 
                    }

                    // --- Lógica de Manejo de Cartas Basada en el Tipo de Objetivo ---

                    // Si el objetivo es 'Player' o 'Enemy', se considera una acción de "jugar" la carta
                    if (myTargetType == TargetType.Player || myTargetType == TargetType.Enemy)
                    {
                        // Solo permite jugar la carta si el juego está en la Fase de Acción
                        if (TurnManager.Instance.CurrentPhase == TurnManager.TurnPhase.ActionPhase)
                        {
                            Debug.Log($"[DropTarget] Soltada carta '{droppedCardData.cardID}' (ID de instancia: {droppedCardData.instanceID}) en objetivo '{myTargetType}' durante la Fase de Acción. Procediendo a jugar la carta.");

                            // Llama al CardManager para ejecutar la lógica de jugar la carta
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
                            // La carta regresará a su posición original automáticamente si no se pudo jugar
                        }
                    }
                    // Si el objetivo es 'Discard', se considera una acción de descarte manual
                    else if (myTargetType == TargetType.Discard)
                    {
                        Debug.Log($"[DropTarget] Soltada carta '{droppedCardData.cardID}' (ID de instancia: {droppedCardData.instanceID}) en objetivo 'Discard'. Intentando descarte manual.");
                        
                        // Llama al método AttemptManualDiscard del CardManager, que ya contiene las validaciones
                        // de la fase de descarte y el número de cartas en mano.
                        bool discarded = CardManager.Instance.AttemptManualDiscard(droppedCardData); 
                        if (discarded)
                        {
                             Debug.Log($"[DropTarget] Carta '{droppedCardData.cardID}' descartada exitosamente en la zona de descarte manual.");
                        }
                        else
                        {
                            // AttemptManualDiscard ya imprime mensajes de advertencia detallados
                            // si el descarte no es permitido por la fase o el límite de cartas.
                            Debug.LogWarning($"[DropTarget] Intento de descarte manual de '{droppedCardData.cardID}' fallido. (Ver logs anteriores para la razón).");
                        }
                    }
                    // Si el tipo de objetivo no está definido o manejado, se registra una advertencia
                    else 
                    {
                        Debug.LogWarning($"[DropTarget] Tipo de objetivo '{myTargetType}' no manejado para la carta {droppedCardData.cardID}. La carta regresará a la mano.");
                    }
                }
                else
                {
                    Debug.LogWarning("[DropTarget] El CardUI del objeto soltado no tiene CardData asignado. Asegúrate de que las cartas tienen sus datos.");
                }
            }
            else
            {
                Debug.LogWarning($"[DropTarget] El objeto arrastrado '{droppedObject.name}' no tiene el script CardUI. Esto no es una carta jugable.");
            }
        }
    }
}