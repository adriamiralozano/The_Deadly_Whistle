using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class QTEPanelAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float fallDuration = 0.8f;
    [SerializeField] private float bounceHeight = 50f;
    [SerializeField] private Ease fallEase = Ease.OutBounce;
    
    [Header("Panel Settings")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private CanvasGroup canvasGroup;
    
    private Vector2 targetPosition;
    private Vector2 startPosition;
    private bool isAnimating = false;
    
    public bool IsAnimating => isAnimating;
    
    private void Awake()
    {
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();
        
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
            
        // Configurar posición inicial (fuera de pantalla arriba)
        SetupInitialPosition();
    }
    
    private void SetupInitialPosition()
    {
        // Guardar la posición objetivo (centro de pantalla)
        targetPosition = panelRect.anchoredPosition;
        
        // Calcular posición inicial (fuera de pantalla arriba)
        startPosition = new Vector2(targetPosition.x, Screen.height + panelRect.rect.height);
        
        // Posicionar fuera de pantalla inicialmente
        panelRect.anchoredPosition = startPosition;
        
        // Hacer invisible inicialmente
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        
        // Desactivar el panel inicialmente
        gameObject.SetActive(false);
    }
    
    public void ShowPanel()
    {
        if (isAnimating) return;
        
        gameObject.SetActive(true);
        
        StartCoroutine(ShowPanelCoroutine());
    }
    
    public void HidePanel()
    {
        if (isAnimating) return;

        
        StartCoroutine(HidePanelCoroutine());
    }
    
    private IEnumerator ShowPanelCoroutine()
    {
        isAnimating = true;
        
        panelRect.anchoredPosition = startPosition;
        
        // Fade in rápido
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.2f);
        }
        
        // Reproducir sonido de aparición
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyCardAppear(); // O crea un sonido específico para QTE
        }
        
        // Animación de caída con rebote
        panelRect.DOAnchorPos(targetPosition, fallDuration)
            .SetEase(fallEase)
            .OnComplete(() => 
            {
                // Pequeño rebote adicional cuando llega al centro
                StartCoroutine(ExtraBounceEffect());
            });
        
        yield return new WaitForSeconds(fallDuration + 0.3f); // Esperar a que termine la animación
        isAnimating = false;
    }
    
    private IEnumerator ExtraBounceEffect()
    {
        // Pequeño rebote hacia arriba y luego regreso
        Vector2 bouncePos = targetPosition + Vector2.up * bounceHeight;
        
        panelRect.DOAnchorPos(bouncePos, 0.15f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => 
            {
                panelRect.DOAnchorPos(targetPosition, 0.15f)
                    .SetEase(Ease.InQuad);
            });
        
        yield return new WaitForSeconds(0.3f);
    }
    
    private IEnumerator HidePanelCoroutine()
    {
        isAnimating = true;
        
        // Animación de salida hacia arriba
        Vector2 exitPosition = new Vector2(targetPosition.x, Screen.height + panelRect.rect.height);
        
        // Fade out
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, 0.3f);
        }
        
        // Mover hacia arriba
        panelRect.DOAnchorPos(exitPosition, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() => 
            {
                gameObject.SetActive(false);
                isAnimating = false;
            });
        
        yield return new WaitForSeconds(0.5f);
    }
    
    // Método para resetear la posición sin animación
    public void ResetPosition()
    {
        panelRect.DOKill();
        if (canvasGroup != null)
            canvasGroup.DOKill();
            
        panelRect.anchoredPosition = startPosition;
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
        isAnimating = false;
    }
}