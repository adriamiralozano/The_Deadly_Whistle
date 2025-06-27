using UnityEngine;
using System.Linq;
using TMPro;

public class GameEndingManager : MonoBehaviour
{
    public static GameEndingManager Instance { get; private set; }

    public ContractDatabaseSO contractDatabase;
    [Header("UI")]
    [SerializeField] private GameObject endingPrefab; // Asigna tu prefab en el inspector
    [SerializeField] private Transform canvasParent;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void ShowEndingUI()
    {
        GameEnding ending = GameStats.Instance.currentEnding;
        string endingText = GetEndingText(ending);

        GameObject instance = Instantiate(endingPrefab, canvasParent);

        TextMeshProUGUI textComp = instance.GetComponentInChildren<TextMeshProUGUI>();
        if (textComp != null)
            textComp.text = endingText;
    }
    private string GetEndingText(GameEnding ending)
    {
        switch (ending)
        {
            case GameEnding.FinalBanda:
                return "Final de la Banda: Has priorizado a la banda sobre la familia.";
            case GameEnding.FinalFamilia:
                return "Final de la Familia: Has priorizado a la familia sobre la banda.";
            case GameEnding.FinalCalabozo:
                return "Final Calabozo: Has acabado en prisión por tus actos ilegales.";
            case GameEnding.FinalMixto:
                return "Final Mixto: Has equilibrado tus intereses entre familia y banda.";
            default:
                return "Final desconocido.";
        }
    }
    public void DecideEnding()
    {
        GameStats stats = GameStats.Instance;
        GameEnding ending = GameEnding.None;

        // Usar el array del ScriptableObject contenedor
        var completedContractSOs = contractDatabase.allContracts
            .Where(c => stats.completedContracts.Contains(c.Title))
            .ToList();

        int ilegalesCompletados = completedContractSOs.Count(c => c.isIllegal);
        bool marshallCompletado = completedContractSOs.Any(c => c.Title == "Marshall");

        if (ilegalesCompletados >= 2)
        {
            ending = GameEnding.FinalCalabozo;
        }
        else if (marshallCompletado)
        {
            ending = GameEnding.FinalCalabozo;
        }
        else if (stats.familyMoney >= 107 && stats.gangMoney >= 52)
        {
            ending = GameEnding.FinalMixto;
        }
        else if (stats.familyMoney < 107)
        {
            ending = GameEnding.FinalBanda;
        }
        else if (stats.gangMoney < 52)
        {
            ending = GameEnding.FinalFamilia;
        }

        stats.currentEnding = ending;
        Debug.Log($"[ENDING] Final determinado: {ending}");
    }
}