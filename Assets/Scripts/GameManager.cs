using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public UIManager uiManager;

    public bool isPaused = false;
    public int daysSurvived = 0;

    // Gündüz gece döngüsü simülasyonu
    private float dayCycleTimer = 0f;
    private float fullDayLength = 120f; // 2 dakika = 1 tam gün

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        HandlePauseInput();
        UpdateDayNightCycle();
    }

    private void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        uiManager.ShowPausePanel(isPaused);
    }

    private void UpdateDayNightCycle()
    {
        if (isPaused) return;

        dayCycleTimer += Time.deltaTime;
        uiManager.dayNightBar.value = dayCycleTimer / fullDayLength;

        if (dayCycleTimer >= fullDayLength)
        {
            dayCycleTimer = 0f;
            daysSurvived++;
        }

        // Metin güncellemesi (Gündüz, Öðlen vs.)
        float progress = uiManager.dayNightBar.value;
        if (progress < 0.25f) uiManager.dayNightText.text = "Gündüz";
        else if (progress < 0.5f) uiManager.dayNightText.text = "Öðlen";
        else if (progress < 0.75f) uiManager.dayNightText.text = "Akþam";
        else uiManager.dayNightText.text = "Gece";
    }
}