using UnityEngine;
using UnityEngine.SceneManagement;

public class SpriteHoverScaler : MonoBehaviour
{
    [SerializeField] private float scaleMultiplier = 1.15f;
    [SerializeField] private float scaleSpeed = 0.15f;
    [Header("Cambio de escena")]
    [SerializeField] private string sceneToLoad; 

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Awake()
    {
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void OnMouseEnter()
    {
        Debug.Log($"{gameObject.name} - OnMouseEnter");
        StartScaling(originalScale * scaleMultiplier);
        SetHighlight(true);
    }

    void OnMouseExit()
    {
        Debug.Log($"{gameObject.name} - OnMouseExit");
        StartScaling(originalScale);
        SetHighlight(false);
    }

    void OnMouseDown()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"Cargando escena: {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void SetHighlight(bool highlight)
    {
        if (spriteRenderer == null) return;
        if (highlight)
            spriteRenderer.color = Color.white;
        else
            spriteRenderer.color = originalColor;
    }

    private void StartScaling(Vector3 targetScale)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(targetScale));
    }

    private System.Collections.IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / scaleSpeed;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}