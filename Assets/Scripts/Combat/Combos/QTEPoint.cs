using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QTEPoint : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public Image image;
    [SerializeField] public Color defaultColor = Color.white;
    [SerializeField] public Color pressedColor = Color.green;
    [SerializeField] public Color failColor = Color.red;

    public int index;
    private CombosManager manager;

    public void Init(int idx, CombosManager mgr)
    {
        index = idx;
        manager = mgr;
        if (image == null)
            image = GetComponent<Image>();

        if (image == null)
        {
            Debug.LogError($"[QTEPoint] No se encontró componente Image en {gameObject.name}");
            return;
        }

        image.color = defaultColor;
        Debug.Log($"[QTEPoint] Punto {idx} inicializado como elemento UI puro");
    }

    // AÑADIR ESTOS MÉTODOS AQUÍ:
    public void SetInactive()
    {
        if (image != null)
        {
            var color = defaultColor;
            color.a = 0.3f; // Semi-transparente
            image.color = color;
        }
    }

    public void SetActive()
    {
        if (image != null)
        {
            image.color = defaultColor; // Color normal
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[QTEPoint] Click detectado en punto {index}. CurrentIndex esperado: {(manager != null ? manager.GetCurrentIndex() : -1)}");

        if (manager != null)
        {
            manager.PulsarPunto(index);
        }
        else
        {
            Debug.LogError("[QTEPoint] Manager es null en OnPointerClick");
        }
    }

    public void SetCorrect()
    {
        if (image != null)
            image.color = pressedColor;
    }

    public void SetFail(float alpha = 1f)
    {
        if (image != null)
        {
            var color = failColor;
            color.a = alpha;
            image.color = color;
        }
    }
}