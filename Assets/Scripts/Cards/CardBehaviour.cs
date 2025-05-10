using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class CardBehaviour : MonoBehaviour, 
    IBeginDragHandler, 
    IDragHandler, 
    IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Selectable selectable;
    private Vector2 initialPosition;
    private bool isIdle = true;
    private float idleTime = 0f;
    private bool isHovering = false;
    private Vector2 hoverLocalCursor;
    private Quaternion targetRotation;
    private Vector3 targetScale;


    [Header("Hover Settings")]
    [SerializeField] private float hoverDuration = 0.2f;
    [SerializeField] private float hoverManualTiltAmount = 10f; // Controla la inclinación máxima
    [SerializeField] private float hoverAutoTiltAmount = 2f;    // Controla el bamboleo automático
    [SerializeField] private float hoverTiltSpeed = 10f;        // Velocidad de interpolación
    [SerializeField] private float hoverShakeStrength = 8f;


    [Header("Drag Settings")]
    [SerializeField] private float lerpSpeed = 10f;
    [SerializeField] private float hoverScale = 1.5f;


    private Vector3 originalScale;
    private bool isDragging = false;
    private Coroutine scaleCoroutine;
    private Coroutine moveCoroutine;
    private Coroutine shakeCoroutine;

    private Vector2 lastCursorPosition;
    private bool cursorStopped = false;
    private Vector2 currentTargetPosition;
    private Quaternion shakeOffset = Quaternion.identity;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        selectable = GetComponent<Selectable>();
        originalScale = transform.localScale;
    }

    private void Start()
    {
        initialPosition = rectTransform.anchoredPosition;
    }
    private void Update()
    {
        if (isDragging)
        {
            Vector2 difference = currentTargetPosition - rectTransform.anchoredPosition;
            float followSpeed = lerpSpeed;
            rectTransform.anchoredPosition += difference * Mathf.Clamp01(followSpeed * Time.deltaTime);

            if (difference.magnitude < 0.5f)
                rectTransform.anchoredPosition = currentTargetPosition;

            float maxRotation = 15f;
            float rotationZ = Mathf.Clamp(-difference.x, -maxRotation, maxRotation);
            targetRotation = Quaternion.Euler(0, 0, rotationZ);
            targetScale = originalScale;
        }
        else if (isHovering)
        {
            Vector3 cardScreenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rectTransform.position);
            Vector3 offset = cardScreenPos - Input.mousePosition;

            float normX = Mathf.Clamp(offset.x / (rectTransform.rect.width * canvas.scaleFactor * 0.5f), -1f, 1f);
            float normY = Mathf.Clamp(offset.y / (rectTransform.rect.height * canvas.scaleFactor * 0.5f), -1f, 1f);

            float savedIndex = 0f;
            float sine = Mathf.Sin(Time.time + savedIndex);
            float cosine = Mathf.Cos(Time.time + savedIndex);

            float tiltX = -normY * hoverManualTiltAmount + sine * hoverAutoTiltAmount;
            float tiltY = -normX * hoverManualTiltAmount + cosine * hoverAutoTiltAmount;

            targetRotation = Quaternion.Euler(tiltX, tiltY, 0);
            targetScale = originalScale;
            rectTransform.anchoredPosition = initialPosition;
        }
        else if (isIdle)
        {
            idleTime += Time.deltaTime;
            float circleSpeed = 1.0f;
            float radius = 0.8f;
            float normX = Mathf.Cos(idleTime * circleSpeed) * radius;
            float normY = Mathf.Sin(idleTime * circleSpeed) * radius;

            float savedIndex = 0f;
            float sine = Mathf.Sin(Time.time + savedIndex);
            float cosine = Mathf.Cos(Time.time + savedIndex);

            float tiltX = -normY * hoverManualTiltAmount + sine * hoverAutoTiltAmount;
            float tiltY = -normX * hoverManualTiltAmount + cosine * hoverAutoTiltAmount;

            targetRotation = Quaternion.Euler(tiltX, tiltY, 0);
            targetScale = originalScale;
            rectTransform.anchoredPosition = initialPosition;
        }
        else
        {
            targetRotation = Quaternion.identity;
            targetScale = originalScale;
            rectTransform.anchoredPosition = Vector2.Lerp(
                rectTransform.anchoredPosition,
                initialPosition,
                10f * Time.deltaTime
            );
        }

        // Aplica la interpolación suave SIEMPRE
        rectTransform.rotation = Quaternion.Lerp(
            rectTransform.rotation,
            targetRotation,
            hoverTiltSpeed * Time.deltaTime
        );
        rectTransform.rotation = Quaternion.Lerp(
            rectTransform.rotation,
            targetRotation * shakeOffset,
            hoverTiltSpeed * Time.deltaTime
        );
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        isIdle = false;
        if (!selectable.interactable) return;

        isDragging = true;
        canvasGroup.blocksRaycasts = false;

        // Escalado al iniciar el drag
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleCard(originalScale * hoverScale));

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out currentTargetPosition
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out currentTargetPosition
        );
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        // Vuelve al tamaño original al soltar la carta
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleCard(originalScale));

        isIdle = false;

        if (!eventData.pointerEnter || !eventData.pointerEnter.CompareTag("DropZone"))
        {
            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(MoveToPosition(initialPosition, 0.2f));
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isIdle = false;
        isHovering = true;
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(HoverShake(0.12f, hoverShakeStrength));
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isIdle = true;
        isHovering = false;
    }

    private System.Collections.IEnumerator ScaleCard(Vector3 targetScale)
    {
        Vector3 initialScale = transform.localScale;
        float timeElapsed = 0f;

        while (timeElapsed < hoverDuration)
        {
            transform.localScale = Vector3.Lerp(
                initialScale, 
                targetScale, 
                timeElapsed / hoverDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
        scaleCoroutine = null;
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
    private System.Collections.IEnumerator MoveToPosition(Vector2 targetPosition, float duration, bool activateIdle = false)
    {
        Vector2 start = rectTransform.anchoredPosition;
        float time = 0f;
        while (time < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(start, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = targetPosition;
        moveCoroutine = null;
        if (activateIdle)
            isIdle = true;
    }
}