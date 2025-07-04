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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
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
        Time.timeScale = 1f; 
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
        Time.timeScale = 0f;
        isPaused = true;
        Debug.Log("Game Paused.");
    }



    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }


    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public void LoadTablonScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Tablon");
        Debug.Log("Loading Tablon scene...");
    }

    public void ReturnToMainMenu()
    {

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveCurrentGame();
            Debug.Log("Partida guardada antes de volver al menú principal.");
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Returning to Main Menu...");
        
        if (postProcessVolume != null)
        {
            postProcessVolume.enabled = false;
        }
    }
}