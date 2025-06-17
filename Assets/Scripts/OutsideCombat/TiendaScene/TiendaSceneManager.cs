using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TiendaSceneManager : MonoBehaviour
{
    public void OnSleepButton()
    {
        if (ActManager.Instance != null && ActManager.Instance.CurrentPhase == ActPhase.PostCombat)
        {
            ActManager.Instance.AdvanceAct(); // Avanza al siguiente acto y pone la fase en PreCombat

            if (SaveManager.Instance != null)
                SaveManager.Instance.SaveCurrentGame();

            SceneManager.LoadScene("Campamento");
        }
        else
        {
            Debug.Log("No puedes dormir aún, no has terminado la fase de combate.");
        }
    }
    public void OnMoneyElection()
    {
            if (SaveManager.Instance != null)
                SaveManager.Instance.SaveCurrentGame();

            SceneManager.LoadScene("MoneyElection");

    }
}
