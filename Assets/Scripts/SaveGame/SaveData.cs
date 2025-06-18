using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int currentAct;
    public int currentPhase;
    public int playerMoney;
    public int familyMoney;
    public int gangMoney;
    //lista de contratos completados
    public List<string> completedContracts = new List<string>();

/*     public List<string> deckCardIDs;
                    public Dictionary<string, int> dialogueStates;
                    public List<string> completedContracts;
                    public List<string> failedContracts; */
}