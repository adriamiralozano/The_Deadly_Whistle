using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private string savePath => Application.persistentDataPath + "/save.json";

    void Start()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("No hay guardado, creando uno nuevo por defecto.");
            NewGame();
        }
    }
    private void Awake()
    {

        Debug.Log("SaveManager Awake. Persistent Data Path: " + Application.persistentDataPath);
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

    public void SaveGame(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public void SaveCurrentGame()
    {
        SaveData data = GatherCurrentGameData();
        SaveGame(data);
    }

    public SaveData LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return null;
    }

    private SaveData GatherCurrentGameData()
    {
        SaveData data = new SaveData();

        // Ejemplo: Recoge el acto actual
        if (ActManager.Instance != null)
            data.currentAct = (int)ActManager.Instance.CurrentAct;

        // Recoge el dinero del jugador
        // data.playerMoney = PlayerStats.Instance.Money; // Si tienes un PlayerStats.Instance

        // Recoge la baraja actual
        // data.deckCardIDs = CardManager.Instance.GetCurrentDeckIDs();

        // Recoge el estado de los diálogos
        // data.dialogueStates = DialogueManager.Instance.GetDialogueStates();

        // Recoge contratos completados/fallidos
        // data.completedContracts = ContractManager.Instance.GetCompletedContracts();
        // data.failedContracts = ContractManager.Instance.GetFailedContracts();

        // ...añade aquí el resto de datos relevantes...

        return data;
    }

    public void NewGame()
    {

        Debug.Log("NewGame called. Creating default save data...");
        SaveData newSave = new SaveData();
        newSave.currentAct = 0; // Tutorial
        newSave.playerMoney = 0;
        newSave.deckCardIDs = new List<string>();
        newSave.dialogueStates = new Dictionary<string, int>();
        newSave.completedContracts = new List<string>();
        newSave.failedContracts = new List<string>();
        // ...otros valores por defecto...

        SaveGame(newSave);

        Debug.Log("Contenido del guardado tras NewGame: " + File.ReadAllText(savePath));
    }
}