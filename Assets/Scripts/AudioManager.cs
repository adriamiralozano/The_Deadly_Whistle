using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Efectos de sonido")]
    public AudioClip cardDrawClip;
    public AudioClip cardHoverClip;
    /*public AudioClip cardPlayClip;
    public AudioClip qteSuccessClip;
    public AudioClip qteFailClip;
    public AudioClip beerClip;
    public AudioClip bagClip; */
    // ...otros clips...

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip, volume);
    }

    // Métodos de conveniencia
    public void PlayCardDraw() => PlaySFX(cardDrawClip);
    public void PlayCardHover() => PlaySFX(cardHoverClip);
    /*public void PlayCardPlay() => PlaySFX(cardPlayClip);
    public void PlayQTESuccess() => PlaySFX(qteSuccessClip);
    public void PlayQTEFail() => PlaySFX(qteFailClip);
    public void PlayBeer() => PlaySFX(beerClip);
    public void PlayBag() => PlaySFX(bagClip); */
}