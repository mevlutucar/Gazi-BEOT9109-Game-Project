using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public UIManager uiManager;

    [Header("Day & Night System")]
    public Light directionalLight;
    public float realMinutesPerGameDay = 5f;
    private float gameMinutesPerRealSecond;
    internal float currentTimeInMinutes = 360f;
    public int daysSurvived = 1;

    [Header("NPC Management (Pooling)")]
    public Transform allyNPCsParent;
    public Transform enemyNPCsParent;

    [Header("Audio System & Volumes")]
    public AudioSource levelAudioSource;

    public AudioClip lightTempoMusic;
    [Range(0f, 1f)] public float lightTempoVolume = 0.5f;

    public AudioClip actionTempoMusic;
    [Range(0f, 1f)] public float actionTempoVolume = 1f;

    public AudioClip nightAmbianceMusic;
    [Range(0f, 1f)] public float nightAmbianceVolume = 1f;

    [Header("Conversation Audio")]
    public AudioClip conversationMusic;
    [Range(0f, 1f)] public float conversationMusicVolume = 1f;
    private AudioSource conversationAudioSource;

    internal bool isPaused = false;
    private bool hasTriggeredConversation = false;
    private bool isDayMusicPlaying = false;

    // Döngü Tetikleyicileri
    private bool hasTriggeredMorning = false;
    private bool hasTriggeredEvening = false;
    private bool hasTriggeredNight = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        gameMinutesPerRealSecond = 1440f / (realMinutesPerGameDay * 60f);

        // --- BU SATIR EKSÝKTÝ, BURAYA EKLÝYORUZ ---
        // Inspector'daki sýfýrlanmayý ezer, oyunu her zaman Gün 1'den baþlatýr.
        daysSurvived = 1;

        conversationAudioSource = gameObject.AddComponent<AudioSource>();
        conversationAudioSource.loop = true;
        conversationAudioSource.ignoreListenerPause = true;
    }

    void Start()
    {
        PlayMusic(lightTempoMusic);
        isDayMusicPlaying = true;

        uiManager.UpdateDayTimeText(daysSurvived, 6, 0);
    }

    void Update()
    {
        if (!isPaused) UpdateDayNightCycle();
        if (Input.GetKeyDown(KeyCode.Escape) && uiManager.inGameCanvas.activeSelf) TogglePause();
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
            hasTriggeredMorning = false;
            hasTriggeredEvening = false;
            hasTriggeredNight = false;
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
        // 06:00 - SABAHLARI ALLY'LER DOÐAR, ENEMY'LER YOK OLUR
        if (displayTime >= 360f && !hasTriggeredMorning)
        {
            ToggleNPCs(allyNPCsParent, true);
            ToggleNPCs(enemyNPCsParent, false);
            hasTriggeredMorning = true;
        }

        // 18:00 - AKÞAMLARI ENEMY'LER DOÐAR
        if (displayTime >= 1080f && !hasTriggeredEvening)
        {
            ToggleNPCs(enemyNPCsParent, true);
            hasTriggeredEvening = true;
        }

        // 18:59 - GÜNBATIMI SÝNYALÝ (Sadece sinyal gider, Ally'ler burada kapanmaz)
        if (displayTime >= 1139f && displayTime < 1140f && !hasTriggeredNight)
        {
            GameEvents.TriggerSunset();
            hasTriggeredNight = true; // Sinyalin 1 kez gitmesi için
        }

        // 19:01 - KONUÞMA TETÝKLEME
        if (displayTime >= 1141f && displayTime < 1142f && !hasTriggeredConversation)
        {
            isDayMusicPlaying = false;
            PlayMusic(actionTempoMusic);
            TriggerConversation();
            hasTriggeredConversation = true;
        }

        if (displayTime >= 360f && displayTime < 361f && !isDayMusicPlaying)
        {
            PlayMusic(lightTempoMusic);
            isDayMusicPlaying = true;
        }
    }

    private void ToggleNPCs(Transform parentObj, bool state)
    {
        if (parentObj != null)
        {
            foreach (Transform child in parentObj)
            {
                child.gameObject.SetActive(state);
            }
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip != null && levelAudioSource != null && levelAudioSource.clip != clip)
        {
            levelAudioSource.clip = clip;
            levelAudioSource.Play();
        }
    }

    private void UpdateMusicVolumeLive()
    {
        float globalGameMusicVol = PlayerPrefs.GetFloat("GameMusicVol", 1f);

        if (levelAudioSource != null && levelAudioSource.clip != null)
        {
            if (levelAudioSource.clip == lightTempoMusic) levelAudioSource.volume = lightTempoVolume * globalGameMusicVol;
            else if (levelAudioSource.clip == actionTempoMusic) levelAudioSource.volume = actionTempoVolume * globalGameMusicVol;
            else if (levelAudioSource.clip == nightAmbianceMusic) levelAudioSource.volume = nightAmbianceVolume * globalGameMusicVol;
        }

        if (conversationAudioSource != null) conversationAudioSource.volume = conversationMusicVolume * globalGameMusicVol;
    }

    private void TriggerConversation()
    {
        Time.timeScale = 0f;
        isPaused = true;
        uiManager.ShowConversationCanvas();
        AudioListener.pause = true;

        if (conversationMusic != null && conversationAudioSource != null)
        {
            conversationAudioSource.clip = conversationMusic;
            conversationAudioSource.Play();
        }
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
        AudioListener.pause = false;

        if (conversationAudioSource != null && conversationAudioSource.isPlaying) conversationAudioSource.Stop();
        if (hasTriggeredConversation && (currentTimeInMinutes % 1440f) >= 1141f) PlayMusic(nightAmbianceMusic);

        GameEvents.TriggerConversationEnded();
    }
}