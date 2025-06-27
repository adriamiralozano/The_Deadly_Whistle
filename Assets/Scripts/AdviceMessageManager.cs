using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

public class AdviceMessageManager : MonoBehaviour
{
    public static AdviceMessageManager Instance { get; private set; }

    [Header("UI Settings")]
    [SerializeField] private GameObject advicePrefab; // Arrastra aquí tu prefab del mensaje
    [SerializeField] private Transform parentCanvas;  // Arrastra aquí el Canvas principal de tu UI

    [Header("Animation Timings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float displayDuration = 2.0f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private void Awake()
    {
        // Configuración del Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void ShowAdvice(string message)
    {
        if (advicePrefab == null)
        {
            Debug.LogError("El prefab del consejo no está asignado en AdviceMessageManager.");
            return;
        }

        // Instancia el prefab como hijo del canvas
        GameObject adviceInstance = Instantiate(advicePrefab, parentCanvas);

        // Busca los componentes necesarios en el prefab instanciado
        TextMeshProUGUI textComponent = adviceInstance.GetComponentInChildren<TextMeshProUGUI>();
        CanvasGroup canvasGroup = adviceInstance.GetComponent<CanvasGroup>();

        if (textComponent == null)
        {
            Debug.LogError("No se encontró un componente TextMeshProUGUI en el prefab del consejo.");
            Destroy(adviceInstance);
            return;
        }

        if (canvasGroup == null)
        {
            canvasGroup = adviceInstance.AddComponent<CanvasGroup>();
        }

        // Configura el texto y la opacidad inicial
        textComponent.text = message;
        canvasGroup.alpha = 0f;

        // Crea y ejecuta la secuencia de animación con DOTween
        Sequence sequence = DOTween.Sequence();
        sequence.Append(canvasGroup.DOFade(1f, fadeInDuration))  // Fade In
                .AppendInterval(displayDuration)                 // Espera
                .Append(canvasGroup.DOFade(0f, fadeOutDuration)) // Fade Out
                .OnComplete(() => {
                    Destroy(adviceInstance); // Destruye el objeto al finalizar
                });
    }
}