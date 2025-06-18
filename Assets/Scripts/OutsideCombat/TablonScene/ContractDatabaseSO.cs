using UnityEngine;

[CreateAssetMenu(fileName = "ContractDatabaseSO", menuName = "Contracts/Contract Database")]
public class ContractDatabaseSO : ScriptableObject
{
    public ContractSO[] allContracts;
}