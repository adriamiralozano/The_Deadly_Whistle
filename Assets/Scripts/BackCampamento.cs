using UnityEngine;
using UnityEngine.SceneManagement;

public class BackCampamento : MonoBehaviour
{
    [SerializeField] private string campamentoSceneName = "Campamento"; // Cambia el nombre si tu escena se llama diferente

    public void GoToCampamento()
    {
        SceneManager.LoadScene(campamentoSceneName);
    }
}