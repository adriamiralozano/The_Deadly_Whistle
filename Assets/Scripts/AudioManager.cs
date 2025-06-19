using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Efectos de sonido")]
    public AudioClip cardDrawClip;
    public AudioClip cardHoverClip;
    public AudioClip WeaponEquippedSound;
    public AudioClip EnemyWeaponEquippedSound;
    public AudioClip EnemyCardAppearSound;
    public AudioClip cardDropSound;
    public AudioClip BangSound;
    public AudioClip BulletReloadSound;
    public AudioClip BulletFailSound;
    public AudioClip ReloadCompleteSound;
    public AudioClip ButtonPressSound;
    public AudioClip ButtonReleaseSound;
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
    public void PlayWeaponEquipped() => PlaySFX(WeaponEquippedSound);
    public void PlayEnemyWeaponEquipped() => PlaySFX(EnemyWeaponEquippedSound);
    public void PlayEnemyCardAppear() => PlaySFX(EnemyCardAppearSound);
    public void PlayCardDrop() => PlaySFX(cardDropSound);
    public void PlayBangSound() => PlaySFX(BangSound);
    public void PlayBulletReload() => PlaySFX(BulletReloadSound);
    public void PlayBulletFail() => PlaySFX(BulletFailSound);
    public void PlayReloadComplete() => PlaySFX(ReloadCompleteSound);
    public void PlayButtonPress() => PlaySFX(ButtonPressSound);
    public void PlayButtonRelease() => PlaySFX(ButtonReleaseSound);

}