using UnityEngine;

public class ContractManager : MonoBehaviour
{
    public static ContractManager Instance { get; private set; }
    public static ContractSO PendingAcceptedContract = null;
    
    private ContractSO acceptedContract;
    private SaveData preContractSave; // Guardado antes de elegir contrato

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Si hay un contrato pendiente, lo asignamos al entrar en la escena de combate
        if (PendingAcceptedContract != null)
        {
            acceptedContract = PendingAcceptedContract;
            Debug.Log($"[ContractManager] Contrato transferido a escena de combate: {acceptedContract.Title}");
            PendingAcceptedContract = null; // Limpiamos la variable estática
        }
    }
    public void SavePreContractState()
    {
        // Guarda el estado actual antes de elegir contrato
        preContractSave = new SaveData();
        preContractSave.currentAct = (int)ActManager.Instance.CurrentAct;
        preContractSave.currentPhase = (int)ActManager.Instance.CurrentPhase;
        preContractSave.familyMoney = GameStats.Instance.familyMoney;
        preContractSave.gangMoney = GameStats.Instance.gangMoney;
        preContractSave.playerMoney = GameStats.Instance.playerMoney;
        //preContractSave.completedContracts = new System.Collections.Generic.List<string>(GameStats.Instance.completedContracts);
        
        Debug.Log("Estado pre-contrato guardado");
    }

    public void RestorePreContractState()
    {
        if (preContractSave != null)
        {
            ActManager.Instance.LoadFromSave(preContractSave);
            GameStats.Instance.LoadFromSave(preContractSave);
            SaveManager.Instance.SaveGame(preContractSave);
            Debug.Log("Estado pre-contrato restaurado");
        }
    }

    public void SetAcceptedContract(ContractSO contract)
    {
        acceptedContract = contract;
        Debug.Log($"Contrato aceptado: {contract.Title}");
    }

    public void OnContractWon()
    {
        if (acceptedContract != null)
        {
            Debug.Log($"¡Contrato ganado! Recompensa: {acceptedContract.Price}");
            
            // Añade dinero al jugador
            GameStats.Instance.playerMoney += acceptedContract.Price;
            
            // Añade contrato a lista de completados
            GameStats.Instance.AddCompletedContract(acceptedContract.Title);
            
            Debug.Log($"Dinero del jugador actualizado: {GameStats.Instance.playerMoney}");
            
            // Limpia el contrato
            acceptedContract = null;
            preContractSave = null;
        }
    }

    public void OnContractLost()
    {
        if (acceptedContract != null)
        {
            Debug.Log($"Contrato perdido: {acceptedContract.Title}");
            
            // Restaura el estado pre-contrato
            RestorePreContractState();
            
            // Limpia el contrato
            acceptedContract = null;
        }
    }

    public ContractSO GetAcceptedContract()
    {
        return acceptedContract;
    }
}