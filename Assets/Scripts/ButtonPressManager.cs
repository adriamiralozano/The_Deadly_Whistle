using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonPressManager : MonoBehaviour
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
        target.DOShakeAnchorPos(0.1f, 3f, 20, 0, false, false)
            .OnComplete(() => target.anchoredPosition = originalPos);
    }
}