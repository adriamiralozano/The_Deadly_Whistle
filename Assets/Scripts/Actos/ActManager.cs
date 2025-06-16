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

    void Start()
    {
        Debug.Log("ActManager Start. Persistent Data Path: " + Application.persistentDataPath);
        SaveData data = SaveManager.Instance.LoadGame();
        if (data != null)
        {
            LoadFromSave(data);
            Debug.Log("Acto actual al cargar partida: " + CurrentAct);
        }
    }

    private void Update()
    {
        // Solo para pruebas: avanzar de acto al pulsar la tecla A
        if (Input.GetKeyDown(KeyCode.A))
        {
            AdvanceAct();
            Debug.Log("Acto avanzado manualmente. Acto actual: " + CurrentAct);
        }

        // Mostrar el acto actual al pulsar la tecla B
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("Acto actual: " + CurrentAct);
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
            SaveManager.Instance.SaveCurrentGame(); // Centraliza el guardado en SaveManager
            // Aquí puedes cargar la escena del siguiente acto o mostrar una transición
        }
    }

}