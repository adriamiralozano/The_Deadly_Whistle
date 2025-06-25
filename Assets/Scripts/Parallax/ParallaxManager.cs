using UnityEngine;

public class ParallaxManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float edgeThreshold = 0.1f;

    [Header("Límites de movimiento")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;

    [Header("Capas de Parallax (de fondo a frente)")]
    [SerializeField] private Transform[] parallaxLayers = new Transform[5];
    [SerializeField] private float[] parallaxFactors = new float[5] { 0.2f, 0.4f, 0.6f, 0.8f, 1f };

    private Camera mainCamera;
    private Vector3 lastCameraPos;

    void Start()
    {
        mainCamera = Camera.main;
        lastCameraPos = mainCamera.transform.position;
    }

    void Update()
    {
        float mouseX = Input.mousePosition.x;
        float screenWidth = Screen.width;

        float leftEdge = screenWidth * edgeThreshold;
        float rightEdge = screenWidth * (1f - edgeThreshold);

        float moveDirection = 0f;

        if (mouseX < leftEdge)
            moveDirection = -1f;
        else if (mouseX > rightEdge)
            moveDirection = 1f;

        if (moveDirection != 0f)
        {
            Vector3 camPos = mainCamera.transform.position;
            camPos.x += moveDirection * moveSpeed * Time.deltaTime;
            camPos.x = Mathf.Clamp(camPos.x, minX, maxX);
            mainCamera.transform.position = camPos;

            // Calcula el desplazamiento de la cámara
            float deltaX = camPos.x - lastCameraPos.x;

            // Mueve cada capa según su factor de parallax
            for (int i = 0; i < parallaxLayers.Length; i++)
            {
                if (parallaxLayers[i] != null)
                {
                    Vector3 layerPos = parallaxLayers[i].position;
                    layerPos.x += deltaX * parallaxFactors[i];
                    parallaxLayers[i].position = layerPos;
                }
            }

            lastCameraPos = camPos;
        }
    }
}