using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 50f;
    private float lifeTimer;

    void OnEnable()
    {
        lifeTimer = 2f; // 2 saniye sonra vuramazsa havuza döner
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0) gameObject.SetActive(false); // Havuza geri yolla
    }

    void OnTriggerEnter(Collider other)
    {
        // Kendi kendine veya loot kutularýna çarpmamasý için kontroller
        if (other.CompareTag("Player") || other.CompareTag("Bullet") || other.isTrigger) return;

        // Sarý uyarý hatasýný çözen kýsým: ClosestPoint yerine merminin mevcut konumunu kullanýyoruz
        Vector3 hitPoint = transform.position;

        // Kan veya Toz efektini havuzdan çaðýr (Inspector'da Tag'in "BloodFX" olduðuna emin ol)
        ObjectPooler.Instance.SpawnFromPool("BloodFX", hitPoint, Quaternion.identity);

        gameObject.SetActive(false); // Mermiyi havuza yolla
    }
}