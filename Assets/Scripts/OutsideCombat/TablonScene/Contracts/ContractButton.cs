using UnityEngine;
using UnityEngine.UI;

public class ContractButton : MonoBehaviour
{
    public ContractSO contractData;
    public ContractVisualManager manager;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
        
        // Solo actualiza visuales si hay contractData válido
        if (contractData != null)
        {
            UpdateButtonVisuals();
        }
        // NO cambiar visibilidad aquí - lo hace ContractVisualManager
    }

    public void SetContract(ContractSO contractSO)
    {
        Debug.Log($"🎮 ContractButton.SetContract llamado en {gameObject.name}");
        Debug.Log($"   Contrato anterior: {(contractData != null ? contractData.Title : "NULL")}");
        Debug.Log($"   Contrato nuevo: {(contractSO != null ? contractSO.Title : "NULL")}");
        
        contractData = contractSO;
        
        if (contractData != null)
        {
            Debug.Log($"   ✅ Contrato asignado a {gameObject.name}: {contractData.Title}");
            UpdateButtonVisuals();
            // NO cambiar visibilidad aquí - UpdateContractButtonsState() se encarga
        }
        else
        {
            Debug.Log($"   ❌ Contrato NULL asignado a {gameObject.name}");
            // NO cambiar visibilidad aquí
        }
    }

    private void UpdateButtonVisuals()
    {
        Debug.Log($"🎨 Actualizando visuales de {gameObject.name}");
        if (contractData != null && contractData.PreviewSprite != null)
        {
            GetComponent<Image>().sprite = contractData.PreviewSprite;
            Debug.Log($"   ✅ Sprite asignado: {contractData.PreviewSprite.name}");
        }
        else
        {
            Debug.LogWarning($"   ⚠️ No se pudo asignar sprite - contractData: {contractData != null}, PreviewSprite: {contractData?.PreviewSprite != null}");
        }
    }

    public void OnButtonClick()
    {
        if (contractData != null)
        {
            manager.ShowContractPreview(contractData);
        }
    }

    // Método público para verificar si tiene contrato asignado
    public bool HasContract()
    {
        return contractData != null;
    }
}