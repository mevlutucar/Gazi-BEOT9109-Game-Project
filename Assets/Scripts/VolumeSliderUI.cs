using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderUI : MonoBehaviour
{
    public enum AudioType { MenuMusic, GameMusic, SFX }

    [Header("Bu Slider Neyi Kontrol Ediyor?")]
    public AudioType audioType;

    [Header("UI Bileþenleri")]
    public Slider volumeSlider;
    public GameObject audioOnIcon;
    public GameObject audioOffIcon;

    private string volKey;
    private string savedVolKey;

    void Start()
    {
        // PlayerPrefs için anahtar kelimeler
        volKey = audioType.ToString() + "Vol";
        savedVolKey = audioType.ToString() + "SavedVol";

        // Baþlangýç deðerini çek (Eðer daha önce kaydedilmediyse 1f yani Full yap)
        float currentVol = PlayerPrefs.GetFloat(volKey, 1f);

        if (volumeSlider != null)
        {
            volumeSlider.value = currentVol;
            // Slider deðeri her deðiþtiðinde OnSliderValueChanged fonksiyonunu otomatik tetikle
            volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        UpdateIcons(currentVol);
    }

    // Slider sürüklendikçe anlýk olarak çalýþýr
    public void OnSliderValueChanged(float value)
    {
        PlayerPrefs.SetFloat(volKey, value);

        // Kullanýcý sesi tamamen sýfýrlamadýysa, son ses seviyesini aklýmýzda tutalým
        if (value > 0f)
        {
            PlayerPrefs.SetFloat(savedVolKey, value);
        }

        UpdateIcons(value);
    }

    // Bu fonksiyonu ikonlarýn Button OnClick() eventine baðlayacaðýz
    public void ToggleMute()
    {
        if (volumeSlider == null) return;

        float currentVol = volumeSlider.value;

        if (currentVol > 0f)
        {
            // SESÝ KAPAT: Mevcut deðeri kaydet ve Slider'ý sýfýrla
            PlayerPrefs.SetFloat(savedVolKey, currentVol);
            volumeSlider.value = 0f;
        }
        else
        {
            // SESÝ AÇ: Kaydedilen eski deðeri geri yükle
            float savedVol = PlayerPrefs.GetFloat(savedVolKey, 1f);
            if (savedVol <= 0.01f) savedVol = 1f; // Eðer hafýzadaki de 0 ise direkt 1'e (Full) çek

            volumeSlider.value = savedVol;
        }
    }

    private void UpdateIcons(float value)
    {
        bool isMuted = (value <= 0f);

        if (audioOnIcon != null) audioOnIcon.SetActive(!isMuted);
        if (audioOffIcon != null) audioOffIcon.SetActive(isMuted);
    }
}