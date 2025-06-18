using UnityEngine;
using UnityEngine.SceneManagement;

public class AcceptContractHandler : MonoBehaviour
{
    public ContractVisualManager visualManager;

    public void OnButtonClick()
    {
        Debug.Log("AcceptContractHandler.OnButtonClick iniciado");
        
        // Validación 1: VisualManager
        if (visualManager == null)
        {
            Debug.LogError("VisualManager es null!");
            return;
        }

        // Validación 2: Contrato actual
        ContractSO currentContract = GetCurrentPreviewContract();
        if (currentContract == null)
        {
            Debug.LogError("No hay contrato seleccionado actualmente!");
            return;
        }

        Debug.Log($"Contrato obtenido: {currentContract.Title}");

        // Validación 3: ContractManager
        if (ContractManager.Instance == null)
        {
            Debug.LogError("ContractManager.Instance es null!");
            return;
        }

        // Validación 4: ActManager
        if (ActManager.Instance == null)
        {
            Debug.LogError("ActManager.Instance es null!");
            return;
        }

        // Validación 5: GameStats
        if (GameStats.Instance == null)
        {
            Debug.LogError("GameStats.Instance es null!");
            return;
        }

        // Si llegamos aquí, todo está bien
        Debug.Log("Todas las validaciones pasadas, procediendo con el contrato...");

        // Guarda el estado antes de elegir contrato
        ContractManager.Instance.SavePreContractState();
        ContractManager.PendingAcceptedContract = currentContract;
        

        SceneManager.LoadScene("Combat_Test_Scene");
    }

    private ContractSO GetCurrentPreviewContract()
    {
        if (visualManager == null)
        {
            Debug.LogError("VisualManager es null en GetCurrentPreviewContract!");
            return null;
        }

        ContractSO contract = visualManager.GetCurrentPreviewContract();
        if (contract == null)
        {
            Debug.LogWarning("GetCurrentPreviewContract devolvió null. ¿Has seleccionado un contrato?");
        }

        return contract;
    }
}