using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpriteSceneLoader : MonoBehaviour
{
    [HideInInspector]
    public string sceneName;

    void OnMouseDown()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("No se ha seleccionado ninguna escena para " + gameObject.name);
        }
    }
}
