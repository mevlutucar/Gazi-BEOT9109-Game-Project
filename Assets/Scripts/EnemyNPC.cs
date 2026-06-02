using UnityEngine;

public class EnemyNPC : MonoBehaviour
{
    [Header("Enemy Type")]
    public bool isChief = false;

    [Header("Stats & Audio")]
    public int health = 100;
    public float hideDelayAfterDeath = 4f;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    private Animator anim;
    private AudioSource audioSource;
    private bool isDead = false;

    private Vector3 startPos;
    private Quaternion startRot;

    void Awake()
    {
        // GARANTÝLÝ ÇÖZÜM: Ýçi boþ sahte Animator'leri es geçer
        Animator[] anims = GetComponentsInChildren<Animator>();
        foreach (Animator a in anims)
        {
            if (a.runtimeAnimatorController != null)
            {
                anim = a;
                break;
            }
        }
        if (anim == null) anim = GetComponentInChildren<Animator>();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;

        startPos = transform.position;
        startRot = transform.rotation;
    }

    void OnEnable()
    {
        health = 100;
        isDead = false;
        transform.position = startPos;
        transform.rotation = startRot;
        CancelInvoke("HideEnemy");

        if (anim != null && anim.runtimeAnimatorController != null && gameObject.activeInHierarchy)
        {
            if (isChief) anim.Play("Looking", 0, 0f);
            else anim.Play("Breathing Idle NPC", 0, 0f);

            anim.Update(0f);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        if (anim != null) anim.SetTrigger("Hit");
        PlaySound(hurtSound);

        if (health <= 0)
        {
            isDead = true;
            if (anim != null) anim.SetTrigger("Die");
            PlaySound(deathSound);

            Invoke("HideEnemy", hideDelayAfterDeath);
        }
    }

    private void HideEnemy() { gameObject.SetActive(false); }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.volume = PlayerPrefs.GetFloat("SFXVol", 1f);
            audioSource.PlayOneShot(clip);
        }
    }
}