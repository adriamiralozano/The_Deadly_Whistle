using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AspectRatioEnforcer : MonoBehaviour
{
    // Relación de aspecto deseada (16:9)
    public float targetAspect = 16f / 9f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdateViewport();
    }

    void Update()
    {
        // Opcional: solo si quieres que se adapte en caliente al cambiar la resolución
        UpdateViewport();
    }

    void UpdateViewport()
    {
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            // Letterbox: barras negras arriba y abajo
            Rect rect = cam.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            cam.rect = rect;
        }
        else
        {
            // Pillarbox: barras negras a los lados
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = cam.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            cam.rect = rect;
        }
    }
}