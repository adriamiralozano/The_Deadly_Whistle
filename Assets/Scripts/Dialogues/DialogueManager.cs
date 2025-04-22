using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    public DialogueNode startNode;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI optionText;
    public Button previousOptionButton;
    public Button nextOptionButton;
    public Button selectOptionButton;

    [Header("Settings")]
    public bool loopOptions = true;

    private DialogueNode currentNode;
    private int currentOptionIndex = 0;

    void Start()
    {
        // Asignar listeners
        previousOptionButton.onClick.AddListener(SelectPreviousOption);
        nextOptionButton.onClick.AddListener(SelectNextOption);
        selectOptionButton.onClick.AddListener(ConfirmSelection);

        // Iniciar diálogo
        StartDialogue(startNode);
    }

    public void StartDialogue(DialogueNode node)
    {
        if (node == null)
        {
            EndDialogue();
            return;
        }

        currentNode = node;
        currentOptionIndex = 0;
        
        UpdateDialogueUI();
    }

    void UpdateDialogueUI()
    {
        // Mostrar texto principal
        dialogueText.text = currentNode.dialogueText;

        // Manejar visibilidad de controles de opción
        bool hasOptions = currentNode.options.Count > 0;
        bool hasMultipleOptions = currentNode.options.Count > 1;
        optionText.gameObject.SetActive(hasOptions);
        previousOptionButton.gameObject.SetActive(hasMultipleOptions);
        nextOptionButton.gameObject.SetActive(hasMultipleOptions);
        selectOptionButton.gameObject.SetActive(hasOptions);

        if (hasOptions)
        {
            // Actualizar texto de opción
            optionText.text = currentNode.options[currentOptionIndex].optionText;

            // Manejar interactividad de botones de navegación
            previousOptionButton.interactable = loopOptions || currentOptionIndex > 0;
            nextOptionButton.interactable = loopOptions || currentOptionIndex < currentNode.options.Count - 1;
        }
    }

    void Update()
    {
        if (currentNode == null || currentNode.options.Count == 0) return;

        // Detección de teclado
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SelectPreviousOption();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SelectNextOption();
        }
        else if (Input.GetKeyDown(KeyCode.Return)) // Enter para confirmar
        {
            ConfirmSelection();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ConfirmSelection();
        }
    }

    void SelectPreviousOption()
    {
        if (currentNode.options.Count <= 1) return;

        currentOptionIndex--;
        if (currentOptionIndex < 0)
        {
            currentOptionIndex = loopOptions ? currentNode.options.Count - 1 : 0;
        }

        UpdateDialogueUI();
    }

    void SelectNextOption()
    {
        if (currentNode.options.Count <= 1) return;

        currentOptionIndex = (currentOptionIndex + 1) % currentNode.options.Count;
        UpdateDialogueUI();
    }
    void ConfirmSelection()
    {
        if (currentNode.options.Count == 0)
        {
            EndDialogue();
            return;
        }

        // Disparar evento si existe
        if (currentNode.options[currentOptionIndex].onSelectEvent != null)
        {
            currentNode.options[currentOptionIndex].onSelectEvent.Invoke();
        }

        // Navegar al siguiente nodo
        StartDialogue(currentNode.options[currentOptionIndex].nextNode);
    }

    void EndDialogue()
    {
        dialogueText.text = "";
        optionText.text = "";
        
        previousOptionButton.gameObject.SetActive(false);
        nextOptionButton.gameObject.SetActive(false);
        selectOptionButton.gameObject.SetActive(false);
    }
}