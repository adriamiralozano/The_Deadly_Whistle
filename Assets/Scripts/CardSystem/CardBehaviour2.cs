using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

// Asegura que estos componentes estén presentes en el GameObject
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
    // Componentes cacheables
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Selectable selectable;
    private LayoutElement layoutElement;

    // Estados de animación
    private bool isIdleAnimationActive = true; 
    private bool isHovering = false;
    private bool isDragging = false;

    // Variables de animación
    private float idleTime = 0f;
    private Quaternion targetRotation;
    private Vector3 targetScale;
    private Vector3 originalScale; 
    
    // **CLAVE**: Posición base dada por el Layout Group, y offset adicional para animaciones
    private Vector3 baseLocalPosition; 
    private Vector3 currentLocalOffset; 

    // Coroutines de control de delay y shake
    private Coroutine exitCoroutine;
    private Coroutine enterCoroutine;
    private Coroutine shakeCoroutine;
    private float lastShakeTime = -10f; 
    private Quaternion shakeOffset = Quaternion.identity; 

    // Configuraciones de Hover (expuestas en el Inspector)
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

    // Configuraciones de Drag (expuestas en el Inspector)
    [Header("Drag Settings")]
    [SerializeField] private float dragLerpSpeed = 10f; 
    [SerializeField] private float hoverScale = 1.2f;

    // Flags para controlar la depuración de logs iniciales
    private bool hasLoggedInitialUpdate = false; 
    private Vector3 lastLoggedLocalPosition = Vector3.zero; 

    // Flag para saber si la baseLocalPosition ha sido inicializada correctamente
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
    }

    private void Start()
    {
        // En lugar de capturar baseLocalPosition directamente aquí, iniciamos una coroutine
        // para esperar al menos un frame (o hasta que el layout esté listo).
        StartCoroutine(InitializeBasePosition());
        Debug.Log($"[Start - {gameObject.name}] Started InitializeBasePosition Coroutine.");
    }

    // Coroutine para inicializar baseLocalPosition
    private IEnumerator InitializeBasePosition()
    {
        // Esperar un frame (o hasta que el Layout Group haya hecho su pase inicial)
        yield return null; 

        // Forzar una reconstrucción inmediata del layout para asegurar que las posiciones están actualizadas.
        // Esto es lo que OnEndDrag hace para corregir la posición, replicamos ese comportamiento.
        if (rectTransform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform.parent.GetComponent<RectTransform>());
        }

        baseLocalPosition = rectTransform.localPosition; 
        basePositionInitialized = true; // Marca que la posición base ha sido capturada

        Debug.Log($"[InitializeBasePosition - {gameObject.name}] baseLocalPosition set to: {baseLocalPosition} (After delay and force rebuild)");
        Debug.Log($"[InitializeBasePosition - {gameObject.name}] current localPosition: {rectTransform.localPosition}");
    }


    private void Update()
    {
        // NO hacer nada con la posición hasta que basePositionInitialized sea true
        if (!basePositionInitialized && Time.frameCount > 1) // Permitimos un par de frames para que se inicialice
        {
            Debug.Log($"[Update - {gameObject.name}] Waiting for baseLocalPosition initialization. Frame: {Time.frameCount}");
            // Asegurarse de que la carta no se mueve de su posición default si aún no está inicializada
            rectTransform.localPosition = Vector3.zero; // O dejarla como el layout group la puso. Si se apelotonan, esto es mejor que se muevan.
            return; // Salir de Update hasta que la posición base esté lista
        }

        // --- Logs de Depuración Selectivos en Update ---
        if (!hasLoggedInitialUpdate && Time.frameCount > 1 && Time.frameCount < 10) 
        {
            Debug.Log($"[Update Initial - {gameObject.name}] Frame {Time.frameCount} | IsDrag: {isDragging}, IsHover: {isHovering}, IsIdleAnim: {isIdleAnimationActive} | LocalPos: {rectTransform.localPosition} | BaseLocalPos: {baseLocalPosition} | CurrentLocalOffset: {currentLocalOffset}");
            if (Time.frameCount == 9) 
                hasLoggedInitialUpdate = true;
        }
        
        if (rectTransform.localPosition != lastLoggedLocalPosition && rectTransform.localPosition == Vector3.zero && Time.frameCount > 10)
        {
            Debug.LogWarning($"[Update ALERT - {gameObject.name}] localPosition REVERTED to ZERO! | Frame: {Time.frameCount} | Last known: {lastLoggedLocalPosition}");
        }
        lastLoggedLocalPosition = rectTransform.localPosition;
        // ------------------------------------------------

        // --- Interpolación de Rotación y Escala (SIEMPRE se aplica) ---
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
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, localPoint, dragLerpSpeed * Time.deltaTime);

            float maxRotation = 15f;
            float rotationZ = Mathf.Clamp(-(Input.mousePosition.x - RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rectTransform.position).x) * 0.1f, -maxRotation, maxRotation);
            targetRotation = Quaternion.Euler(0, 0, rotationZ);
            targetScale = originalScale * hoverScale;
            currentLocalOffset = Vector3.up * hoverVerticalOffset; 

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

            float tiltX = normY * hoverManualTiltAmount + sine * hoverAutoTiltAmount;
            float tiltY = -normX * hoverManualTiltAmount + cosine * hoverAutoTiltAmount;

            targetRotation = Quaternion.Euler(tiltX, tiltY, 0);
            targetScale = originalScale * hoverScale;
            currentLocalOffset = Vector3.up * hoverVerticalOffset; 

        }
        else if (isIdleAnimationActive) 
        {
            idleTime += Time.deltaTime; // Incrementa el tiempo para la animación de idle

            // Cálculos para la rotación de bamboleo
            float circleSpeed = 1.0f; // Velocidad de la oscilación
            float radius = 0.8f;      // Radio de la oscilación (no afecta la posición, solo se usa para tilt aquí)

            // Estos normX y normY se usarán para el tilt, NO para la posición
            float normX = Mathf.Cos(idleTime * circleSpeed) * radius;
            float normY = Mathf.Sin(idleTime * circleSpeed) * radius;

            float savedIndex = 0f; // Puedes usar un offset por carta aquí para que cada carta bambolee de forma ligeramente diferente
            float sine = Mathf.Sin(Time.time * hoverAutoTiltAmount + savedIndex); // Usamos hoverAutoTiltAmount como velocidad aquí
            float cosine = Mathf.Cos(Time.time * hoverAutoTiltAmount + savedIndex);

            // Calcula el tilt (rotación) basado en normX, normY y los auto-tilt factors
            float tiltX = normY * hoverManualTiltAmount + sine * hoverAutoTiltAmount;
            float tiltY = -normX * hoverManualTiltAmount + cosine * hoverAutoTiltAmount;

            targetRotation = Quaternion.Euler(tiltX, tiltY, 0); // Aplica la rotación de bamboleo
            targetScale = originalScale;                          // Mantén la escala original en idle
            
            // ¡IMPORTANTE! Asegúrate de que el offset de posición sea CERO en el modo idle
            currentLocalOffset = Vector3.zero; 
            // Si quieres un PEQUEÑO efecto de movimiento vertical en idle, 
            // podrías usar algo como:
            // currentLocalOffset = Vector3.up * (Mathf.Sin(idleTime * 2f) * 5f); // Un pequeño "flotar"
            // Pero ten cuidado con los valores grandes, ya que podrían chocar con el Layout Group
        }
        else 
        {
            targetRotation = Quaternion.identity;
            targetScale = originalScale;
            currentLocalOffset = Vector3.zero; 
            Debug.Log($"[Update - {gameObject.name}] DEFAULT STATE: Setting currentLocalOffset to ZERO!");
        }

        // --- Interpolación de la Posición Local (se aplica si NO estamos arrastrando) ---
        if (!isDragging) 
        {
            Vector3 targetPositionWithOffset = baseLocalPosition + currentLocalOffset;
            rectTransform.localPosition = Vector3.Lerp(
                rectTransform.localPosition,
                targetPositionWithOffset,
                dragLerpSpeed * Time.deltaTime 
            );
        }
    }

    // --- Implementaciones de Interfaces de Eventos UI ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[OnBeginDrag - {gameObject.name}] Drag Started. LocalPos: {rectTransform.localPosition}, BaseLocalPos: {baseLocalPosition}");

        if (!selectable.interactable) return;

        isIdleAnimationActive = false; 
        isHovering = false; 
        isDragging = true; 

        canvasGroup.blocksRaycasts = false; 

        if (layoutElement != null)
        {
            layoutElement.enabled = false;
        }
        
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position, 
            canvas.worldCamera,
            out localPoint
        );
        rectTransform.anchoredPosition = localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[OnEndDrag - {gameObject.name}] Drag Ended. LocalPos: {rectTransform.localPosition}, BaseLocalPos: {baseLocalPosition}");

        if (!isDragging) return;

        isDragging = false; 
        canvasGroup.blocksRaycasts = true; 

        if (layoutElement != null)
        {
            layoutElement.enabled = true;
        }

        if (rectTransform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform.parent.GetComponent<RectTransform>());
            baseLocalPosition = rectTransform.localPosition; 
            Debug.Log($"[OnEndDrag - {gameObject.name}] Layout Rebuilt. New BaseLocalPos: {baseLocalPosition}");
        }
        
        isHovering = false; 
        isIdleAnimationActive = true; 
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging) return;
        Debug.Log($"[OnPointerEnter - {gameObject.name}] Hover Started.");

        if (exitCoroutine != null) StopCoroutine(exitCoroutine);
        if (enterCoroutine != null) StopCoroutine(enterCoroutine);
        enterCoroutine = StartCoroutine(HoverEnterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return;
        Debug.Log($"[OnPointerExit - {gameObject.name}] Hover Ended.");

        if (exitCoroutine != null) StopCoroutine(exitCoroutine);
        exitCoroutine = StartCoroutine(HoverExitDelay());
    }

    // --- Coroutines de Animación ---
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