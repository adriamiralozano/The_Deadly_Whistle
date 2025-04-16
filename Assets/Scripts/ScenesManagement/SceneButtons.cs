using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Campamento");
    }

    public void BackCamp()
    {
        SceneManager.LoadScene("Campamento");
    }

    public void GoQuestTable()
    {
        SceneManager.LoadScene("Tablon");
    }

    public void GoBonfire()
    {
        SceneManager.LoadScene("Hoguera");
    }

    public void GoTent()
    {
        SceneManager.LoadScene("Tienda");
    }

}
