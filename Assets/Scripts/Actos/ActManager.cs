using UnityEngine;

public class ActManager : MonoBehaviour
{
    public static ActManager Instance { get; private set; }
    public GameAct CurrentAct { get; private set; } = GameAct.Tutorial;

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

    public void LoadFromSave(SaveData data)
    {
        if (data != null)
            CurrentAct = (GameAct)data.currentAct;
    }

    public void AdvanceAct()
    {
        if (CurrentAct < GameAct.Epilogue)
        {
            CurrentAct++;
            SaveManager.Instance.SaveGame(GatherCurrentGameData());
            // Aquí puedes cargar la escena del siguiente acto o mostrar una transición
        }
    }

    private SaveData GatherCurrentGameData()
    {
        SaveData data = new SaveData();
        data.currentAct = (int)CurrentAct;
        // ...añade el resto de datos necesarios...
        return data;
    }
}