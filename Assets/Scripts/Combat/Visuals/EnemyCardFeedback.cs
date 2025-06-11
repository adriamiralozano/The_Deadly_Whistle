using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyCardFeedback : MonoBehaviour
{
    [Header("Card Feedback Settings")]
    [SerializeField] public Canvas feedbackCanvas; // Canvas donde aparecerá la carta
    [SerializeField] public GameObject cardFeedbackPrefab; // Prefab con una Image para mostrar la carta
    public float showDuration = 1f; // Tiempo que se muestra la carta
    public Vector3 offsetFromEnemy = new Vector3(100f, 50f, 0f); // Offset respecto al enemigo

    public IEnumerator ShowCardFeedbackCoroutine(Sprite cardSprite, Vector3 enemyPosition)
    {
        yield return StartCoroutine(ShowCardCoroutine(cardSprite, enemyPosition));
    }

    public void ShowCardFeedback(Sprite cardSprite, Vector3 enemyPosition)
    {
        StartCoroutine(ShowCardCoroutine(cardSprite, enemyPosition));
    }

    private IEnumerator ShowCardCoroutine(Sprite cardSprite, Vector3 enemyPosition)
    {

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyCardAppear();
        }
        // Crear la imagen de la carta
        GameObject cardObj = Instantiate(cardFeedbackPrefab, feedbackCanvas.transform);
        Image cardImage = cardObj.GetComponent<Image>();
        cardImage.sprite = cardSprite;

        // Posiciones: inicial (fuera de pantalla a la derecha) y final (al lado del enemigo)
        RectTransform rt = cardObj.GetComponent<RectTransform>();
        Vector3 targetPos = Camera.main.WorldToScreenPoint(enemyPosition) + offsetFromEnemy;
        Vector3 startPos = new Vector3(Screen.width + rt.rect.width, targetPos.y, targetPos.z); // Fuera de pantalla a la derecha

        // Posición inicial fuera de pantalla
        rt.position = startPos;

        // Animación de entrada: deslizar desde la derecha hacia la posición objetivo
        float elapsedTime = 0f;
        float entryDuration = 0.5f;
        while (elapsedTime < entryDuration)
        {
            rt.position = Vector3.Lerp(startPos, targetPos, elapsedTime / entryDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        rt.position = targetPos;

        // Esperar 1.5 segundos
        yield return new WaitForSeconds(1.5f);

        // Animación de salida: deslizar hacia la derecha (fuera de pantalla)
        elapsedTime = 0f;
        float exitDuration = 0.5f;
        while (elapsedTime < exitDuration)
        {
            rt.position = Vector3.Lerp(targetPos, startPos, elapsedTime / exitDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Destruir
        Destroy(cardObj);
    }
}