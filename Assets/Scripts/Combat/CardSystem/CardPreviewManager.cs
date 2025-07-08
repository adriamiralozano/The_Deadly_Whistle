using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class CardPreviewManager : MonoBehaviour
{
    public static CardPreviewManager Instance { get; private set; }

    [Header("Referencias UI")]
    public GameObject previewCanvas;
    public Image cardImage;

    [Header("PostProcess")]
    public PostProcessVolume blurVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        if (blurVolume == null && Camera.main != null)
            blurVolume = Camera.main.GetComponent<PostProcessVolume>();

        previewCanvas.SetActive(false);
        if (blurVolume != null)
            blurVolume.enabled = false;
    }

    public void ShowCard(Sprite cardSprite)
    {
        cardImage.sprite = cardSprite;
        AdjustPreviewScale();
        previewCanvas.SetActive(true);
        if (blurVolume != null)
            blurVolume.enabled = true;
    }

    public void HidePreview()
    {
        previewCanvas.SetActive(false);
        if (blurVolume != null)
            blurVolume.enabled = false;
    }

    private void Update()
    {
        if (previewCanvas.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                cardImage.rectTransform,
                Input.mousePosition,
                null))
            {
                HidePreview();
            }
        }
    }

    void AdjustPreviewScale()
    {
        float targetHeightPercent = 0.6f;
        RectTransform rt = cardImage.rectTransform;

        float targetHeight = Screen.height * targetHeightPercent;

        float aspect = rt.sizeDelta.x / rt.sizeDelta.y;
        rt.sizeDelta = new Vector2(targetHeight * aspect, targetHeight);
    }
}