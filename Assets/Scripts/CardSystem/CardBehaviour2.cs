using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(LayoutElement))]
public class CardBehaviour2 : MonoBehaviour,
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
    private LayoutElement layoutElement;

    private bool isIdleAnimationActive = true; 
    private bool isHovering = false;
    private bool isDragging = false;
    private bool isReturning = false; 

    private float idleTime = 0f;
    private Quaternion targetRotation;
    private Vector3 targetScale;
    private Vector3 originalScale; 
    
    // RENOMBRADA: Esta es la posición base que el Layout Group le da a la carta.
    // Debería ser la "verdadera" posición del slot en el layout.
    private Vector3 trueBaseLayoutPosition; 
    private Vector3 currentLocalOffset; // Offset para animaciones (hover, etc.)

    private Coroutine exitCoroutine;
    private Coroutine enterCoroutine;
    private Coroutine shakeCoroutine;
    private Coroutine returnAnimationCoroutine; 

    private float lastShakeTime = -10f; 
    private Quaternion shakeOffset = Quaternion.identity; 

    private Vector2 initialDragOffset; 

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

    [Header("Drag Settings")]
    [SerializeField] private float dragLerpSpeed = 10f; 
    [SerializeField] private float hoverScale = 1.2f;
    [SerializeField] private float returnSpeed = 8f; 

    private Vector3 lastLoggedLocalPosition = Vector3.zero; 
    private bool basePositionInitialized = false; 

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        selectable = GetComponent<Selectable>();
        layoutElement = GetComponent<LayoutElement>();

        originalScale = transform.localScale; 
        currentLocalOffset = Vector3.zero; 

        Debug.Log($"[Awake - {gameObject.name}] localPosition: {rectTransform.localPosition}, anchoredPosition: {rectTransform.anchoredPosition}");
        Debug.Log($"[Awake - {gameObject.name}] LayoutElement.enabled: {layoutElement.enabled}");
    }

    private void Start()
    {
        StartCoroutine(InitializeBasePosition());
        Debug.Log($"[Start - {gameObject.name}] Started InitializeBasePosition Coroutine.");
    }

    private IEnumerator InitializeBasePosition()
    {
        // Espera dos frames para que el Layout Group se construya y la UI se renderice
        yield return null; 
        yield return null; 

        if (rectTransform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform.parent.GetComponent<RectTransform>());
        }

        // Captura la posición local que el Layout Group le ha asignado inicialmente
        // Esta es la posición "verdadera" que el Layout Group le da.
        trueBaseLayoutPosition = rectTransform.localPosition; 
        basePositionInitialized = true; 

        Debug.Log($"[InitializeBasePosition - {gameObject.name}] trueBaseLayoutPosition set to: {trueBaseLayoutPosition} (After delay and force rebuild)");
        Debug.Log($"[InitializeBasePosition - {gameObject.name}] current localPosition: {rectTransform.localPosition}");
        Debug.Log($"[InitializeBasePosition - {gameObject.name}] LayoutElement.enabled: {layoutElement.enabled}");
    }

    private void Update()
    {
        if (!basePositionInitialized) 
        {
            return; 
        }

        // --- DEBUGGING: Detección de reseteo a ZERO o cambio brusco ---
        if (rectTransform.localPosition != lastLoggedLocalPosition)
        {
            if (rectTransform.localPosition == Vector3.zero && Time.frameCount > 10 && !isDragging && !isReturning)
            {
                Debug.LogWarning($"[Update ALERT - {gameObject.name}] localPosition REVERTED to ZERO! | Frame: {Time.frameCount} | Last known: {lastLoggedLocalPosition} | isDragging: {isDragging}, isReturning: {isReturning}");
            }
            if (!isDragging && !isReturning && Vector3.Distance(rectTransform.localPosition, lastLoggedLocalPosition) > 1f)
            {
                 Debug.Log($"[Update DEBUG - {gameObject.name}] localPosition changed unexpectedly. From: {lastLoggedLocalPosition} To: {rectTransform.localPosition} | Frame: {Time.frameCount} | isDragging: {isDragging}, isReturning: {isReturning}");
            }
            lastLoggedLocalPosition = rectTransform.localPosition;
        }

        // Interpolación de Rotación y Escala (siempre se aplica)
        rectTransform.rotation = Quaternion.Lerp(
            rectTransform.rotation,
            targetRotation * shakeOffset, 
            hoverTiltSpeed * Time.deltaTime
        );
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            hoverDuration * Time.deltaTime
        );

        // --- Lógica de Estado Principal para Determinar Target (Rotación, Escala, Offset) ---
        if (isDragging)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out localPoint
            );
            
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, localPoint + initialDragOffset, dragLerpSpeed * Time.deltaTime);

            float maxRotation = 15f;
            float rotationZ = Mathf.Clamp(-(Input.mousePosition.x - RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rectTransform.position).x) * 0.1f, -maxRotation, maxRotation);
            targetRotation = Quaternion.Euler(0, 0, rotationZ);
            targetScale = originalScale * hoverScale;
            currentLocalOffset = Vector3.up * hoverVerticalOffset; 
        }
        // Si no estamos arrastrando Y NO estamos en el proceso de retorno animado
        else if (!isReturning) 
        {
            // Lógica de Hover y Idle
            if (isHovering)
            {
                Vector3 cardScreenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rectTransform.position);
                Vector3 offset = cardScreenPos - Input.mousePosition;

                float normX = Mathf.Clamp(offset.x / (rectTransform.rect.width * canvas.scaleFactor * 0.5f), -1f, 1f);
                float normY = Mathf.Clamp(offset.y / (rectTransform.rect.height * canvas.scaleFactor * 0.5f), -1f, 1f);

                float savedIndex = 0f; 
                float sine = Mathf.Sin(Time.time + savedIndex);
                float cosine = Mathf.Cos(Time.time + savedIndex);

                float tiltX = normY * hoverManualTiltAmount + sine * hoverAutoTiltAmount;
                float tiltY = -normX * hoverManualTiltAmount + cosine * hoverAutoTiltAmount;

                targetRotation = Quaternion.Euler(tiltX, tiltY, 0);
                targetScale = originalScale * hoverScale;
                currentLocalOffset = Vector3.up * hoverVerticalOffset; 
            }
            else if (isIdleAnimationActive) 
            {
                idleTime += Time.deltaTime; 

                float circleSpeed = 1.0f; 
                float radius = 0.8f;      

                float normX = Mathf.Cos(idleTime * circleSpeed) * radius;
                float normY = Mathf.Sin(idleTime * circleSpeed) * radius;

                float savedIndex = 0f; 
                float sine = Mathf.Sin(Time.time * hoverAutoTiltAmount + savedIndex); 
                float cosine = Mathf.Cos(Time.time * hoverAutoTiltAmount + savedIndex);

                float tiltX = normY * hoverManualTiltAmount + sine * hoverAutoTiltAmount;
                float tiltY = -normX * hoverManualTiltAmount + cosine * hoverAutoTiltAmount;

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

            // Interpolación de la Posición Local para Hover/Idle
            // SUMAR currentLocalOffset a trueBaseLayoutPosition
            Vector3 targetPositionWithOffset = trueBaseLayoutPosition + currentLocalOffset;
            rectTransform.localPosition = Vector3.Lerp(
                rectTransform.localPosition,
                targetPositionWithOffset,
                dragLerpSpeed * Time.deltaTime 
            );
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[OnBeginDrag - {gameObject.name}] Drag Started. Current localPosition: {rectTransform.localPosition}");
        Debug.Log($"[OnBeginDrag - {gameObject.name}] LayoutElement.enabled: {layoutElement.enabled}");

        if (!selectable.interactable) return;

        if (returnAnimationCoroutine != null) StopCoroutine(returnAnimationCoroutine);
        isReturning = false; 
        Debug.Log($"[OnBeginDrag - {gameObject.name}] isReturning set to FALSE.");

        isIdleAnimationActive = false; 
        isHovering = false; 
        isDragging = true; 

        canvasGroup.blocksRaycasts = false; 

        // CRÍTICO: DESHABILITAR el LayoutElement al inicio del drag.
        if (layoutElement != null)
        {
            layoutElement.enabled = false;
        }
        Debug.Log($"[OnBeginDrag - {gameObject.name}] LayoutElement.enabled set to FALSE.");

        Vector2 localCursorPointInCanvas;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position, 
            canvas.worldCamera,
            out localCursorPointInCanvas
        );
        initialDragOffset = rectTransform.anchoredPosition - localCursorPointInCanvas;
        
        Debug.Log($"[OnBeginDrag - {gameObject.name}] initialDragOffset calculated: {initialDragOffset}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[OnEndDrag - {gameObject.name}] Drag Ended. Current localPosition: {rectTransform.localPosition}");
        Debug.Log($"[OnEndDrag - {gameObject.name}] LayoutElement.enabled (before logic): {layoutElement.enabled}"); 

        if (!isDragging) return;

        isDragging = false; 
        canvasGroup.blocksRaycasts = true; 

        if (returnAnimationCoroutine != null) StopCoroutine(returnAnimationCoroutine);
        Debug.Log($"[OnEndDrag - {gameObject.name}] Stopped previous return animation (if any).");
        
        isReturning = true;
        returnAnimationCoroutine = StartCoroutine(ReturnToOriginalPosition());
        Debug.Log($"[OnEndDrag - {gameObject.name}] Started ReturnToOriginalPosition Coroutine. isReturning set to TRUE.");

        isHovering = false; 
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging || isReturning) return;
        if (exitCoroutine != null) StopCoroutine(exitCoroutine);
        if (enterCoroutine != null) StopCoroutine(enterCoroutine);
        enterCoroutine = StartCoroutine(HoverEnterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging || isReturning) return;
        if (exitCoroutine != null) StopCoroutine(exitCoroutine);
        exitCoroutine = StartCoroutine(HoverExitDelay());
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        Vector3 startPosition = rectTransform.localPosition; 
        // El destino es la posición 'verdadera' del Layout Group.
        Vector3 endPosition = trueBaseLayoutPosition;  

        float t = 0f; 

        Debug.Log($"[ReturnToOriginalPosition - {gameObject.name}] Starting return animation. From: {startPosition} To: {endPosition}");
        Debug.Log($"[ReturnToOriginalPosition - {gameObject.name}] LayoutElement.enabled at start of coroutine: {layoutElement.enabled}"); 

        float distance = Vector3.Distance(startPosition, endPosition);
        if (distance < 0.1f) 
        {
            Debug.Log($"[ReturnToOriginalPosition - {gameObject.name}] Card already very close to target. Skipping animation.");
            rectTransform.localPosition = endPosition; 
            if (layoutElement != null)
            {
                layoutElement.enabled = true;
                Debug.Log($"[ReturnToOriginalPosition - {gameObject.name}] LayoutElement.enabled set to TRUE (skipped anim).");
            }
            if (rectTransform.parent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform.parent.GetComponent<RectTransform>());
                Debug.Log($"[ReturnToOriginalPosition - {gameObject.name}] Layout Rebuilt (skipped anim).");
            }
            isReturning = false; 
            isIdleAnimationActive = true; 
            returnAnimationCoroutine = null; 
            yield break;
        }


        while (t < 1f)
        {
            if (isDragging) 
            {
                Debug.LogWarning($"[ReturnToOriginalPosition - {gameObject.name}] Animation INTERRUPTED by new drag. Exiting coroutine.");
                isReturning = false;
                yield break; 
            }

            t += Time.deltaTime * returnSpeed; 
            
            rectTransform.localPosition = Vector3.Lerp(startPosition, endPosition, t);
            yield return null; 
        }

        rectTransform.localPosition = endPosition; 
        Debug.Log($"[ReturnToOriginalPosition - {gameObject.name}] Animation finished. Final localPosition: {rectTransform.localPosition}");

        if (layoutElement != null)
        {
            layoutElement.enabled = true;
            Debug.Log($"[ReturnToOriginalPosition - {gameObject.name}] LayoutElement.enabled set to TRUE.");
        }

        // MUY IMPORTANTE: Recapturar trueBaseLayoutPosition aquí.
        // Después de que el Layout Group ha re-integrado la carta, su posición real
        // dentro del layout podría haber cambiado si otras cartas se movieron o añadieron/quitaron.
        if (rectTransform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform.parent.GetComponent<RectTransform>());
            trueBaseLayoutPosition = rectTransform.localPosition; // ¡CAPTURA LA NUEVA POSICIÓN REAL DEL LAYOUT!
            Debug.Log($"[ReturnToOriginalPosition - {gameObject.name}] Layout Rebuilt after animation completion. NEW trueBaseLayoutPosition: {trueBaseLayoutPosition}");
        }


        isReturning = false; 
        isIdleAnimationActive = true; 
        returnAnimationCoroutine = null; 
        Debug.Log($"[ReturnToOriginalPosition - {gameObject.name}] Coroutine completed. isReturning set to FALSE.");
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
}