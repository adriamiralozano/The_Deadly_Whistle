using UnityEngine;
using UnityEngine.UI;

public class ContractButton : MonoBehaviour
{
    public ContractSO contractData;
    public ContractVisualManager manager;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
        
        if (contractData != null)
        {
            UpdateButtonVisuals();
        }
    }

    public void SetContract(ContractSO contractSO)
    {        
        contractData = contractSO;
        
        if (contractData != null)
        {
            UpdateButtonVisuals();
        }
    }

    private void UpdateButtonVisuals()
    {
        if (contractData != null && contractData.PreviewSprite != null)
        {
            GetComponent<Image>().sprite = contractData.PreviewSprite;
        }
    }

    public void OnButtonClick()
    {
        if (contractData != null)
        {
            manager.ShowContractPreview(contractData);
        }
    }

    public bool HasContract()
    {
        return contractData != null;
    }
}