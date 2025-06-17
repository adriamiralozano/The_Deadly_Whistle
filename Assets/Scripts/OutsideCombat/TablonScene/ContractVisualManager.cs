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
        // Solo refrescar si estamos en la escena Tablón
        if (SceneManager.GetActiveScene().name != "Tablon")
            return;

        if (ActManager.Instance != null)
        {
            int currentAct = (int)ActManager.Instance.CurrentAct;
            if (currentAct != lastActLoaded)
            {
                Debug.Log($"Cambio de acto detectado: {lastActLoaded} → {currentAct}. Recargando contratos.");
                LoadContractsForCurrentAct();
                lastActLoaded = currentAct;
            }
        }
    }
    void Start()
    {
        Debug.Log("🎯 ContractVisualManager.Start() - Cargando contratos para el acto actual");
        
        // Simplemente carga los contratos del acto actual
        // Sin eventos, sin complicaciones
        StartCoroutine(LoadContractsWhenReady());
    }

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
            postProcessVolume = mainCamera.GetComponent<PostProcessVolume>();

        // OCULTAR TODOS LOS BOTONES AL INICIAR
        if (contractButtons != null)
        {
            foreach (var btn in contractButtons)
            {
                if (btn != null)
                    btn.SetActive(false);
            }
        }
    }

    // QUITAR TODOS LOS MÉTODOS DE EVENTOS - No los necesitamos
    // private void OnEnable() { ... }
    // private void OnDisable() { ... }
    // private void OnActChanged() { ... }

    private IEnumerator LoadContractsWhenReady()
    {
        Debug.Log("=== INICIANDO LoadContractsWhenReady ===");

        // Espera hasta que ActManager esté listo
        int waitFrames = 0;
        while (ActManager.Instance == null)
        {
            waitFrames++;
            Debug.Log($"Esperando ActManager... Frame {waitFrames}");
            yield return null;
        }

        Debug.Log($"ActManager encontrado después de {waitFrames} frames");

        // Espera un frame adicional para asegurar que ActManager haya cargado sus datos
        yield return new WaitForEndOfFrame();

        Debug.Log($"=== ESTADO ACTUAL ===");
        Debug.Log($"ActManager.CurrentAct: {ActManager.Instance.CurrentAct} (int: {(int)ActManager.Instance.CurrentAct})");
        Debug.Log($"ActManager.CurrentPhase: {ActManager.Instance.CurrentPhase} (int: {(int)ActManager.Instance.CurrentPhase})");
        Debug.Log($"actContractsManager asignado: {actContractsManager != null}");
        Debug.Log($"contractButtons array length: {contractButtons?.Length ?? 0}");

        LoadContractsForCurrentAct();

        Debug.Log("=== FINALIZANDO LoadContractsWhenReady ===");
    }

    public ContractSO GetCurrentPreviewContract()
    {
        if (currentPreviewContract == null)
        {
            Debug.LogWarning("currentPreviewContract es null!");
        }
        else
        {
            Debug.Log($"Devolviendo contrato: {currentPreviewContract.Title}");
        }
        return currentPreviewContract;
    }

    public void ShowContractPreview(ContractSO contract)
    {
        if (contract == null)
        {
            Debug.LogError("Se intentó mostrar un contrato null!");
            return;
        }

        currentPreviewContract = contract;
        Debug.Log($"Contrato asignado a currentPreviewContract: {contract.Title}");

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
        Debug.Log("=== ACTUALIZANDO VISIBILIDAD DE BOTONES ===");
        
        bool actManagerReady = ActManager.Instance != null;
        ActPhase currentPhase = actManagerReady ? ActManager.Instance.CurrentPhase : ActPhase.PreCombat;
        GameAct currentAct = actManagerReady ? ActManager.Instance.CurrentAct : GameAct.Tutorial;
        bool preCombat = actManagerReady && currentPhase == ActPhase.PreCombat;
        bool notTutorial = currentAct != GameAct.Tutorial;
        bool shouldShowButtons = preCombat && notTutorial;

        Debug.Log($"📊 Estado actual:");
        Debug.Log($"   ActManager ready: {actManagerReady}");
        Debug.Log($"   Current Act: {currentAct}");
        Debug.Log($"   Current Phase: {currentPhase}");
        Debug.Log($"   Es PreCombat: {preCombat}");
        Debug.Log($"   No es Tutorial: {notTutorial}");
        Debug.Log($"   Botones a mostrar: {shouldShowButtons}");

        for (int i = 0; i < contractButtons.Length; i++)
        {
            if (contractButtons[i] != null)
            {
                // SIEMPRE OCULTAR EN TUTORIAL
                if (!notTutorial)
                {
                    contractButtons[i].SetActive(false);
                    Debug.Log($"   Button[{i}] ({contractButtons[i].name}): Forzado a oculto por Tutorial");
                    continue;
                }

                var contractButton = contractButtons[i].GetComponent<ContractButton>();
                bool hasContract = contractButton != null && contractButton.HasContract();
                bool shouldShow = shouldShowButtons && hasContract;

                bool wasActive = contractButtons[i].activeInHierarchy;
                contractButtons[i].SetActive(shouldShow);

                Debug.Log($"   Button[{i}] ({contractButtons[i].name}):");
                Debug.Log($"     - Tiene contrato: {hasContract}");
                Debug.Log($"     - Debe mostrar: {shouldShowButtons}");
                Debug.Log($"     - Resultado: {wasActive} → {shouldShow}");
            }
            else
            {
                Debug.LogWarning($"   Button[{i}] es NULL!");
            }
        }

        Debug.Log("=== VISIBILIDAD ACTUALIZADA ===");
    }

    private void RefreshContractButtonsArray()
    {
        // Busca todos los ContractButton activos en la escena y guarda sus GameObjects
        var foundButtons = FindObjectsOfType<ContractButton>(includeInactive: true);
        contractButtons = new GameObject[foundButtons.Length];
        for (int i = 0; i < foundButtons.Length; i++)
        {
            contractButtons[i] = foundButtons[i].gameObject;
        }
        Debug.Log($"[RefreshContractButtonsArray] Encontrados {contractButtons.Length} botones de contrato.");
    }
    private void LoadContractsForCurrentAct()
    {
        RefreshContractButtonsArray();
        Debug.Log("=== INICIANDO LoadContractsForCurrentAct ===");

        if (ActManager.Instance == null)
        {
            Debug.LogError("❌ ActManager.Instance es null!");
            return;
        }

        if (actContractsManager == null)
        {
            Debug.LogError("❌ actContractsManager es null! ¿Está asignado en el Inspector?");
            return;
        }

        int currentAct = (int)ActManager.Instance.CurrentAct;
        Debug.Log($"📊 Cargando contratos para ACTO {currentAct}");

        // Debug del ActContractsManager
        Debug.Log($"📋 ActContractsManager tiene {actContractsManager.contractsByAct.Count} elementos:");
        for (int i = 0; i < actContractsManager.contractsByAct.Count; i++)
        {
            var element = actContractsManager.contractsByAct[i];
            Debug.Log($"   [{i}] Act Number: {element.actNumber}");
            Debug.Log($"       Legal: {(element.legalContract != null ? element.legalContract.Title : "NULL")}");
            Debug.Log($"       Illegal: {(element.illegalContract != null ? element.illegalContract.Title : "NULL")}");
        }

        // Primero, limpia todos los contratos
        Debug.Log("🧹 Limpiando contratos existentes...");
        ClearAllContracts();

        if (currentAct == 0)
        {
            Debug.Log("📚 Tutorial activo (acto 0) - No se cargan contratos");
            UpdateContractButtonsState();
            return;
        }

        Debug.Log($"🔍 Buscando contratos para acto {currentAct}...");
        var contracts = actContractsManager.GetContractsForAct(currentAct);

        if (contracts == null)
        {
            Debug.LogError($"❌ No se encontraron contratos para el acto {currentAct}!");
            Debug.Log("💡 Verifica que el ActContractsManager tenga un elemento con actNumber = " + currentAct);
            UpdateContractButtonsState();
            return;
        }

        Debug.Log($"✅ Contratos encontrados para acto {currentAct}:");
        Debug.Log($"   Legal: {(contracts.legalContract != null ? contracts.legalContract.Title : "NULL")}");
        Debug.Log($"   Illegal: {(contracts.illegalContract != null ? contracts.illegalContract.Title : "NULL")}");

        // Debug de contractButtons
        Debug.Log($"🎮 Contract Buttons disponibles: {contractButtons.Length}");
        for (int i = 0; i < contractButtons.Length; i++)
        {
            Debug.Log($"   Button[{i}]: {(contractButtons[i] != null ? contractButtons[i].name : "NULL")}");
            if (contractButtons[i] != null)
            {
                var contractButton = contractButtons[i].GetComponent<ContractButton>();
                Debug.Log($"   Button[{i}] ContractButton component: {(contractButton != null ? "✅" : "❌")}");
            }
        }

        // Asigna el contrato legal al primer botón
        if (contractButtons.Length > 0 && contractButtons[0] != null && contracts.legalContract != null)
        {
            Debug.Log("🏛️ Asignando contrato LEGAL...");
            var contractButton = contractButtons[0].GetComponent<ContractButton>();
            if (contractButton != null)
            {
                contractButton.SetContract(contracts.legalContract);
                Debug.Log($"✅ Contrato legal asignado: {contracts.legalContract.Title}");
                Debug.Log($"✅ Botón legal activado: {contractButtons[0].activeInHierarchy}");
            }
            else
            {
                Debug.LogError("❌ ContractButton component no encontrado en contractButtons[0]!");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ No se puede asignar contrato legal:");
            Debug.LogWarning($"   - Botones disponibles: {contractButtons.Length}");
            Debug.LogWarning($"   - Button[0] existe: {contractButtons.Length > 0 && contractButtons[0] != null}");
            Debug.LogWarning($"   - Contrato legal existe: {contracts.legalContract != null}");
        }

        // Asigna el contrato ilegal al segundo botón
        if (contractButtons.Length > 1 && contractButtons[1] != null && contracts.illegalContract != null)
        {
            Debug.Log("🏴‍☠️ Asignando contrato ILEGAL...");
            var contractButton = contractButtons[1].GetComponent<ContractButton>();
            if (contractButton != null)
            {
                contractButton.SetContract(contracts.illegalContract);
                Debug.Log($"✅ Contrato ilegal asignado: {contracts.illegalContract.Title}");
                Debug.Log($"✅ Botón ilegal activado: {contractButtons[1].activeInHierarchy}");
            }
            else
            {
                Debug.LogError("❌ ContractButton component no encontrado en contractButtons[1]!");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ No se puede asignar contrato ilegal:");
            Debug.LogWarning($"   - Botones disponibles: {contractButtons.Length}");
            Debug.LogWarning($"   - Button[1] existe: {contractButtons.Length > 1 && contractButtons[1] != null}");
            Debug.LogWarning($"   - Contrato ilegal existe: {contracts.illegalContract != null}");
        }

        // Actualiza la visibilidad después de cargar los contratos
        Debug.Log("👁️ Actualizando visibilidad de botones...");
        UpdateContractButtonsState();

        lastActLoaded = (int)ActManager.Instance.CurrentAct;
        Debug.Log("=== FINALIZANDO LoadContractsForCurrentAct ===");
    }

    private void ClearAllContracts()
    {
        Debug.Log("🧹 Limpiando todos los contratos...");
        for (int i = 0; i < contractButtons.Length; i++)
        {
            if (contractButtons[i] != null)
            {
                var contractButton = contractButtons[i].GetComponent<ContractButton>();
                if (contractButton != null)
                {
                    contractButton.SetContract(null);
                    Debug.Log($"   Button[{i}] limpiado (contrato removido)");
                    // NO cambiar visibilidad aquí - UpdateContractButtonsState() se encarga
                }
                else
                {
                    Debug.LogWarning($"   Button[{i}] no tiene ContractButton component");
                }
            }
            else
            {
                Debug.LogWarning($"   Button[{i}] es NULL o ha sido destruido");
            }
        }
    }
}