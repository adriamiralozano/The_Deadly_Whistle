using UnityEngine;

[System.Serializable]
public class ContractData
{
    public string Title;
    public string Description;
    public int Price;
    public Sprite PreviewSprite;
    public bool isIllegal;

    public ContractData(string title, string description, int price, Sprite previewSprite, bool isIllegal)
    {
        Title = title;
        Description = description;
        Price = price;
        PreviewSprite = previewSprite;
        this.isIllegal = isIllegal;
    }
}