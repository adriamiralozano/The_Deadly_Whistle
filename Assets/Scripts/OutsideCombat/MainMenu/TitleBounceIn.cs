using UnityEngine;
using DG.Tweening;

public class TitleBounceIn : MonoBehaviour
{
    [SerializeField] private float startYOffset = 300f; // Desplazamiento inicial hacia arriba
    [SerializeField] private float animDuration = 1.2f; // Duración de la animación

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.position;
        transform.position = originalPosition + Vector3.up * startYOffset;
        transform.DOMoveY(originalPosition.y, animDuration)
                 .SetEase(Ease.OutBounce);
    }
}