using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Player Bars")]
    public Slider healthBar;     // Kýrmýzý
    public Slider staminaBar;    // Kýrýk Beyaz
    public TextMeshProUGUI ammoText;
    public Image weaponIcon;     // Yumruk veya Silah görseli
    public Sprite fistSprite, rifleSprite;

    [Header("Day/Night UI")]
    public Slider dayNightBar;   // Gündüz-Gece ilerlemesi
    public TextMeshProUGUI dayNightText;

    [Header("Panels")]
    public GameObject crosshairPanel;
    public GameObject deathPanel;
    public GameObject pausePanel;
    public TextMeshProUGUI daysSurvivedText;
    public TextMeshProUGUI pauseFlashingText;

    public void UpdateUI(PlayerController player)
    {
        healthBar.value = player.currentHealth / player.maxHealth;
        staminaBar.value = player.currentStamina / player.maxStamina;
        ammoText.text = player.ammoCount.ToString();
    }

    public void SetWeaponIcon(bool hasWeapon)
    {
        weaponIcon.sprite = hasWeapon ? rifleSprite : fistSprite;
    }

    public void ToggleCrosshair(bool state)
    {
        crosshairPanel.SetActive(state);
    }

    public void ShowDeathPanel()
    {
        deathPanel.SetActive(true);
        daysSurvivedText.text = "Hayatta kalýnan gün sayýsý: " + GameManager.Instance.daysSurvived;
    }

    public void ShowPausePanel(bool state)
    {
        pausePanel.SetActive(state);
        if (state) StartCoroutine(FlashPauseText());
        else StopAllCoroutines();
    }

    private IEnumerator FlashPauseText()
    {
        while (true)
        {
            pauseFlashingText.enabled = !pauseFlashingText.enabled;
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    // Buton Fonksiyonlarý
    public void Btn_Resume()
    {
        GameManager.Instance.TogglePause();
    }

    public void Btn_Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Btn_MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}