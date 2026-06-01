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
        // KENDÝNÝ VURMAYI ÖNLER: Çarpýlan obje tetikleyiciyse, Player ise veya Player'ýn elindeki bir silahsa yoksay!
        if (other.isTrigger || other.CompareTag("Player") || other.CompareTag("Bullet") || other.transform.root.CompareTag("Player"))
            return;

        Vector3 hitPoint = transform.position;
        ObjectPooler.Instance.SpawnFromPool("BloodFX", hitPoint, Quaternion.identity);

        gameObject.SetActive(false);
    }
}