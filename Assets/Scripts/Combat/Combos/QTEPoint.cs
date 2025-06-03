using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QTEPoint : MonoBehaviour
{
    [SerializeField] public Image image;
    [SerializeField] public Color defaultColor = Color.white;
    [SerializeField] public Color pressedColor = Color.green;
    [SerializeField] public Color failColor = Color.red;

    public int index; // El índice de este punto en la secuencia
    private CombosManager manager;

    public void Init(int idx, CombosManager mgr)
    {
        index = idx;
        manager = mgr;
        if (image == null) image = GetComponent<Image>();
        image.color = defaultColor;
    }

    public void OnPress()
    {
        manager.PulsarPunto(index);
    }

    public void SetCorrect()
    {
        image.color = pressedColor;
    }

    public void SetFail(float alpha = 1f)
    {
        var color = failColor;
        color.a = alpha;
        image.color = color;
    }
}