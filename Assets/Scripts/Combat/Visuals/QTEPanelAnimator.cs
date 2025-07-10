using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class QTEPanelAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float fallDuration = 0.8f;

    [SerializeField] private Ease fallEase = Ease.OutBounce;
    
    [Header("Panel Settings")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject blockerOverlay;
    
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
            
        SetupInitialPosition();
    }
    
    private void SetupInitialPosition()
    {
        targetPosition = panelRect.anchoredPosition;
        
        startPosition = new Vector2(targetPosition.x, Screen.height + panelRect.rect.height);
        
        panelRect.anchoredPosition = startPosition;
        
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        
        gameObject.SetActive(false);
    }
    
    public void ShowPanel()
    {
        if (isAnimating) return;

        if (blockerOverlay != null)
            blockerOverlay.SetActive(true);

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
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.2f);
        }

        panelRect.DOAnchorPos(targetPosition, fallDuration)
            .SetEase(fallEase);
            
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayQTEPanelIn();
        }
        yield return new WaitForSeconds(fallDuration + 0.3f);
        isAnimating = false;
    }


    private IEnumerator HidePanelCoroutine()
    {
        isAnimating = true;
        Vector2 exitPosition = new Vector2(targetPosition.x, Screen.height + panelRect.rect.height);

        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, 0.3f);
        }

        panelRect.DOAnchorPos(exitPosition, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                isAnimating = false;
            });
        if (blockerOverlay != null)
            blockerOverlay.SetActive(false);
        yield return new WaitForSeconds(0.5f);
    }
    
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