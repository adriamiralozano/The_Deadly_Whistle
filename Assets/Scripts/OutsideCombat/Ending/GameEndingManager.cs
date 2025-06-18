using UnityEngine;
using System.Linq;

public class GameEndingManager : MonoBehaviour
{
    public static GameEndingManager Instance { get; private set; }

    public ContractDatabaseSO contractDatabase; // Asigna el asset en el inspector

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