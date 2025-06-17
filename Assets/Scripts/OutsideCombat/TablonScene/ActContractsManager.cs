using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActContractsManager", menuName = "Game/ActContractsManager")]
public class ActContractsManager : ScriptableObject
{
    [System.Serializable]
    public class ActContracts
    {
        [Header("Configuración del Acto")]
        public int actNumber;
        [Header("Contratos Disponibles")]
        public ContractSO legalContract;
        public ContractSO illegalContract;
    }

    [Header("Contratos por Acto")]
    public List<ActContracts> contractsByAct = new List<ActContracts>();

    public ActContracts GetContractsForAct(int actNumber)
    {
        return contractsByAct.Find(x => x.actNumber == actNumber);
    }
}