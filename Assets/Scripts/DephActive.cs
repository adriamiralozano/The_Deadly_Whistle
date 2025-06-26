using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DephActive : MonoBehaviour
{
    void OnEnable()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            PostProcessVolume ppv = mainCam.GetComponent<PostProcessVolume>();
            if (ppv != null)
            {
                ppv.enabled = true;
            }
        }
    }
    void OnDisable()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            PostProcessVolume ppv = mainCam.GetComponent<PostProcessVolume>();
            if (ppv != null)
            {
                ppv.enabled = false;
            }
        }
    }
}