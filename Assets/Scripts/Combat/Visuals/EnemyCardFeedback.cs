using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyCardFeedback : MonoBehaviour
{
    [Header("Card Feedback Settings")]
    [SerializeField] public Canvas feedbackCanvas;
    [SerializeField] public GameObject cardFeedbackPrefab;
    public float showDuration = 1f;
    public Vector3 offsetFromEnemy = new Vector3(100f, 50f, 0f);

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

        GameObject cardObj = Instantiate(cardFeedbackPrefab, feedbackCanvas.transform);
        Image cardImage = cardObj.GetComponent<Image>();
        cardImage.sprite = cardSprite;

        RectTransform rt = cardObj.GetComponent<RectTransform>();
        Vector3 targetPos = Camera.main.WorldToScreenPoint(enemyPosition) + offsetFromEnemy;
        Vector3 startPos = new Vector3(Screen.width + rt.rect.width, targetPos.y, targetPos.z);

        rt.position = startPos;

        float elapsedTime = 0f;
        float entryDuration = 0.5f;
        while (elapsedTime < entryDuration)
        {
            rt.position = Vector3.Lerp(startPos, targetPos, elapsedTime / entryDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        rt.position = targetPos;

        yield return new WaitForSeconds(1.5f);

        elapsedTime = 0f;
        float exitDuration = 0.5f;
        while (elapsedTime < exitDuration)
        {
            rt.position = Vector3.Lerp(targetPos, startPos, elapsedTime / exitDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(cardObj);
    }
}