using UnityEngine;
using UnityEngine.SceneManagement; // Opcional, para botones como "Volver al Menú"
using UnityEngine.Rendering.PostProcessing;
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private PostProcessVolume postProcessVolume;

    private bool isPaused = false;
    public bool IsPaused => isPaused;

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
            // Opcional: si quieres que el manager persista entre escenas
            // DontDestroyOnLoad(gameObject); 
        }
    }

    private void Start()
    {
        // Asegúrate de que el menú de pausa esté oculto al empezar
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        if (postProcessVolume != null)
        {
            postProcessVolume.enabled = false;
        }
    }

    void Update()
    {
        // Escucha la tecla 'Escape' para pausar o reanudar
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }


    public void Resume()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        if (postProcessVolume != null)
        {
            postProcessVolume.enabled = false;
        }
        Time.timeScale = 1f; // Restaura el flujo del tiempo
        isPaused = false;
        Debug.Log("Game Resumed.");
    }


    void Pause()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }
        if (postProcessVolume != null)
        {
            postProcessVolume.enabled = true;
        }
        Time.timeScale = 0f; // Congela el tiempo del juego
        isPaused = true;
        Debug.Log("Game Paused.");
    }

    // --- Métodos Opcionales para Botones del Menú ---


    public void LoadMenu()
    {
        // ¡Importante! Asegúrate de reanudar el tiempo antes de cambiar de escena.
        Time.timeScale = 1f;
        // Cambia "MainMenuScene" por el nombre real de tu escena de menú
        SceneManager.LoadScene("MainMenuScene");
    }


    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
    
    public void LoadTablonScene()
    {
        // Restaura el tiempo antes de cambiar de escena para evitar problemas.
        Time.timeScale = 1f;
        SceneManager.LoadScene("Tablon");
        Debug.Log("Loading Tablon scene...");
    }
}