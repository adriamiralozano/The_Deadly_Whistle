using UnityEngine;
using UnityEngine.SceneManagement;

public class AcceptContractHandler : MonoBehaviour
{
    public void OnButtonClick()
    {
        SceneManager.LoadScene("Combat_Test_Scene");
    }
}