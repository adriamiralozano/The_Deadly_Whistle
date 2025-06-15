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

        // Si no está asignado en el inspector, lo busca en la MainCamera
        if (blurVolume == null && Camera.main != null)
            blurVolume = Camera.main.GetComponent<PostProcessVolume>();

        previewCanvas.SetActive(false);
        if (blurVolume != null)
            blurVolume.enabled = false;
    }

    public void ShowCard(Sprite cardSprite)
    {
        cardImage.sprite = cardSprite;
        AdjustPreviewScale(); // <-- aquí
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
        // Porcentaje de la altura de pantalla que quieres que ocupe la carta (por ejemplo, 40%)
        float targetHeightPercent = 0.6f;

        // Obtén el RectTransform de la carta
        RectTransform rt = cardImage.rectTransform;

        // Calcula la altura deseada en píxeles
        float targetHeight = Screen.height * targetHeightPercent;

        // Ajusta el tamaño manteniendo la proporción del sprite
        float aspect = rt.sizeDelta.x / rt.sizeDelta.y;
        rt.sizeDelta = new Vector2(targetHeight * aspect, targetHeight);
    }
}