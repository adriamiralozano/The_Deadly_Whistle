using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual Feedback")]
    [SerializeField] private Color highlightColor = new Color(0.8f, 1f, 0.8f, 1f); // Verde claro
    private Color normalColor; 
    private Image targetImage;

    public enum TargetType { Player, Enemy, Discard, Hand }
    public TargetType myTargetType;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        if (targetImage == null)
        {
            Debug.LogWarning($"DropTarget en {gameObject.name} no tiene un componente Image. No se podrá cambiar el color.");
        }
        else
        {
            normalColor = targetImage.color; 
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && targetImage != null)
        {
            if (eventData.pointerDrag.GetComponent<CardUI>() != null) 
            {
                targetImage.color = highlightColor;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetImage != null)
        {
            targetImage.color = normalColor;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"Carta soltada en {gameObject.name} ({myTargetType} Target)");

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
                    // Verificar que los Managers existan antes de usarlos
                    if (TurnManager.Instance == null)
                    {
                        Debug.LogError("[DropTarget] TurnManager.Instance no encontrado. No se puede verificar la fase del juego.");
                        return; 
                    }
                    if (CardManager.Instance == null)
                    {
                        Debug.LogError("[DropTarget] CardManager.Instance no encontrado.");
                        return; 
                    }

                    // Lógica de "jugar" carta (va al descarte) - SOLO SI EL TIPO DE TARGET ES JUGADOR/ENEMIGO
                    if (myTargetType == TargetType.Player || myTargetType == TargetType.Enemy)
                    {
                        // AHORA SÍ: Comprobación de la fase. Solo permitir jugar en la Fase de Acción.
                        if (TurnManager.Instance.CurrentPhase == TurnManager.TurnPhase.ActionPhase) // Usamos TurnPhase de TurnManager
                        {
                            Debug.Log($"[DropTarget] Soltada carta '{droppedCardData.cardID}' (Instance ID: {droppedCardData.instanceID}) en objetivo '{myTargetType}' durante la Fase de Acción. Jugando la carta.");

                            // Llama al CardManager para que "juegue" la carta (que la mueve a descarte y destruye su visual)
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
                            Debug.LogWarning($"[DropTarget] No se puede jugar la carta '{droppedCardData.cardID}' en objetivo '{myTargetType}' durante la fase {TurnManager.Instance.CurrentPhase}. Solo se puede en ActionPhase.");
                            // Si no se puede jugar, la carta regresará a su posición original automáticamente por CardBehaviour2
                        }
                    }
                    // Lógica de "descartar directamente" a una zona de descarte dedicada.
                    // Esto se activaría si tuvieras un DropTarget con myTargetType.Discard.
                    // No tiene restricción de fase aquí, ya que el descarte forzado se gestiona por el TurnManager.
                    else if (myTargetType == TargetType.Discard)
                    {
                        Debug.Log($"[DropTarget] Soltada carta '{droppedCardData.cardID}' (Instance ID: {droppedCardData.instanceID}) en objetivo 'Discard'. Descartando directamente.");
                        // Aquí se llamaría al DiscardCardInternal directamente, si esa es la intención para esta zona.
                        CardManager.Instance.DiscardCardInternal(droppedCardData);
                    }
                    else 
                    {
                        Debug.LogWarning($"[DropTarget] Tipo de objetivo '{myTargetType}' no manejado para la carta {droppedCardData.cardID}. La carta regresará a la mano.");
                    }
                }
                else
                {
                    Debug.LogWarning("[DropTarget] El CardUI del objeto dropeado no tiene CardData asignado.");
                }
            }
            else
            {
                Debug.LogWarning($"[DropTarget] El objeto arrastrado '{droppedObject.name}' no tiene el script CardUI. No es una carta jugable.");
            }
        }
    }
}