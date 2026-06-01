using UnityEngine;

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
                // OnCollect artýk true/false dönüyor. Sadece true dönerse kutu yok olur.
                bool isCollected = OnCollect();
                if (isCollected)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    // Toplama iþlemi baþarýlýysa true dönmeli
    protected abstract bool OnCollect();
}

public class HealthChest : LootItem
{
    protected override bool OnCollect()
    {
        // CAN KONTROLÜ: Can zaten full ise toplama iþlemini iptal et (false dön)
        if (player.currentHealth >= player.maxHealth)
        {
            return false;
        }

        player.currentHealth += 25f;
        if (player.currentHealth > player.maxHealth) player.currentHealth = player.maxHealth;

        player.uiManager.UpdatePlayerBars(player.currentHealth, player.maxHealth, player.currentStamina, player.maxStamina);

        return true; // Baþarýyla toplandý, kutuyu yok et
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

        return true; // Mermi her zaman toplanabilir, kutuyu yok et
    }
}

public enum LootType { Health, Ammo }

// --- 2. ANA YÖNETÝCÝ VE FABRÝKA METODU (LootSystem) ---
public class LootSystem : MonoBehaviour
{
    [Header("Loot Prefablarý")]
    public GameObject healthChestPrefab;
    public GameObject ammoChestPrefab;

    private ISpawnStrategy spawnStrategy;
    private bool hasSpawnedTonight = false;

    void Start()
    {
        spawnStrategy = new DesertSpawnStrategy();
    }

    void Update()
    {
        float time = GameManager.Instance.currentTimeInMinutes % 1440f;

        if (time >= 1141f || time < 359f)
        {
            if (!hasSpawnedTonight)
            {
                SpawnLootWave();
                hasSpawnedTonight = true;
            }
        }
        else
        {
            if (time > 360f && time < 1000f) hasSpawnedTonight = false;
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
        }

        int ammoAmount = Random.Range(8, 17);
        for (int i = 0; i < ammoAmount; i++)
        {
            GameObject ammoChest = CreateLoot(LootType.Ammo, spawnStrategy.GetSpawnPosition());
            if (ammoChest.GetComponent<AmmoChest>() == null) ammoChest.AddComponent<AmmoChest>();
        }
    }
}