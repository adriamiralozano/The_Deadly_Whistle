using UnityEngine;

public class MenuBootstrapper : MonoBehaviour
{
    void Start()
    {
        GameObject globalManager = GameObject.Find("GlobalGameManager");
        if (globalManager != null)
        {
            Destroy(globalManager);
            Debug.Log("GlobalGameManager destruido al volver al menú principal.");
        }
    }
}