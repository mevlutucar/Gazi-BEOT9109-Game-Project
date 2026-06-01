using UnityEngine;
using System.Collections.Generic; // Liste hafýzasý için eklendi

// --- 1. STRATEGY PATTERN (Konumlandýrma Stratejisi) ---
public interface ISpawnStrategy
{
    Vector3 GetSpawnPosition();
}

public class DesertSpawnStrategy : ISpawnStrategy
{
    public Vector3 GetSpawnPosition()
    {
        float randomX = Random.Range(-100f, 100f);
        float randomZ = Random.Range(-65f, 85f);

        Vector3 rayStart = new Vector3(randomX, 100f, randomZ);
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 200f))
        {
            return new Vector3(randomX, hit.point.y + 1f, randomZ);
        }

        return new Vector3(randomX, 1f, randomZ);
    }
}

// --- KUTU DAVRANIÞLARI (Toplanma ve Dönme) ---
public abstract class LootItem : MonoBehaviour
{
    protected PlayerController player;

    void Update()
    {
        transform.Rotate(0f, 45f * Time.deltaTime, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                bool isCollected = OnCollect();
                if (isCollected)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    protected abstract bool OnCollect();
}

public class HealthChest : LootItem
{
    protected override bool OnCollect()
    {
        if (player.currentHealth >= player.maxHealth) return false; // Can fullse alma

        player.currentHealth += 25f;
        if (player.currentHealth > player.maxHealth) player.currentHealth = player.maxHealth;

        player.uiManager.UpdatePlayerBars(player.currentHealth, player.maxHealth, player.currentStamina, player.maxStamina);

        if (LootSystem.Instance.healthCollectSound != null)
        {
            player.PlaySound(LootSystem.Instance.healthCollectSound, 1f);
        }

        return true;
    }
}

public class AmmoChest : LootItem
{
    protected override bool OnCollect()
    {
        int currentAmmo;
        int.TryParse(player.uiManager.rifleBulletTxt.text, out currentAmmo);
        currentAmmo += 8;

        player.ammoCount = currentAmmo;
        player.uiManager.UpdateAmmoText(currentAmmo);

        if (LootSystem.Instance.ammoCollectSound != null)
        {
            player.PlaySound(LootSystem.Instance.ammoCollectSound, 1f);
        }

        return true;
    }
}

public enum LootType { Health, Ammo }

// --- 2. ANA YÖNETÝCÝ VE FABRÝKA METODU (LootSystem) ---
public class LootSystem : MonoBehaviour
{
    public static LootSystem Instance;

    [Header("Loot Prefablarý")]
    public GameObject healthChestPrefab;
    public GameObject ammoChestPrefab;

    [Header("Loot Toplama Sesleri (SFX)")]
    public AudioClip healthCollectSound;
    public AudioClip ammoCollectSound;

    private ISpawnStrategy spawnStrategy;
    private bool hasSpawnedTonight = false;

    // Gündüzleri silmek için kutularý tuttuðumuz hafýza listesi
    private List<GameObject> activeLoots = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        spawnStrategy = new DesertSpawnStrategy();
    }

    void Update()
    {
        float time = GameManager.Instance.currentTimeInMinutes % 1440f;

        // Saat 19:01 (1141) ile 05:59 (359) arasý Gece Vakti: Kutularý oluþtur
        if (time >= 1141f || time < 360f)
        {
            if (!hasSpawnedTonight)
            {
                SpawnLootWave();
                hasSpawnedTonight = true;
            }
        }
        else // Saat 06:00 itibariyle Gündüz Vakti: Kalan kutularý temizle
        {
            if (hasSpawnedTonight)
            {
                ClearLootWave();
                hasSpawnedTonight = false;
            }
        }
    }

    private GameObject CreateLoot(LootType type, Vector3 position)
    {
        GameObject prefab = (type == LootType.Health) ? healthChestPrefab : ammoChestPrefab;
        return Instantiate(prefab, position, Quaternion.identity);
    }

    private void SpawnLootWave()
    {
        int healthAmount = Random.Range(8, 17);
        for (int i = 0; i < healthAmount; i++)
        {
            GameObject healthChest = CreateLoot(LootType.Health, spawnStrategy.GetSpawnPosition());
            if (healthChest.GetComponent<HealthChest>() == null) healthChest.AddComponent<HealthChest>();

            activeLoots.Add(healthChest); // Üretilen kutuyu hafýzaya kaydet
        }

        int ammoAmount = Random.Range(8, 17);
        for (int i = 0; i < ammoAmount; i++)
        {
            GameObject ammoChest = CreateLoot(LootType.Ammo, spawnStrategy.GetSpawnPosition());
            if (ammoChest.GetComponent<AmmoChest>() == null) ammoChest.AddComponent<AmmoChest>();

            activeLoots.Add(ammoChest); // Üretilen kutuyu hafýzaya kaydet
        }
    }

    // Haritada toplanmamýþ ne kadar kutu varsa döngüyle temizler
    private void ClearLootWave()
    {
        foreach (GameObject loot in activeLoots)
        {
            if (loot != null) Destroy(loot);
        }
        activeLoots.Clear(); // Listeyi ertesi gece için boþalt
    }
}