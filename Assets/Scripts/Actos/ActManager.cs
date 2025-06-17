using UnityEngine;

public class ActManager : MonoBehaviour
{
    public static ActManager Instance { get; private set; }
    public GameAct CurrentAct { get; private set; } = GameAct.Tutorial;
    public ActPhase CurrentPhase { get; private set; } = ActPhase.PreCombat;

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
        if (Input.GetKeyDown(KeyCode.S))
        {
            RetrocedeAct();
            Debug.Log("Acto retrocedido manualmente. Acto actual: " + CurrentAct);
        }

        // Mostrar el acto actual al pulsar la tecla B
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("Acto actual: " + CurrentAct);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("Fase actual: " + CurrentPhase);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            AdvancePhase();
            Debug.Log("Fase cambiada manualmente. Nueva fase: " + CurrentPhase);
        }
            if (Input.GetKeyDown(KeyCode.I))
        {
            if (GameStats.Instance != null)
            {
                GameStats.Instance.playerMoney += 50;
                SaveManager.Instance.SaveCurrentGame();
                Debug.Log($"Dinero añadido. Nuevo playerMoney: {GameStats.Instance.playerMoney}");
            }
        }
    }

    public void LoadFromSave(SaveData data)
    {
        if (data != null)
        {
            CurrentAct = (GameAct)data.currentAct;
            CurrentPhase = (ActPhase)data.currentPhase;
        }
    }

    public void AdvanceAct()
    {
        if (CurrentAct < GameAct.Epilogue)
        {
            CurrentAct++;
            CurrentPhase = ActPhase.PreCombat;
            SaveManager.Instance.SaveCurrentGame();
            Debug.Log("¡Acto avanzado! Nuevo acto: " + CurrentAct + ", fase: " + CurrentPhase);
        }
        else
        {
            Debug.Log("Ya estás en el Epilogue, no se puede avanzar más.");
        }
    }

    public void RetrocedeAct()
    {
        if (CurrentAct < GameAct.Epilogue)
        {
            CurrentAct--;
            CurrentPhase = ActPhase.PreCombat;
            SaveManager.Instance.SaveCurrentGame();
            Debug.Log("¡Acto retrocedido! Nuevo acto: " + CurrentAct + ", fase: " + CurrentPhase);
        }
        else
        {
            Debug.Log("Ya estás en el Epilogue, no se puede avanzar más.");
        }
    }

    public void AdvancePhase()
    {
        if (CurrentPhase == ActPhase.PreCombat)
        {
            CurrentPhase = ActPhase.PostCombat;
        }
        else
        {
            AdvanceAct();
            CurrentPhase = ActPhase.PreCombat;
        }
        SaveManager.Instance.SaveCurrentGame();
        Debug.Log("Fase cambiada. Nueva fase: " + CurrentPhase + ", Acto: " + CurrentAct);
    }
    

}