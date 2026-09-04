using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    public AudioClip background;
    public AudioClip Game;

    private void Awake()
    {
        // 1. Check if an instance of AudioManager already exists
        if (instance == null)
        {
            instance = this;

            // 2. THIS keeps the Audio Manager alive when changing scenes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 3. Destroys duplicate Audio Managers if you reload the Main Menu scene
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (background != null && musicSource != null)
        {
            musicSource.clip = background;
            musicSource.Play();
        }
    }

    public void ToggleMusic(bool isOn)
    {
        musicSource.mute = !isOn;
    }

    public void ToggleSFX(bool isOn)
    {
        SFXSource.mute = !isOn;
    }
}
