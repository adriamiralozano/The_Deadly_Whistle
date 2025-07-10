using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MoneyElectionManager : MonoBehaviour
{
    public TMP_Text tituloDiaText;
    public TMP_Text RecompensaText;
    public TMP_Text dineroActualText;
    public TMP_Text dineroFamiliaText;
    public TMP_Text dineroBandaText;

    public Toggle checkboxFamilia;
    public Toggle checkboxBanda;
    private int dineroActualPrueba;
    private int dineroActualInicial;
    private int NuevaCantidadBanda;
    private int NuevaCantidadFamilia;
    private int boteFamilia;
    private int boteBanda;


    private int recompensa;

    [System.Serializable]
    public class MoneyRequest
    {
        public int familia;
        public int banda;
    }

    // Lista inicializada con los valores para cada acto
    public List<MoneyRequest> moneyRequestsPorActo = new List<MoneyRequest>()
    {
        new MoneyRequest() { familia = 50, banda = 20 },   // Acto 1
        new MoneyRequest() { familia = 65, banda = 35 },   // Acto 2
        new MoneyRequest() { familia = 100, banda = 50 }   // Acto 3
    };

    void Start()
    {
        int acto = (int)ActManager.Instance.CurrentAct;
        int index = acto == 0 ? 0 : acto - 1;

        Debug.Log($"[MoneyElectionManager] CurrentAct: {ActManager.Instance.CurrentAct} (int: {acto}), usando índice: {index}");

        if (index < 0 || index >= moneyRequestsPorActo.Count)
        {
            Debug.LogError($"El índice de acto ({index}) está fuera de rango para moneyRequestsPorActo (tamaño: {moneyRequestsPorActo.Count})");
            return;
        }
        int dia = acto;

        string titulo = acto == 0 ? "DAY 1" : $"DAY {dia}";
        tituloDiaText.text = titulo;
        int dineroActual = SaveManager.Instance.LoadGame().playerMoney;
        dineroActualPrueba = dineroActual; ;
        dineroActualText.text = $"Total money ...................... {dineroActualPrueba}";


        int dineroFamilia = moneyRequestsPorActo[index].familia;
        int dineroBanda = moneyRequestsPorActo[index].banda;
        RecompensaText.text = $"Day rewards ...................... {dineroActual}";
        dineroFamiliaText.text = $"Family wastes .................. -{dineroFamilia}";
        dineroBandaText.text = $"Gang wastes ....................... -{dineroBanda}";

        checkboxFamilia.isOn = false;
        checkboxBanda.isOn = false;

        dineroActualInicial = dineroActualPrueba;

        checkboxFamilia.onValueChanged.AddListener((isOn) => OnToggleFamilia(isOn, dineroFamilia));
        checkboxBanda.onValueChanged.AddListener((isOn) => OnToggleBanda(isOn, dineroBanda));
    }

    void OnToggleFamilia(bool isOn, int cantidad)
    {

        Debug.Log($"DineroTotal al reiniciar familia: {dineroActualPrueba}");
        if (isOn)
        {
            NuevaCantidadFamilia = cantidad - dineroActualPrueba;
            if (NuevaCantidadFamilia <= 0)
            {
                NuevaCantidadFamilia = 0;
                dineroFamiliaText.text = $"Family wastes .................. {NuevaCantidadFamilia}";
                Debug.Log($"DineroActual cuando On 1: {dineroActualPrueba}");
            }
            else
            {
                dineroFamiliaText.text = $"Family wastes .................. -{NuevaCantidadFamilia}";
                Debug.Log($"DineroActual cuando On 2: {dineroActualPrueba}");
            }
            dineroActualPrueba = dineroActualPrueba - (cantidad - NuevaCantidadFamilia);
            Debug.Log($"DineroActual cuando On 3: {dineroActualPrueba}");
            dineroActualText.text = $"Total money ...................... {dineroActualPrueba}";

            boteFamilia = cantidad - NuevaCantidadFamilia;
            Debug.Log($"Bote familia: {boteFamilia}");
        }
        else
        {
            Debug.Log($"NuevaCantidad off: {NuevaCantidadFamilia}");
            dineroActualPrueba = dineroActualPrueba + (cantidad - NuevaCantidadFamilia);
            Debug.Log($"DineroTotal cuando Off familia: {dineroActualPrueba}");
            dineroFamiliaText.text = $"Family wastes .................. -{cantidad}";
            dineroActualText.text = $"Total money ...................... {dineroActualPrueba}";
        }
    }


    void OnToggleBanda(bool isOn, int cantidad)
    {

        Debug.Log($"DineroTotal al reiniciar banda: {dineroActualPrueba}");
        if (isOn)
        {
            NuevaCantidadBanda = cantidad - dineroActualPrueba;

            if (NuevaCantidadBanda <= 0)
            {
                NuevaCantidadBanda = 0;
                dineroBandaText.text = $"Gang wastes ....................... {NuevaCantidadBanda}";
                Debug.Log($"DineroActual cuando On 1: {dineroActualPrueba}");
            }
            else
            {
                dineroBandaText.text = $"Gang wastes ....................... -{NuevaCantidadBanda}";
                Debug.Log($"DineroActual cuando On 2: {dineroActualPrueba}");
            }
            dineroActualPrueba = dineroActualPrueba - (cantidad - NuevaCantidadBanda);
            Debug.Log($"DineroActual cuando On 3: {dineroActualPrueba}");
            dineroActualText.text = $"Total money ...................... {dineroActualPrueba}";

            boteBanda = cantidad - NuevaCantidadBanda;
            Debug.Log($"Bote banda: {boteBanda}");

        }
        else
        {
            Debug.Log($"NuevaCantidad off: {NuevaCantidadBanda}");
            dineroActualPrueba = dineroActualPrueba + (cantidad - NuevaCantidadBanda);
            Debug.Log($"DineroTotal cuando Off banda: {dineroActualPrueba}");
            dineroBandaText.text = $"Gang wastes ....................... -{cantidad}";
            dineroActualText.text = $"Total money ...................... {dineroActualPrueba}";
        }
    }

    public void OnSleepButton()
    {

        if (checkboxFamilia.isOn && checkboxBanda.isOn)
        {

            if (GameStats.Instance != null)
            {
                GameStats.Instance.familyMoney += boteFamilia;
                Debug.Log($"Dinero familia guardado: {GameStats.Instance.familyMoney}");
                GameStats.Instance.gangMoney += boteBanda;
                Debug.Log($"Dinero banda guardado: {GameStats.Instance.gangMoney}");
                GameStats.Instance.playerMoney = dineroActualPrueba;
                Debug.Log($"Dinero jugador guardado: {GameStats.Instance.playerMoney}");
            }

            if (SaveManager.Instance != null)
                SaveManager.Instance.SaveCurrentGame();

            if (ActManager.Instance.CurrentAct == GameAct.Act3)
            {
                ActManager.Instance.AdvanceAct();
                GameEndingManager.Instance.DecideEnding();
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameEnding");
            }
            else
            {
                ActManager.Instance.AdvanceAct();
                UnityEngine.SceneManagement.SceneManager.LoadScene("Campamento");
            }
            
        }
        else
        {
            Debug.Log("Debes seleccionar ambas opciones para continuar.");
        }

    }
    public void SyncStatsToGameStats()
    {
        if (GameStats.Instance != null)
        {
            GameStats.Instance.familyMoney += boteFamilia;
            GameStats.Instance.gangMoney += boteBanda;
            GameStats.Instance.playerMoney = dineroActualPrueba;
            Debug.Log($"GameStats sincronizado: familyMoney={boteFamilia}, gangMoney={boteBanda}, playerMoney={dineroActualPrueba}");
        }
    }
}