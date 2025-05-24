using UnityEngine;

public class PositionWatcher : MonoBehaviour
{
    RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Solo si realmente cambia
        if (rectTransform.localPosition != Vector3.zero && Time.frameCount < 10) 
        {
            Debug.Log($"[Watcher - {gameObject.name} - Frame {Time.frameCount}] localPosition: {rectTransform.localPosition}");
        }
        if (rectTransform.localPosition == Vector3.zero && Time.frameCount > 1 && Time.frameCount < 10)
        {
            Debug.LogWarning($"[Watcher - {gameObject.name} - Frame {Time.frameCount}] localPosition REVERTED to ZERO!");
        }
    }
}