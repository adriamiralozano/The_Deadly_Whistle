using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "DialogueNode", menuName = "Dialog System/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    [TextArea(3, 5)]
    public string dialogueText;
    
    public List<DialogueOption> options;

    private void OnValidate()
    {
        if (options == null) options = new List<DialogueOption>();
        if (options.Count == 0) options.Add(new DialogueOption());
    }
}

[System.Serializable]
public class DialogueOption
{
    [TextArea(1, 3)]
    public string optionText = "Continue...";
    public DialogueNode nextNode;
    public UnityEvent onSelectEvent; 
}