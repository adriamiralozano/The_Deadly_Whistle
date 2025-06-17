using System.Collections.Generic;
using UnityEngine;

public class GameStats : MonoBehaviour
{
    public static GameStats Instance { get; private set; }

    // Stats importantes
    public int familyMoney;
    public int gangMoney;
    public int playerMoney;
    
    // Lista de contratos completados
    public List<string> completedContracts = new List<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("GameStats Start");
        SaveData data = SaveManager.Instance.LoadGame();
        if (data != null)
        {
            LoadFromSave(data);
            Debug.Log($"GameStats cargado: familyMoney={familyMoney}, gangMoney={gangMoney}, playerMoney={playerMoney}");
        }
        else
        {
            Debug.Log("No hay datos de guardado, GameStats mantiene valores por defecto.");
        }
    }

    public void LoadFromSave(SaveData data)
    {
        if (data != null)
        {
            familyMoney = data.familyMoney;
            gangMoney = data.gangMoney;
            playerMoney = data.playerMoney;
            //completedContracts = data.completedContracts ?? new List<string>();
        }
    }

    public void AddCompletedContract(string contractTitle)
    {
        if (!completedContracts.Contains(contractTitle))
        {
            completedContracts.Add(contractTitle);
            Debug.Log($"Contrato completado añadido: {contractTitle}");
        }
    }
}