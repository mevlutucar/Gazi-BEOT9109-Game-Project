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

    [Header("Audio System")]
    public AudioSource levelAudioSource;
    public AudioClip lightTempoMusic;
    public AudioClip actionTempoMusic;
    public AudioClip nightAmbianceMusic;

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
    }

    private void UpdateDayNightCycle()
    {
        currentTimeInMinutes += gameMinutesPerRealSecond * Time.deltaTime;

        if (currentTimeInMinutes >= 1800f) // 24:00 (1440) + 06:00 (360)
        {
            currentTimeInMinutes -= 1440f;
            daysSurvived++;
            hasTriggeredConversation = false;
        }

        float displayTime = currentTimeInMinutes % 1440f;
        int hours = Mathf.FloorToInt(displayTime / 60f);
        int minutes = Mathf.FloorToInt(displayTime % 60f);

        uiManager.UpdateDayTimeText(daysSurvived, hours, minutes);

        // Gündüz-Gece Barý (Fill Amount) Hesaplamasý (06:00 ile 06:00 arasý 0'dan 1'e dolar)
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
        if (clip != null && levelAudioSource.clip != clip)
        {
            levelAudioSource.clip = clip;
            levelAudioSource.Play();
        }
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