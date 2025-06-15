using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using TMPro;

public class ContractVisualManager : MonoBehaviour
{
    [Header("Preview UI")]
    public GameObject contractPreview;
    public Image previewImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI priceText;

    [Header("Post Process")]
    public Camera mainCamera; // Arrastra aquí tu Main Camera

    [Header("Botón de salida")]
    public PolygonCollider2D backButtonCollider; 

    private PostProcessVolume postProcessVolume;

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
            postProcessVolume = mainCamera.GetComponent<PostProcessVolume>();
    }

    public void ShowContractPreview(ContractSO contract)
    {
        contractPreview.SetActive(true);
        previewImage.sprite = contract.PreviewSprite;
        titleText.text = contract.Title;
        descriptionText.text = contract.Description;
        priceText.text = contract.Price.ToString() + " $";

        // Activa el post process
        if (postProcessVolume != null)
            postProcessVolume.enabled = true;

        if (backButtonCollider != null)
            backButtonCollider.enabled = false;
    }

    public void HideContractPreview()
    {
        contractPreview.SetActive(false);

        // Desactiva el post process
        if (postProcessVolume != null)
            postProcessVolume.enabled = false;

        if (backButtonCollider != null)
            backButtonCollider.enabled = true;
    }
}