// CardBehaviour2.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class CardBehaviour2 : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerUpHandler,
    IPointerDownHandler
{
    // Flag estática para controlar el dragging global ===
    public static bool IsAnyCardDragging = false;
    // ===============================================================


    private RectTransform rectTransform;
    private Canvas parentCanvas; // El Canvas principal que contiene esta carta 
    private Canvas cardCanvas;   // El Canvas propio de esta carta (para el sorting order)
    private CanvasGroup canvasGroup;
    private Selectable selectable;
    private LayoutElement layoutElement;

    private bool isIdleAnimationActive = true;
    private bool isHovering = false;
    private bool isDragging = false;
    private bool isReturning = false;
    private bool wasDragged = false;
    private bool isOverDropTarget = false;

    private float idleTime = 0f;
    private Quaternion targetRotation;
    private Vector3 targetScale;
    private Vector3 originalScale;

    private Vector3 trueBaseLayoutPosition;
    private Vector3 currentLocalOffset;

    private Coroutine exitCoroutine;
    private Coroutine enterCoroutine;
    private Coroutine shakeCoroutine;
    private Coroutine returnAnimationCoroutine;
    private Coroutine moveCoroutine;

    private float lastShakeTime = -10f;
    private Quaternion shakeOffset = Quaternion.identity;

    private Vector2 initialDragOffset;

    private int originalSortingOrder = -1; // Guarda el sorting order base de la carta
    public bool IsHovering => isHovering;

    // === Variable para el desplazamiento aleatorio de la fase del idle tilt ===
    private float randomIdlePhaseOffset;
    // =================================================================================

    [Header("Idle Animation Settings")]
    [SerializeField] private AnimationCurve smoothCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Sorting Settings (UI)")]
    [SerializeField] private int hoverUISortingOrder = 500; // Sorting Order para el estado de hover
    [SerializeField] private int draggingUISortingOrder = 1000; // Sorting Order para el estado de arrastre (más alto)


    [Header("Hover Settings")]
    [SerializeField] private float hoverDuration = 0.2f;
    [SerializeField] private float hoverManualTiltAmount = 10f;
    [SerializeField] private float hoverAutoTiltAmount = 2f;
    [SerializeField] private float hoverTiltSpeed = 10f;
    [SerializeField] private float hoverShakeStrength = 8f;
    [SerializeField] private float hoverExitDelay = 0.2f;
    [SerializeField] private float hoverEnterDelay = 0.02f;
    [SerializeField] private float shakeCooldown = 0.2f;
    [SerializeField] private float hoverVerticalOffset = 50f;

    [SerializeField] private float hoverScale = 1.2f;

    [Header("Drag Settings")]
    [SerializeField] private float dragLerpSpeed = 10f;
    [SerializeField] private float returnSpeed = 8f;
    [SerializeField] private float dragScaleSpeed = 20f;



    private bool basePositionInitialized = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>(); 
        canvasGroup = GetComponent<CanvasGroup>();
        selectable = GetComponent<Selectable>();
        layoutElement = GetComponent<LayoutElement>();

        cardCanvas = GetComponent<Canvas>();
        if (cardCanvas == null)
        {
            cardCanvas = gameObject.AddComponent<Canvas>();
        }
        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        cardCanvas.overrideSorting = true;
        cardCanvas.sortingLayerID = parentCanvas.sortingLayerID; // Hereda la capa de sorting del Canvas padre
        cardCanvas.renderMode = parentCanvas.renderMode; // Hereda el modo de renderizado
        cardCanvas.worldCamera = parentCanvas.worldCamera; // Hereda la cámara del Canvas padre

        originalSortingOrder = cardCanvas.sortingOrder;

        originalScale = transform.localScale;
        targetScale = originalScale;
        currentLocalOffset = Vector3.zero;


        randomIdlePhaseOffset = Random.Range(0f, 1000f); 
    }

    private void Start()
    {
        StartCoroutine(InitializeBasePosition());
    }

    private IEnumerator InitializeBasePosition()
    {
        yield return null;
        yield return null;

        if (rectTransform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform.parent.GetComponent<RectTransform>());
        }

        trueBaseLayoutPosition = rectTransform.localPosition;
        basePositionInitialized = true;
    }

    private void Update()
    {
        if (!basePositionInitialized)
        {
            return;
        }

        rectTransform.rotation = Quaternion.Lerp(
            rectTransform.rotation,
            targetRotation * shakeOffset,
            hoverTiltSpeed * Time.deltaTime
        );

        float currentScaleLerpSpeed;

        if (isDragging || isReturning)
        {
            currentScaleLerpSpeed = dragScaleSpeed;
        }
        else
        {
            currentScaleLerpSpeed = 1f / hoverDuration;
        }

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            currentScaleLerpSpeed * Time.deltaTime
        );

        if (isDragging)
        {



            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                Input.mousePosition,
                parentCanvas.worldCamera,
                out localPoint
            );

            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, localPoint + initialDragOffset, dragLerpSpeed * Time.deltaTime);

            float maxRotation = 15f;
            float rotationZ = Mathf.Clamp(-(Input.mousePosition.x - RectTransformUtility.WorldToScreenPoint(parentCanvas.worldCamera, rectTransform.position).x) * 0.1f, -maxRotation, maxRotation);
            targetRotation = Quaternion.Euler(0, 0, rotationZ);

            if (!isOverDropTarget)
                targetScale = originalScale * 1.5f;
            currentLocalOffset = Vector3.up * hoverVerticalOffset;
        }
        else if (!isReturning)
        {
            if (isHovering)
            {
                Vector3 cardScreenPos = RectTransformUtility.WorldToScreenPoint(parentCanvas.worldCamera, rectTransform.position);
                Vector3 offset = cardScreenPos - Input.mousePosition;

                float normX = Mathf.Clamp(offset.x / (rectTransform.rect.width * parentCanvas.scaleFactor * 0.5f), -1f, 1f);
                float normY = Mathf.Clamp(offset.y / (rectTransform.rect.height * parentCanvas.scaleFactor * 0.5f), -1f, 1f);

                float sine = Mathf.Sin(Time.time * hoverAutoTiltAmount + randomIdlePhaseOffset);
                float cosine = Mathf.Cos(Time.time * hoverAutoTiltAmount + randomIdlePhaseOffset);

                float tiltX = normY * hoverManualTiltAmount + sine * hoverAutoTiltAmount;
                float tiltY = -normX * hoverManualTiltAmount + cosine * hoverAutoTiltAmount;

                targetRotation = Quaternion.Euler(tiltX, tiltY, 0);

                if (IsAnyCardDragging)
                {
                    targetScale = originalScale;
                }
                else
                {
                    targetScale = originalScale * hoverScale;
                }

                currentLocalOffset = Vector3.up * hoverVerticalOffset;
            }
            else if (isIdleAnimationActive)
            {
                idleTime += Time.deltaTime;

                float circleSpeed = 0.7f;
                float radius = 1.2f; 

                float currentIdlePhase = (idleTime * circleSpeed) + randomIdlePhaseOffset;
                float normX = Mathf.Cos(currentIdlePhase) * radius;
                float normY = Mathf.Sin(currentIdlePhase) * radius;

                float sine = Mathf.Sin(Time.time * hoverAutoTiltAmount + randomIdlePhaseOffset);
                float cosine = Mathf.Cos(Time.time * hoverAutoTiltAmount + randomIdlePhaseOffset);

                float idleTiltOverallMultiplier = 1.5f; 

                float tiltX = (normY * hoverManualTiltAmount + sine * hoverAutoTiltAmount) * idleTiltOverallMultiplier;
                float tiltY = (-normX * hoverManualTiltAmount + cosine * hoverAutoTiltAmount) * idleTiltOverallMultiplier;

                targetRotation = Quaternion.Euler(tiltX, tiltY, 0);
                targetScale = originalScale;

                currentLocalOffset = Vector3.zero;
            }
            else
            {
                targetRotation = Quaternion.identity;
                targetScale = originalScale;
                currentLocalOffset = Vector3.zero;
            }

            Vector3 targetPositionWithOffset = trueBaseLayoutPosition + currentLocalOffset;
            rectTransform.localPosition = Vector3.Lerp(
                rectTransform.localPosition,
                targetPositionWithOffset,
                dragLerpSpeed * Time.deltaTime
            );
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        wasDragged = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("BEGIN DRAG");
        wasDragged = false;

        if (TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.ActionPhase &&
            TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.DiscardPostShot)
        {
            eventData.pointerDrag = null; // Cancela el drag
            return;
        }
        if (TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.ActionPhase)
        {
            if (TurnManager.Instance.CurrentPhase == TurnManager.TurnPhase.DiscardPostShot)
            {
                Debug.LogWarning($"Aquí tambien puede empezar el drag");
            }
            else
            {
                Debug.LogWarning($"[CardBehaviour2] OnBeginDrag llamado fuera de la ActionPhase para {gameObject.name}. Ignorando.");
                return;
            }
        }



        IsAnyCardDragging = true;

        if (returnAnimationCoroutine != null) StopCoroutine(returnAnimationCoroutine);
        isReturning = false;

        isIdleAnimationActive = false;
        isHovering = false;
        isDragging = true;

        canvasGroup.blocksRaycasts = false;

        if (cardCanvas != null)
        {
            cardCanvas.sortingOrder = draggingUISortingOrder;
        }

        trueBaseLayoutPosition = rectTransform.localPosition;

        if (layoutElement != null)
        {
            layoutElement.enabled = false;
        }

        Vector2 localCursorPointInCanvas;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out localCursorPointInCanvas
        );
        initialDragOffset = rectTransform.anchoredPosition - localCursorPointInCanvas;

        targetScale = originalScale * 1.5f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        wasDragged = true;
        if (!isDragging) return;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[CardBehaviour2] OnEndDrag llamado para {gameObject.name}");
        if (!isDragging) return;

        IsAnyCardDragging = false;

        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        targetScale = originalScale;

        if (returnAnimationCoroutine != null) StopCoroutine(returnAnimationCoroutine);

        isReturning = true;
        returnAnimationCoroutine = StartCoroutine(ReturnToOriginalPosition());

        isHovering = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!wasDragged && eventData.button == PointerEventData.InputButton.Left && !isDragging)
        {
            Debug.Log("[CardBehaviour2] CLICK SEGURO para la PREVIEW");
            var cardUI = GetComponent<CardUI>();
            if (cardUI == null)
            {
                Debug.LogError("CardUI es null en " + gameObject.name);
                return;
            }
            if (CardPreviewManager.Instance == null)
            {
                Debug.LogError("CardPreviewManager.Instance es null");
                return;
            }
            if (cardUI.CardSprite == null)
            {
                Debug.LogError("CardSprite es null en " + gameObject.name);
                return;
            }
            CardPreviewManager.Instance.ShowCard(cardUI.CardSprite);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging || isReturning) return;

        if (exitCoroutine != null) StopCoroutine(exitCoroutine);
        if (enterCoroutine != null) StopCoroutine(enterCoroutine);
        enterCoroutine = StartCoroutine(HoverEnterDelay());

        if (cardCanvas != null)
        {
            if (!isHovering)
                originalSortingOrder = cardCanvas.sortingOrder;

            cardCanvas.sortingOrder = hoverUISortingOrder; 
        }
        AudioManager.Instance?.PlayCardHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging || isReturning) return;

        if (exitCoroutine != null) StopCoroutine(exitCoroutine);
        exitCoroutine = StartCoroutine(HoverExitDelay());
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        Debug.Log($"[CardBehaviour2] {gameObject.name} ReturnToOriginalPosition: from {rectTransform.localPosition} to {trueBaseLayoutPosition}");
        Vector3 startPosition = rectTransform.localPosition;
        Vector3 endPosition = new Vector3(trueBaseLayoutPosition.x, 0, trueBaseLayoutPosition.z); // Fuerza Y=0

        float t = 0f;

        float distance = Vector3.Distance(startPosition, endPosition);
        if (distance < 0.1f)
        {
            rectTransform.localPosition = endPosition;
            if (layoutElement != null)
            {
                layoutElement.enabled = true;
            }
            if (rectTransform.parent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform.parent.GetComponent<RectTransform>());
            }
            isReturning = false;
            isIdleAnimationActive = true;
            returnAnimationCoroutine = null;

            if (cardCanvas != null && !isDragging)
            {
                cardCanvas.sortingOrder = originalSortingOrder;
            }
            yield break;
        }

        while (t < 1f)
        {
            if (isDragging)
            {
                isReturning = false;
                yield break;
            }

            t += Time.deltaTime * returnSpeed;

            rectTransform.localPosition = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        rectTransform.localPosition = endPosition;

        if (layoutElement != null)
        {
            layoutElement.enabled = true;
        }

        if (rectTransform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform.parent.GetComponent<RectTransform>());
            trueBaseLayoutPosition = rectTransform.localPosition;
        }

        isReturning = false;
        isIdleAnimationActive = true;
        returnAnimationCoroutine = null;

        if (cardCanvas != null && !isDragging)
        {
            cardCanvas.sortingOrder = originalSortingOrder;
        }
    }

    private System.Collections.IEnumerator HoverShake(float duration = 0.12f, float strength = 8f)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float shakeAmount = Mathf.Sin(elapsed * 100f) * strength * (1f - (elapsed / duration));
            shakeOffset = Quaternion.Euler(0, 0, shakeAmount);
            elapsed += Time.deltaTime;
            yield return null;
        }
        shakeOffset = Quaternion.identity;
        shakeCoroutine = null;
    }

    private System.Collections.IEnumerator HoverExitDelay()
    {
        yield return new WaitForSeconds(hoverExitDelay);

        isIdleAnimationActive = true;
        isHovering = false;
        exitCoroutine = null;

        if (cardCanvas != null && !isDragging && !isReturning)
        {
            cardCanvas.sortingOrder = originalSortingOrder;
        }
    }

    private System.Collections.IEnumerator HoverEnterDelay()
    {
        yield return new WaitForSeconds(hoverEnterDelay);
        isIdleAnimationActive = false;
        isHovering = true;

        if (Time.time - lastShakeTime > shakeCooldown)
        {
            lastShakeTime = Time.time;
            if (shakeCoroutine != null)
                StopCoroutine(shakeCoroutine);
            shakeCoroutine = StartCoroutine(HoverShake(0.12f, hoverShakeStrength));
        }
        enterCoroutine = null;
    }

    public void AnimateToLayoutPosition(float duration = 0.2f)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(AnimateToCurrentAnchoredPosition(duration));
    }

    private IEnumerator AnimateToCurrentAnchoredPosition(float duration)
    {
        Vector2 start = rectTransform.anchoredPosition;
        yield return null; 
        Vector2 end = rectTransform.anchoredPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float smoothT = t * t * (3f - 2f * t);
            rectTransform.anchoredPosition = Vector2.Lerp(start, end, smoothT);
            yield return null;
        }
        rectTransform.anchoredPosition = end;
        moveCoroutine = null;
    }

    public void UpdateBaseLayoutPosition()
    {
        if (rectTransform != null)
            trueBaseLayoutPosition = rectTransform.localPosition;
        basePositionInitialized = true;
    }

    public void AnimateToCurrentLayoutPosition(float duration = 0.3f)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        rectTransform.DOKill();
        rectTransform.DOLocalMove(rectTransform.localPosition, duration).SetEase(Ease.InOutQuad);

    }

    public void SetDragOverTargetScale(bool isOverTarget)
    {
        isOverDropTarget = isOverTarget;
        if (isOverDropTarget)
            targetScale = originalScale * 0.5f;
        else if (isDragging)
            targetScale = originalScale * 1.5f;
        else
            targetScale = originalScale;
    }

}