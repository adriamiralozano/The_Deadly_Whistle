using UnityEngine;
using UnityEngine.UI; // Necesario para acceder al componente Image

public class CardUI : MonoBehaviour
{
    private CardData cardData; // Referencia a los datos de la carta
    private Image cardImage; // Referencia al componente Image del GameObject principal

    void Awake()
    {
        // Obtener la referencia al componente Image en el mismo GameObject
        cardImage = GetComponent<Image>();
        if (cardImage == null)
        {
            Debug.LogError("[CardUI] No se encontró el componente Image en este GameObject. Asegúrate de que el CardUI_Prefab lo tiene.", this);
        }
    }

    public void SetCardData(CardData data)
    {
        cardData = data;

        // Carga el artwork si existe
        if (cardImage != null && data.artwork != null)
        {
            cardImage.sprite = data.artwork;
        }
        else if (cardImage != null)
        {
            // Si no hay artwork, asegura que el sprite sea nulo para no mostrar un sprite anterior
            cardImage.sprite = null;
        }

    }

    public CardData GetCardData()
    {
        return cardData;
    }

}