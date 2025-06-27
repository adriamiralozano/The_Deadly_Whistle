using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFinale : MonoBehaviour
{
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}