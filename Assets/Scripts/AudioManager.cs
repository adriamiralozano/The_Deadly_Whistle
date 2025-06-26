using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }


    [Header("Música de Fondo (OST)")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip backgroundWind;

    private AudioSource musicSource;
    private AudioSource windSource;

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
    public AudioClip IndicatorTurnSound;
    public AudioClip QTEPanelInSound;



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 


        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        
        windSource = gameObject.AddComponent<AudioSource>();
        windSource.playOnAwake = false;
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Combat_Test_Scene")
        {
            PlayMusic(backgroundMusic);
        }
        PlayWind(backgroundWind);
    }


    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;

        musicSource.clip = clip;
        musicSource.loop = true; 
        musicSource.Play();
    }

    public void PlayWind(AudioClip clip)
    {
        if (clip == null || windSource == null) return;

        windSource.clip = clip;
        windSource.loop = true;
        windSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        // Crea un GameObject temporal solo para reproducir este sonido.
        GameObject soundGameObject = new GameObject("SFX_" + clip.name);
        AudioSource tempAudioSource = soundGameObject.AddComponent<AudioSource>();

        // Configura y reproduce el sonido.
        tempAudioSource.clip = clip;
        tempAudioSource.volume = volume;
        tempAudioSource.Play();

        // Destruye el GameObject temporal después de que el clip haya terminado.
        Destroy(soundGameObject, clip.length);
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
    public void PlayIndicatorTurn() => PlaySFX(IndicatorTurnSound);
    public void PlayQTEPanelIn() => PlaySFX(QTEPanelInSound);

}