using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QTEPoint : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public Image image;
    [SerializeField] public Sprite defaultSprite;
    [SerializeField] public Sprite pressedSprite;
    [SerializeField] public Sprite failSprite; // Nuevo sprite para el estado fail

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

        image.sprite = defaultSprite;
        image.color = Color.white; // Asegura que el color no afecte la sprite
        Debug.Log($"[QTEPoint] Punto {idx} inicializado como elemento UI puro");
    }

    public void SetInactive()
    {
        if (image != null)
        {
            var color = image.color;
            color.a = 0.3f; // Semi-transparente
            image.color = color;
        }
    }

    public void SetActive()
    {
        if (image != null)
        {
            image.sprite = defaultSprite;
            image.color = Color.white;
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
        {
            image.sprite = pressedSprite;
            image.color = Color.white;
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBulletReload();
        }
    }

    public void SetFail(float alpha = 1f)
    {
        if (image != null)
        {
            image.sprite = failSprite;
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBulletFail();
        }
    }
}