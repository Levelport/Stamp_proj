using UnityEngine;
using System.Collections;


public class SoundManager_H : MonoBehaviour
{

    public static SoundManager_H Instance;

    [Header("Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    [Header("Volumes")]
    [Range(0, 1)] public float bgmVolume = 1;
    [Range(0, 1)] public float seVolume = 1;

    private const string BGM_VOLUME_KEY ="BGM_VOLUME";
    private const string SE_VOLUME_KEY ="SE_VOLUME";

    private Coroutine bgmCoroutine;

    [SerializeField] private SoundLibrary_H soundLibrary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            soundLibrary.Init();

            bgmVolume=PlayerPrefs.GetFloat(BGM_VOLUME_KEY,1f);
            seVolume=PlayerPrefs.GetFloat(SE_VOLUME_KEY,1f);

            if (bgmSource != null)
                bgmSource.loop = true;

            if (seSource != null)
                seSource.loop = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void PlayBGM(string name, float fadeTime = 1f)
    {
        AudioClip clip = soundLibrary.Get(name);
        if (clip != null)
        {
            ChangeBGM(clip, fadeTime);
        }
    }

    public void ChangeBGM(AudioClip newClip, float fadeTime = 1f)
    {
        if (bgmCoroutine != null) StopCoroutine(bgmCoroutine);
        bgmCoroutine = StartCoroutine(FadeBGM(newClip, fadeTime));
    }

    private IEnumerator FadeBGM(AudioClip newClip, float fadeTime)
    {
        if (bgmSource.isPlaying)
        {
            float t = 0;
            float start = bgmSource.volume;

            while (t < fadeTime)
            {
                bgmSource.volume = Mathf.Lerp(start, 0, t / fadeTime);
                t += Time.deltaTime;
                yield return null;
            }

            bgmSource.volume = 0;
        }

        bgmSource.clip = newClip;
        bgmSource.Play();

        float t2 = 0;
        while (t2 < fadeTime)
        {
            bgmSource.volume = Mathf.Lerp(0, bgmVolume, t2 / fadeTime);
            t2 += Time.deltaTime;
            yield return null;
        }

        bgmSource.volume = bgmVolume;
    }

    public void PlaySE(string name ,float multiply = 1f)
    {
        AudioClip clip = soundLibrary.Get(name);
        if (clip != null)
        {
            seSource.PlayOneShot(clip, seVolume* multiply);
        }
    }
    public void PauseBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (bgmSource.clip != null && !bgmSource.isPlaying)
        {
            bgmSource.UnPause();
        }
    }

    public bool isitPlaying()
    {
        if(bgmSource.isPlaying)
        {return true;}
        else
        {return false;}
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume =Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;

        PlayerPrefs.SetFloat(BGM_VOLUME_KEY,bgmVolume);
    }

    public void SetSEVolume(float volume)
    {
        seVolume=Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SE_VOLUME_KEY,seVolume);
    }

}
