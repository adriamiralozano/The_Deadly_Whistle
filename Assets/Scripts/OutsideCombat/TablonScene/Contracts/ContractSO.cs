using UnityEngine;

[CreateAssetMenu(fileName = "NewContract", menuName = "Tablon/Contract")]
public class ContractSO : ScriptableObject
{
    public string Title;
    [TextArea] public string Description;
    public int Price;
    public Sprite PreviewSprite;
    public bool isIllegal;
}