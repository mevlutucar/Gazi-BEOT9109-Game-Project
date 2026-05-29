using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Canvases")]
    public GameObject inGameCanvas;
    public GameObject pauseMenuCanvas;
    public GameObject deathMenuCanvas;
    public GameObject conversationCanvas;

    [Header("InGame UI Elements")]
    public Image healthBarImage;
    public Image staminaBarImage;
    public Image dayNightFillImage; // Inspector'dan InGameCanvas>InGamePanel>DayNightBar>Night objesini buraya ata
    public TextMeshProUGUI rifleBulletTxt;
    public TextMeshProUGUI dayAndTimeTxt;

    [Header("Crosshair Elements")]
    public GameObject crosshairObj;
    public RectTransform leftLine, rightLine, upLine, downLine;

    [Header("NPC Conversation Elements")]
    public GameObject npcObj;
    public GameObject leaderObj;

    [Header("Menu Texts")]
    public TextMeshProUGUI gamePausedTxt;
    public TextMeshProUGUI endGameTxt;

    private bool isAiming = false;
    private float crosshairLerpSpeed = 10f;

    void Start()
    {
        ShowInGameCanvas();
    }

    void Update()
    {
        HandleCrosshairAnimation();
    }

    public void UpdatePlayerBars(float currentHealth, float maxHealth, float currentStamina, float maxStamina)
    {
        // Inspector'dan Image Type: Filled, Method: Horizontal, Origin: Left yaptýðýn objeler burada otomatik dolar/boþalýr.
        healthBarImage.fillAmount = currentHealth / maxHealth;
        staminaBarImage.fillAmount = currentStamina / maxStamina;
    }

    public void UpdateDayNightBar(float fillAmount)
    {
        if (dayNightFillImage != null)
        {
            dayNightFillImage.fillAmount = fillAmount;
        }
    }

    public void UpdateAmmoText(int currentAmmo)
    {
        rifleBulletTxt.text = currentAmmo.ToString();
    }

    public void UpdateDayTimeText(int days, int hours, int minutes)
    {
        // Gün ve Saat çakýþmasýný önlemek için formatlamayý netleþtirdik.
        dayAndTimeTxt.text = "Gün " + days + ", " + hours.ToString("00") + ":" + minutes.ToString("00");
    }

    public void SetAimingState(bool state)
    {
        isAiming = state;
    }

    public void ToggleCrosshairVisibility(bool state)
    {
        crosshairObj.SetActive(state);
    }

    private void HandleCrosshairAnimation()
    {
        if (!crosshairObj.activeSelf) return;

        Vector2 leftPos = isAiming ? new Vector2(-5, 0) : new Vector2(-30, 0);
        float leftWidth = isAiming ? 30f : 40f;

        Vector2 rightPos = isAiming ? new Vector2(55, 0) : new Vector2(80, 0);
        float rightWidth = isAiming ? 30f : 40f;

        Vector2 upPos = isAiming ? new Vector2(25, 25) : new Vector2(25, 50);
        float upWidth = isAiming ? 30f : 40f;

        Vector2 downPos = isAiming ? new Vector2(25, -25) : new Vector2(25, -50);
        float downWidth = isAiming ? 30f : 40f;

        leftLine.anchoredPosition = Vector2.Lerp(leftLine.anchoredPosition, leftPos, Time.deltaTime * crosshairLerpSpeed);
        leftLine.sizeDelta = new Vector2(Mathf.Lerp(leftLine.sizeDelta.x, leftWidth, Time.deltaTime * crosshairLerpSpeed), leftLine.sizeDelta.y);

        rightLine.anchoredPosition = Vector2.Lerp(rightLine.anchoredPosition, rightPos, Time.deltaTime * crosshairLerpSpeed);
        rightLine.sizeDelta = new Vector2(Mathf.Lerp(rightLine.sizeDelta.x, rightWidth, Time.deltaTime * crosshairLerpSpeed), rightLine.sizeDelta.y);

        upLine.anchoredPosition = Vector2.Lerp(upLine.anchoredPosition, upPos, Time.deltaTime * crosshairLerpSpeed);
        upLine.sizeDelta = new Vector2(Mathf.Lerp(upLine.sizeDelta.x, upWidth, Time.deltaTime * crosshairLerpSpeed), upLine.sizeDelta.y);

        downLine.anchoredPosition = Vector2.Lerp(downLine.anchoredPosition, downPos, Time.deltaTime * crosshairLerpSpeed);
        downLine.sizeDelta = new Vector2(Mathf.Lerp(downLine.sizeDelta.x, downWidth, Time.deltaTime * crosshairLerpSpeed), downLine.sizeDelta.y);
    }

    public void ShowInGameCanvas()
    {
        inGameCanvas.SetActive(true);
        pauseMenuCanvas.SetActive(false);
        deathMenuCanvas.SetActive(false);
        conversationCanvas.SetActive(false);
    }

    public void ShowPausePanel(bool state)
    {
        inGameCanvas.SetActive(!state);
        pauseMenuCanvas.SetActive(state);
        AudioListener.pause = state; // Oyun seslerini global olarak kapatýr/açar

        if (state) StartCoroutine(FlashText(gamePausedTxt));
        else StopAllCoroutines();
    }

    public void ShowDeathPanel()
    {
        inGameCanvas.SetActive(false);
        deathMenuCanvas.SetActive(true);
        AudioListener.pause = true; // Karakter öldüðünde oyun seslerini kapatýr
        endGameTxt.text = "OYUN SONA ERDÝ ÞEF! HAYATTA KALINAN GÜN SAYISI: " + GameManager.Instance.daysSurvived;
    }

    public void ShowConversationCanvas()
    {
        inGameCanvas.SetActive(false);
        conversationCanvas.SetActive(true);
        npcObj.SetActive(true);
        leaderObj.SetActive(false);
    }

    private IEnumerator FlashText(TextMeshProUGUI textToFlash)
    {
        while (true)
        {
            textToFlash.enabled = !textToFlash.enabled;
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    // --- Buton Fonksiyonlarý (Script eksiðin tamamen giderildi) ---
    public void Btn_NPC_Talk()
    {
        npcObj.SetActive(false);
        leaderObj.SetActive(true);
    }

    public void Btn_Leader_FinishTalk()
    {
        GameManager.Instance.ResumeGame();
    }

    public void Btn_Resume()
    {
        GameManager.Instance.TogglePause();
    }

    public void Btn_Restart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false; // Müzikleri yeniden baþlatýr
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Btn_MainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false; // Ana menüde müziklerin çalýþmasý için kilidi açar
        SceneManager.LoadScene("GameMainMenu");
    }
}