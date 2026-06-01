using UnityEngine;

public class CampfireManager : MonoBehaviour
{
    [Header("Tüm Kamp Ateþi Objeleri")]
    public GameObject[] campfires; // Campfire_Pot_1, Campfire_2 vs. buraya atýlacak

    private bool areFiresActive = false;

    void Update()
    {
        // GameManager'daki zamaný okuyoruz
        float time = GameManager.Instance.currentTimeInMinutes % 1440f;

        // 06:00 (360) - 09:00 (540) ARASI VEYA 18:00 (1080) - 05:59 (359) ARASI
        bool shouldBeActive = (time >= 360f && time <= 540f) || (time >= 1080f || time < 360f);

        if (shouldBeActive != areFiresActive)
        {
            areFiresActive = shouldBeActive;
            ToggleCampfires(areFiresActive);
        }
    }

    private void ToggleCampfires(bool state)
    {
        foreach (GameObject fire in campfires)
        {
            if (fire != null)
            {
                // Objenin altýndaki child'larý (FX ve Iþýk) bulup kapat/aç
                Transform fx = fire.transform.Find("FX_Fire_01");
                Transform light = fire.transform.Find("Camp_Fire_Point_Light");

                if (fx != null) fx.gameObject.SetActive(state);
                if (light != null) light.gameObject.SetActive(state);
            }
        }
    }
}