using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [Header("Paneles del tutorial (en orden)")]
    [SerializeField] private List<GameObject> tutorialPanels;

    [Header("Botones de navegación")]
    [SerializeField] private GameObject previousButton;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject backButton; 

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.4f;

    private int currentPanelIndex = 0;
    private bool isFading = false;

    void Start()
    {
        // Inicializa todos los paneles
        for (int i = 0; i < tutorialPanels.Count; i++)
        {
            var cg = tutorialPanels[i].GetComponent<CanvasGroup>();
            if (cg == null) cg = tutorialPanels[i].AddComponent<CanvasGroup>();
            cg.alpha = (i == currentPanelIndex) ? 1f : 0f;
            tutorialPanels[i].SetActive(i == currentPanelIndex);
        }
        UpdateButtons();
    }

    public void NextPanel()
    {
        if (currentPanelIndex < tutorialPanels.Count - 1 && !isFading)
        {
            FadeToPanel(currentPanelIndex, currentPanelIndex + 1);
        }
    }

    public void PreviousPanel()
    {
        if (currentPanelIndex > 0 && !isFading)
        {
            FadeToPanel(currentPanelIndex, currentPanelIndex - 1);
        }
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void FadeToPanel(int from, int to)
    {
        isFading = true;
        GameObject fromPanel = tutorialPanels[from];
        GameObject toPanel = tutorialPanels[to];

        CanvasGroup fromCG = fromPanel.GetComponent<CanvasGroup>();
        CanvasGroup toCG = toPanel.GetComponent<CanvasGroup>();

        toPanel.SetActive(true);
        toCG.alpha = 0f;

        fromCG.DOFade(0f, fadeDuration);
        toCG.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            fromPanel.SetActive(false);
            currentPanelIndex = to;
            UpdateButtons();
            isFading = false;
        });
    }

    private void UpdateButtons()
    {
        if (previousButton != null)
            previousButton.SetActive(currentPanelIndex > 0);

        if (nextButton != null)
            nextButton.SetActive(currentPanelIndex < tutorialPanels.Count - 1);

        if (backButton != null)
            backButton.SetActive(currentPanelIndex == tutorialPanels.Count - 1); 
    }
}