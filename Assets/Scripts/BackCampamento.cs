using UnityEngine;
using UnityEngine.SceneManagement;

public class BackCampamento : MonoBehaviour
{
    [SerializeField] private string campamentoSceneName = "Campamento"; 

    public void GoToCampamento()
    {
        SceneManager.LoadScene(campamentoSceneName);
    }
}