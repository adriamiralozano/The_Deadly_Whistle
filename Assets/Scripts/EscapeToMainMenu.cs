using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeToMainMenu : MonoBehaviour
{
    [Header("Nombre de la escena del menú principal")]
    public string mainMenuSceneName = "MainMenu"; // Cambia si tu escena se llama diferente

    void Update()
    {


        if (Input.GetKeyDown(KeyCode.Escape) && SceneManager.GetActiveScene().name != "Combat_Test_Scene")
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveCurrentGame();
                Debug.Log("Partida guardada antes de volver al menú principal.");
            }
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    // Este método debe recopilar el estado actual del juego

}