using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    private CardData cardData;
    private Image cardImage;
    public Sprite CardSprite => cardData != null ? cardData.artwork : null;


    void Awake()
    {
        cardImage = GetComponent<Image>();
        if (cardImage == null)
        {
            Debug.LogError("[CardUI] No se encontró el componente Image en este GameObject. Asegúrate de que el CardUI_Prefab lo tiene.", this);
        }
    }

    public void SetCardData(CardData data)
    {
        cardData = data;

        if (cardImage != null && data.artwork != null)
        {
            cardImage.sprite = data.artwork;
        }
        else if (cardImage != null)
        {
            cardImage.sprite = null;
        }

    }

    public CardData GetCardData()
    {
        return cardData;
    }



}