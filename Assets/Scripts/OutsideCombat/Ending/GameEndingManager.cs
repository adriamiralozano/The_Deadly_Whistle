using UnityEngine;
using System.Linq;
using TMPro;

public class GameEndingManager : MonoBehaviour
{
    public static GameEndingManager Instance { get; private set; }

    public ContractDatabaseSO contractDatabase;
    [Header("UI")]
    [SerializeField] private GameObject endingPrefab;
    [SerializeField] private Transform canvasParent;


    void Start()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "GameEnding")
        {
            ShowFinalScreen();
        }
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowEndingUI(GameEnding ending)
    {
        string endingText = GetEndingText(ending);
        Debug.Log($"[ENDING UI] Valor de ending: {ending}");
        Debug.Log($"[ENDING UI] Texto que se va a poner: {endingText}");

        GameObject instance = Instantiate(endingPrefab, canvasParent);
        Debug.Log($"[ENDING UI] Prefab instanciado: {instance.name}");

        var tmpros = instance.GetComponentsInChildren<TextMeshProUGUI>(true);
        Debug.Log($"[ENDING UI] Encontrados {tmpros.Length} componentes TextMeshProUGUI en el prefab instanciado.");

        foreach (var t in tmpros)
        {
            Debug.Log($"[ENDING UI] Antes de asignar: {t.text}");
            t.text = endingText;
            Debug.Log($"[ENDING UI] Después de asignar: {t.text}");
        }
    }
    private string GetEndingText(GameEnding ending)
    {
        switch (ending)
        {
            case GameEnding.FinalBanda:
                return "Gold flowed to the band, securing their loyalty and your position among them. You have earned the respect of your companions, a safe place in camp and the promise of future adventures under the relentless Western sun. However, the echo of gunfire cannot silence the silence of a neglected home. News of your family is few and bleak; the scarcity of resources has taken its toll, and the health of your brother, Thomas, has deteriorated without much-needed help. The camaraderie of the gang is a bitter comfort in the face of the weight of your decisions.";
            case GameEnding.FinalFamilia:
                return "The money sent home has been a balm to your family's arid fate. Your mother and Thomas have found respite, and your brother's education and health, a future. You have fulfilled your promise, ensuring their welfare at the cost of your position. The gang, however, has noticed your lack of commitment. Their cold stares and increasing isolation have made it clear to you that your journey with them is over. You find yourself alone again in the vast and dangerous Wild West, with the satisfaction of having protected your own, but without the protection and companionship the camp once provided.";
            case GameEnding.FinalCalabozo:
                return "The dust of the road has settled, but not the dust of your sins. The shadows of clandestine contracts have followed you, or perhaps it was the audacity to confront Marshall himself. The whistle that once gave you a name now echoes like a condemnation. The cell doors close with a metallic echo, sealing your fate behind bars. The Wild West is relentless, and the law, though slow, has finally caught up with you. Your freedom is now only a memory, a distant echo in the desert. Here, in the darkness, decisions made under the sun haunt you more than any outlaw.";
            case GameEnding.FinalMixto:
                return "You have walked a tightrope, balancing loyalties and needs. Your family, though not without difficulty, has managed to stay afloat, and the gang, while not considering you its most devoted member, has accepted your contribution. There are no great celebrations or devastating tragedies. Life in the Wild West goes on, rough and unguaranteed, but you have managed to forge a path where neither your blood nor your comrades have abandoned you completely. It is a fragile peace, won with a constant balance between duty and survival.";
            default:
                return "Final desconocido.";
        }
    }

    public GameEnding DecideEnding()
    {
        GameStats stats = GameStats.Instance;
        GameEnding ending = GameEnding.None;

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
        return ending;
    }

    public void ShowFinalScreen()
    {
        GameEnding ending = DecideEnding();
        ShowEndingUI(ending);

        SaveManager.Instance.NewGame();
    }
}