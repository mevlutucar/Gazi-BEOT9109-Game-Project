using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public UIManager uiManager;

    [Header("Day & Night System")]
    public Light directionalLight;
    public float realMinutesPerGameDay = 5f;
    private float gameMinutesPerRealSecond;
    internal float currentTimeInMinutes = 360f; // 06:00 (360. dakika)
    public int daysSurvived = 1; // Oyun 1. Günden baþlar

    [Header("Audio System & Volumes")]
    public AudioSource levelAudioSource;

    public AudioClip lightTempoMusic;
    [Range(0f, 1f)] public float lightTempoVolume = 0.5f;

    public AudioClip actionTempoMusic;
    [Range(0f, 1f)] public float actionTempoVolume = 1f;

    public AudioClip nightAmbianceMusic;
    [Range(0f, 1f)] public float nightAmbianceVolume = 1f;

    internal bool isPaused = false;
    private bool hasTriggeredConversation = false;
    private bool isDayMusicPlaying = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        gameMinutesPerRealSecond = 1440f / (realMinutesPerGameDay * 60f);
    }

    void Start()
    {
        PlayMusic(lightTempoMusic);
        isDayMusicPlaying = true;
    }

    void Update()
    {
        if (!isPaused)
        {
            UpdateDayNightCycle();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && uiManager.inGameCanvas.activeSelf)
        {
            TogglePause();
        }

        // --- CANLI SES KONTROLÜ (Slider'larýn çalýþmasýný saðlar) ---
        UpdateMusicVolumeLive();
    }

    private void UpdateDayNightCycle()
    {
        currentTimeInMinutes += gameMinutesPerRealSecond * Time.deltaTime;

        if (currentTimeInMinutes >= 1800f)
        {
            currentTimeInMinutes -= 1440f;
            daysSurvived++;
            hasTriggeredConversation = false;
        }

        float displayTime = currentTimeInMinutes % 1440f;
        int hours = Mathf.FloorToInt(displayTime / 60f);
        int minutes = Mathf.FloorToInt(displayTime % 60f);

        uiManager.UpdateDayTimeText(daysSurvived, hours, minutes);

        float fillRatio = (currentTimeInMinutes - 360f) / 1440f;
        uiManager.UpdateDayNightBar(fillRatio);

        UpdateLighting(displayTime);
        CheckTimeEvents(displayTime);
    }

    private void UpdateLighting(float displayTime)
    {
        float sunAngle = Mathf.Lerp(-90f, 270f, displayTime / 1440f);
        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, 50f, 0f);

        float intensity = (displayTime > 360f && displayTime < 1140f) ? 1f : 0.1f;
        directionalLight.intensity = Mathf.Lerp(directionalLight.intensity, intensity, Time.deltaTime);
    }

    private void CheckTimeEvents(float displayTime)
    {
        if (displayTime >= 1141f && displayTime < 1142f)
        {
            if (!hasTriggeredConversation)
            {
                isDayMusicPlaying = false;
                PlayMusic(actionTempoMusic);
                TriggerConversation();
                hasTriggeredConversation = true;
            }
        }

        if (displayTime >= 360f && displayTime < 361f && !isDayMusicPlaying)
        {
            PlayMusic(lightTempoMusic);
            isDayMusicPlaying = true;
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        // Burada sadece müziði baþlatýyoruz, sesi "UpdateMusicVolumeLive" yönetecek
        if (clip != null && levelAudioSource.clip != clip)
        {
            levelAudioSource.clip = clip;
            levelAudioSource.Play();
        }
    }

    // Inspector'daki Slider deðerlerini anlýk olarak AudioSource'a aktaran sistem
    private void UpdateMusicVolumeLive()
    {
        if (levelAudioSource == null || levelAudioSource.clip == null) return;

        if (levelAudioSource.clip == lightTempoMusic)
            levelAudioSource.volume = lightTempoVolume;
        else if (levelAudioSource.clip == actionTempoMusic)
            levelAudioSource.volume = actionTempoVolume;
        else if (levelAudioSource.clip == nightAmbianceMusic)
            levelAudioSource.volume = nightAmbianceVolume;
    }

    private void TriggerConversation()
    {
        Time.timeScale = 0f;
        isPaused = true;
        uiManager.ShowConversationCanvas();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        uiManager.ShowPausePanel(isPaused);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        uiManager.ShowInGameCanvas();

        if (hasTriggeredConversation && (currentTimeInMinutes % 1440f) >= 1141f)
        {
            PlayMusic(nightAmbianceMusic);
        }
    }
}