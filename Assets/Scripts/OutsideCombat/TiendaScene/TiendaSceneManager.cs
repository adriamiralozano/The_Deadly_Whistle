using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TiendaSceneManager : MonoBehaviour
{
    public void OnMoneyElection()
    {
        if (ActManager.Instance != null && ActManager.Instance.CurrentPhase == ActPhase.PostCombat)
        {

            if (SaveManager.Instance != null)
                SaveManager.Instance.SaveCurrentGame();

            SceneManager.LoadScene("MoneyElection");
        }
        else
        {
            Debug.Log("No puedes dormir aún, no has terminado la fase de combate.");
        }
    }
}
