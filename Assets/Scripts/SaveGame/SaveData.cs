using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int currentAct;
    public int currentPhase; 
    public int playerMoney;
    public List<string> deckCardIDs;
    public Dictionary<string, int> dialogueStates;
    public List<string> completedContracts;
    public List<string> failedContracts;
}