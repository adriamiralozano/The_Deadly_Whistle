using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class ContractVisualManager : MonoBehaviour
{
    [Header("Preview UI")]
    public GameObject contractPreview;
    public Image previewImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI priceText;
    public GameObject[] contractButtons;

    [Header("Post Process")]
    public Camera mainCamera;

    [Header("Botón de salida")]
    public PolygonCollider2D backButtonCollider;

    [Header("Contracts Management")]
    public ActContractsManager actContractsManager;

    private PostProcessVolume postProcessVolume;
    private ContractSO currentPreviewContract;
    private int lastActLoaded = -1;


    void Update()
    {
        if (SceneManager.GetActiveScene().name != "Tablon")
            return;

        if (ActManager.Instance != null)
        {
            int currentAct = (int)ActManager.Instance.CurrentAct;
            if (currentAct != lastActLoaded)
            {
                LoadContractsForCurrentAct();
                lastActLoaded = currentAct;
            }
        }
    }
    void Start()
    {

        StartCoroutine(LoadContractsWhenReady());
    }

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
            postProcessVolume = mainCamera.GetComponent<PostProcessVolume>();

        if (contractButtons != null)
        {
            foreach (var btn in contractButtons)
            {
                if (btn != null)
                    btn.SetActive(false);
            }
        }
    }

    private IEnumerator LoadContractsWhenReady()
    {
        int waitFrames = 0;
        while (ActManager.Instance == null)
        {
            waitFrames++;
            yield return null;
        }
        yield return new WaitForEndOfFrame();
        LoadContractsForCurrentAct();
    }

    public ContractSO GetCurrentPreviewContract()
    {
        return currentPreviewContract;
    }

    public void ShowContractPreview(ContractSO contract)
    {
        if (contract == null)
        {
            return;
        }

        currentPreviewContract = contract;
        contractPreview.SetActive(true);
        previewImage.sprite = contract.PreviewSprite;
        titleText.text = contract.Title;
        descriptionText.text = contract.Description;
        priceText.text = contract.Price.ToString() + " $";

        if (postProcessVolume != null)
            postProcessVolume.enabled = true;

        if (backButtonCollider != null)
            backButtonCollider.enabled = false;
    }

    public void HideContractPreview()
    {
        contractPreview.SetActive(false);

        if (postProcessVolume != null)
            postProcessVolume.enabled = false;

        if (backButtonCollider != null)
            backButtonCollider.enabled = true;
    }

    public void UpdateContractButtonsState()
    {
        
        bool actManagerReady = ActManager.Instance != null;
        ActPhase currentPhase = actManagerReady ? ActManager.Instance.CurrentPhase : ActPhase.PreCombat;
        GameAct currentAct = actManagerReady ? ActManager.Instance.CurrentAct : GameAct.Tutorial;
        bool preCombat = actManagerReady && currentPhase == ActPhase.PreCombat;
        bool notTutorial = currentAct != GameAct.Tutorial;
        bool shouldShowButtons = preCombat && notTutorial;


        for (int i = 0; i < contractButtons.Length; i++)
        {
            if (contractButtons[i] != null)
            {
                if (!notTutorial)
                {
                    contractButtons[i].SetActive(false);
                    continue;
                }

                var contractButton = contractButtons[i].GetComponent<ContractButton>();
                bool hasContract = contractButton != null && contractButton.HasContract();
                bool shouldShow = shouldShowButtons && hasContract;

                bool wasActive = contractButtons[i].activeInHierarchy;
                contractButtons[i].SetActive(shouldShow);
            }
        }
    }

    private void RefreshContractButtonsArray()
    {
        var foundButtons = FindObjectsOfType<ContractButton>(includeInactive: true);
        contractButtons = new GameObject[foundButtons.Length];
        for (int i = 0; i < foundButtons.Length; i++)
        {
            contractButtons[i] = foundButtons[i].gameObject;
        }
    }
    private void LoadContractsForCurrentAct()
    {
        RefreshContractButtonsArray();

        if (ActManager.Instance == null)
        {
            return;
        }

        if (actContractsManager == null)
        {
            return;
        }

        int currentAct = (int)ActManager.Instance.CurrentAct;

        for (int i = 0; i < actContractsManager.contractsByAct.Count; i++)
        {
            var element = actContractsManager.contractsByAct[i];
        }
        ClearAllContracts();

        if (currentAct == 0)
        {
            UpdateContractButtonsState();
            return;
        }

        var contracts = actContractsManager.GetContractsForAct(currentAct);

        if (contracts == null)
        {
            UpdateContractButtonsState();
            return;
        }

        for (int i = 0; i < contractButtons.Length; i++)
        {
            if (contractButtons[i] != null)
            {
                var contractButton = contractButtons[i].GetComponent<ContractButton>();
            }
        }


        if (contractButtons.Length > 0 && contractButtons[0] != null && contracts.legalContract != null)
        {
            var contractButton = contractButtons[0].GetComponent<ContractButton>();
            if (contractButton != null)
            {
                contractButton.SetContract(contracts.legalContract);
            }
        }


        if (contractButtons.Length > 1 && contractButtons[1] != null && contracts.illegalContract != null)
        {
            var contractButton = contractButtons[1].GetComponent<ContractButton>();
            if (contractButton != null)
            {
                contractButton.SetContract(contracts.illegalContract);
            }
        }
        UpdateContractButtonsState();
        lastActLoaded = (int)ActManager.Instance.CurrentAct;
    }

    private void ClearAllContracts()
    {
        for (int i = 0; i < contractButtons.Length; i++)
        {
            if (contractButtons[i] != null)
            {
                var contractButton = contractButtons[i].GetComponent<ContractButton>();
                if (contractButton != null)
                {
                    contractButton.SetContract(null);
                }
            }
        }
    }
}