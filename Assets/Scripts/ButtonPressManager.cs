using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonPressManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Button button;
    public RectTransform target;

    private Vector2 originalPos;

    void Start()
    {
        if (button == null)
            button = GetComponent<Button>();
        if (target == null)
            target = GetComponent<RectTransform>();

        originalPos = target.anchoredPosition;
        button.onClick.AddListener(ShakeButton);
    }

    void ShakeButton()
    {
        target.DOShakeAnchorPos(0.1f, 10f, 20, 0, false, false)
            .OnComplete(() => target.anchoredPosition = originalPos);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonPress();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonRelease();
    }
}