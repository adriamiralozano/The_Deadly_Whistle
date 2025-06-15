using UnityEngine;
using UnityEngine.UI;

public class ContractButton : MonoBehaviour
{
    public ContractSO contractData;
    public ContractVisualManager manager;

    private void Start()
    {
        // Asigna la imagen del botón al sprite del contrato
        GetComponent<Image>().sprite = contractData.PreviewSprite;
        // Asigna el evento al botón
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    public void OnButtonClick()
    {
        manager.ShowContractPreview(contractData);
    }
}