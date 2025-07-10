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

        if (ActManager.Instance != null)
        {
            data.currentAct = (int)ActManager.Instance.CurrentAct;
            data.currentPhase = (int)ActManager.Instance.CurrentPhase;

        }
        if (GameStats.Instance != null)
        {
            data.familyMoney = GameStats.Instance.familyMoney;
            data.gangMoney = GameStats.Instance.gangMoney;
            data.playerMoney = GameStats.Instance.playerMoney;
            data.completedContracts = new List<string>(GameStats.Instance.completedContracts);
        }


        return data;
    }

    public void NewGame()
    {
        Debug.Log("NewGame called. Creating default save data...");
        SaveData newSave = new SaveData();
        newSave.currentAct = 1; // Act1
        newSave.currentPhase = 0; // PreCombat
        newSave.playerMoney = 0; 
        newSave.familyMoney = 0;
        newSave.gangMoney = 0;
        newSave.completedContracts = new List<string>();
        SaveGame(newSave);
        Debug.Log("Contenido del guardado tras NewGame: " + File.ReadAllText(savePath));
    }
    

}