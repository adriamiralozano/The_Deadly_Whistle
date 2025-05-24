// CardUI.cs
using UnityEngine;
using UnityEngine.UI; // Necesario para acceder al componente Image

public class CardUI : MonoBehaviour
{
    private CardData cardData; // Referencia a los datos de la carta
    private Image cardImage; // Referencia al componente Image del GameObject principal

    [Header("Type Colors")]
    public Color weaponColor = Color.red;     // Color para cartas de Arma
    public Color effectColor = Color.yellow; // Color para cartas de Efecto (hechizo/habilidad)
    public Color passiveColor = Color.blue;   // Color para cartas Pasivas
    public Color defaultColor = Color.white;  // Color por defecto si el tipo no está definido

    void Awake()
    {
        // Obtener la referencia al componente Image en el mismo GameObject
        cardImage = GetComponent<Image>();
        if (cardImage == null)
        {
            Debug.LogError("[CardUI] No se encontró el componente Image en este GameObject. Asegúrate de que el CardUI_Prefab lo tiene.", this);
        }
    }

    /// <summary>
    /// Inicializa la UI de la carta con los datos proporcionados y actualiza su color.
    /// </summary>
    /// <param name="data">Los datos de la carta a mostrar.</param>
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

        UpdateCardColor(); // Llama a la función para actualizar el color (tiñendo el artwork/Image)
    }

    public CardData GetCardData()
    {
        return cardData;
    }

    /// <summary>
    /// Actualiza el color del componente Image de la carta UI basado en el CardType de su CardData.
    /// </summary>
    private void UpdateCardColor()
    {
        if (cardImage == null || cardData == null)
        {
            return; // Salir si no hay imagen o datos de carta
        }

        switch (cardData.type)
        {
            case CardType.Weapon:
                cardImage.color = weaponColor;
                break;
            case CardType.Effect:
                cardImage.color = effectColor;
                break;
            case CardType.Passive:
                cardImage.color = passiveColor;
                break;
            case CardType.None:
            default:
                cardImage.color = defaultColor;
                break;
        }
    }
}