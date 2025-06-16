using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    [Header("Referencias de Menús")]
    public GameObject menu1;
    public GameObject menuHistoria;
    public GameObject ConfirmationExitDesktop;
    public GameObject CanvasBloqueador;

    [Header("Botones de partida")]
    public Button continueButton;
    public Button newGameButton;

    [Header("Nombre de la escena inicial")]
    public string campamentoSceneName = "Campamento";

    // Llama a este método desde el botón de historia (OnClick)


    private void Start()
    {
        // Comprobar si hay guardado
        bool hasSave = File.Exists(Application.persistentDataPath + "/save.json");
        if (continueButton != null)
            continueButton.interactable = hasSave;
        if (newGameButton != null && !hasSave)
            newGameButton.Select();
    }

    public void OnContinueButton()
    {
        bool hasSave = File.Exists(Application.persistentDataPath + "/save.json");
        if (hasSave)
        {
            SceneManager.LoadScene(campamentoSceneName);
        }
        else
        {
            Debug.LogWarning("No hay partida guardada para continuar.");
        }
    }

    public void OnNewGameButton()
    {
        Debug.Log("OnNewGameButton PRESSED");

        if (SaveManager.Instance != null)
            SaveManager.Instance.NewGame();

        // Reinicia el estado de ActManager en memoria si existe
        if (ActManager.Instance != null)
            ActManager.Instance.LoadFromSave(SaveManager.Instance.LoadGame());

        SceneManager.LoadScene(campamentoSceneName);
    }

    public void OnStoryButtonClick()
    {
        if (menu1 != null)
            menu1.SetActive(false);
        if (menuHistoria != null)
            menuHistoria.SetActive(true);
    }

    public void ExitStoryMenuButton()
    {
        if (menuHistoria != null)
            menuHistoria.SetActive(false);
        if (menu1 != null)
            menu1.SetActive(true);

    }


    // ------- Métodos para salir del escritorio -------
    public void ExitDesktopButton()
    {
        Debug.Log("ExitDesktopButton called");
        if (ConfirmationExitDesktop != null)
            Debug.Log("ConfirmationExitDesktop is not null");
        ConfirmationExitDesktop.SetActive(true);
        if (CanvasBloqueador != null)
            CanvasBloqueador.SetActive(true);
    }
    public void ExitDesktopButtonConfirmation()
    {
        Debug.Log("ExitDesktopButtonConfirmation called");
        Application.Quit();
    }
    public void ExitDesktopButtonCancel()
    {
        Debug.Log("ExitDesktopButtonCancel called");
        if (ConfirmationExitDesktop != null)
            ConfirmationExitDesktop.SetActive(false);
        if (CanvasBloqueador != null)
            CanvasBloqueador.SetActive(false);
    }
    // ------------------------------------------------
    
    public void CleanupPersistentManagers()
    {
        // Destruye el ActManager si existe
        if (ActManager.Instance != null)
            Destroy(ActManager.Instance.gameObject);

    }
}