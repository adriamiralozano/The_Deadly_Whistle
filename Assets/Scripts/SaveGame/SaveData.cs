using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int currentAct;
    public int playerMoney;
    public List<string> deckCardIDs; // IDs únicas de las cartas en la baraja
    public Dictionary<string, int> dialogueStates; // Ej: {"NPC1": nodoID, "NPC2": nodoID}
    public List<string> completedContracts;
    public List<string> failedContracts;
    // Puedes añadir más campos según crezca tu juego
}