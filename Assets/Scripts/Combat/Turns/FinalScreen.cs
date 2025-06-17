using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using System.Collections;

public class FinalScreen : MonoBehaviour
{
    [Header("Panel de contrato")]
    public GameObject contratoFallidoPanel;
    public GameObject contratoCompletadoPanel;

    [Header("PostProcess")]
    public PostProcessVolume postProcessVolume;

    [Header("Bloqueo de interacción")]
    public GameObject blockerOverlay; 

    private void OnEnable()
    {
        PlayerStats.OnPlayerDeath += ShowContratoFallido;
        Enemy.OnEnemyDied += ShowContratoCompletado;
    }

    private void OnDisable()
    {
        PlayerStats.OnPlayerDeath -= ShowContratoFallido;
        Enemy.OnEnemyDied -= ShowContratoCompletado;
    }

    private void ShowContratoFallido()
    {
        StartCoroutine(ShowContratoFallidoCoroutine());
    }

    private IEnumerator ShowContratoFallidoCoroutine()
    {
        yield return new WaitForSeconds(2f);

        // CONTRATO PERDIDO - Restaura estado pre-contrato
        if (ContractManager.Instance != null)
        {
            ContractManager.Instance.OnContractLost();
        }

        if (contratoFallidoPanel != null)
            contratoFallidoPanel.SetActive(true);

        if (postProcessVolume != null)
            postProcessVolume.enabled = true;

        if (blockerOverlay != null)
            blockerOverlay.SetActive(true);

        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene("Tablon"); // Vuelve al Tablon
    }

    private void ShowContratoCompletado(Enemy enemy)
    {
        StartCoroutine(ShowContratoCompletadoCoroutine());
    }

    private IEnumerator ShowContratoCompletadoCoroutine()
    {
        yield return new WaitForSeconds(2f);

        // CONTRATO GANADO - Añade dinero y contrato completado
        if (ContractManager.Instance != null)
        {
            ContractManager.Instance.OnContractWon();
        }

        if (contratoCompletadoPanel != null)
            contratoCompletadoPanel.SetActive(true);

        if (postProcessVolume != null)
            postProcessVolume.enabled = true;

        if (blockerOverlay != null)
            blockerOverlay.SetActive(true);

        if (ActManager.Instance != null)
            ActManager.Instance.AdvancePhase();

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveCurrentGame();

        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene("Campamento"); // Va al Campamento
    }
}