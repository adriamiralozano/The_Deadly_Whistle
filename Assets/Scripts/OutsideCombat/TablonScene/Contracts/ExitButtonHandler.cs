using UnityEngine;

public class ExitButtonHandler : MonoBehaviour
{
    public ContractVisualManager manager;

    public void OnButtonClick()
    {
        if (manager != null)
            manager.HideContractPreview();
    }
}