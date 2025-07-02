using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int currentAct;
    public int currentPhase;
    public int playerMoney;
    public int familyMoney;
    public int gangMoney;
    public List<string> completedContracts = new List<string>();


}