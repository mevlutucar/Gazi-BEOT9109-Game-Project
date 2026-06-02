using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 50f;
    private float lifeTimer;

    void OnEnable()
    {
        lifeTimer = 2f;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0) gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || other.CompareTag("Player") || other.CompareTag("Bullet") || other.transform.root.CompareTag("Player"))
            return;

        // SADECE "Enemy" tagine sahip olan nesnelere çarpýldýðýnda kan çýkar!
        if (other.CompareTag("Enemy"))
        {
            Vector3 hitPoint = transform.position;
            ObjectPooler.Instance.SpawnFromPool("BloodFX", hitPoint, Quaternion.identity);
        }

        gameObject.SetActive(false);
    }
}