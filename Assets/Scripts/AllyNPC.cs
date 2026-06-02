using UnityEngine;
using System.Collections;

public class AllyNPC : MonoBehaviour
{
    public enum AllyType { Assistant, Talker, Looker, Wheel }

    [Header("NPC Settings")]
    public AllyType npcType;
    public bool canInteract = false;
    public float hideDelayAfterConversation = 5f;

    [Header("Easter Egg Settings (Wheel NPC)")]
    public int health = 25;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    private bool isVulnerable = false; // Sadece ses bitince True olacak
    private bool isDead = false;

    [Header("Audio")]
    public AudioClip interactSound;
    public AudioClip walkingSound;
    private AudioSource audioSource;
    private Animator anim;

    private Vector3 posA = new Vector3(20.266f, 5.4f, 74.97f);
    private Vector3 posB = new Vector3(34.76f, 5.4f, 74.97f);
    private Vector3 posC = new Vector3(27.85f, 5.4f, 74.97f);
    private bool movingToB = true;

    private Vector3 startPos;
    private Quaternion startRot;

    void Awake()
    {
        // Ýçi boþ sahte Animator'leri es geçer, Controller'ý olan GERÇEK Animator'ü bulur!
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
        GameEvents.OnConversationEnded += HideAlly;

        transform.position = startPos;
        transform.rotation = startRot;
        movingToB = true;

        // Yeni Günde Easter Egg Sýfýrlama
        health = 25;
        isVulnerable = false;
        isDead = false;
        if (npcType == AllyType.Talker || npcType == AllyType.Wheel) canInteract = true;

        CancelInvoke("DeactivateRoutine");

        SetInitialAnimation();
    }

    void Start()
    {
        SetInitialAnimation();
    }

    private void SetInitialAnimation()
    {
        if (anim == null || anim.runtimeAnimatorController == null || !gameObject.activeInHierarchy) return;

        if (npcType != AllyType.Assistant)
            anim.SetFloat("Speed", 0f);

        if (npcType == AllyType.Talker)
        {
            anim.Play("Talking", 0, 0f);
        }
        else if (npcType == AllyType.Looker || npcType == AllyType.Wheel)
        {
            anim.Play("Looking", 0, 0f);
        }
        else if (npcType == AllyType.Assistant)
        {
                anim.SetFloat("Speed", 0.3f);
                anim.Play("Walking NPC", 0, 0f);
        }
    }

    void OnDisable()
    {
        GameEvents.OnConversationEnded -= HideAlly;
    }

    private void HideAlly()
    {
        Invoke("DeactivateRoutine", hideDelayAfterConversation);
    }

    private void DeactivateRoutine()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (npcType == AllyType.Assistant) HandleAssistantLogic();
    }

    private void HandleAssistantLogic()
    {
        float time = GameManager.Instance.currentTimeInMinutes % 1440f;

        if (time >= 360f && time <= 1080f)
        {
            anim.SetFloat("Speed", 0.3f);
            if (!audioSource.isPlaying && walkingSound != null)
            {
                audioSource.clip = walkingSound;
                audioSource.volume = PlayerPrefs.GetFloat("SFXVol", 1f);
                audioSource.Play();
            }

            Vector3 target = movingToB ? posB : posA;
            transform.position = Vector3.MoveTowards(transform.position, target, 2f * Time.deltaTime);
            Quaternion targetRot = movingToB ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.1f) movingToB = !movingToB;
        }
        else if (time > 1080f && time < 1139f)
        {
            if (Vector3.Distance(transform.position, posC) > 0.1f)
            {
                anim.SetFloat("Speed", 1f);
                transform.position = Vector3.MoveTowards(transform.position, posC, 2f * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 180, 0), 5f * Time.deltaTime);
            }
            else
            {
                anim.SetFloat("Speed", 0f);
                anim.Play("Looking");
                audioSource.Stop();
            }
        }
    }

    public void Interact(PlayerController player)
    {
        if (audioSource.isPlaying || isDead) return; // Öldüyse konuþamaz

        audioSource.volume = PlayerPrefs.GetFloat("SFXVol", 1f);
        audioSource.PlayOneShot(interactSound);

        if (npcType == AllyType.Wheel) StartCoroutine(WheelRoutine());
    }

    private IEnumerator WheelRoutine()
    {
        if (anim != null) anim.Play("Talking", 0, 0f);

        yield return new WaitForSeconds(interactSound.length); // Sesin bitmesini bekle

        if (anim != null) anim.Play("Looking", 0, 0f);

        // EASTER EGG: Ses bitti, artýk vurulabilir!
        isVulnerable = true;
    }

    // EASTER EGG HASAR SÝSTEMÝ
    public void TakeDamage(int damage)
    {
        // Karakter Wheel deðilse, konuþma bitmediyse veya zaten öldüyse hasar almaz!
        if (!isVulnerable || isDead) return;

        health -= damage;
        if (anim != null) anim.SetTrigger("Hit");

        if (hurtSound != null)
        {
            audioSource.volume = PlayerPrefs.GetFloat("SFXVol", 1f);
            audioSource.PlayOneShot(hurtSound);
        }

        if (health <= 0)
        {
            isDead = true;
            canInteract = false;
            if (anim != null) anim.SetTrigger("Die");

            if (deathSound != null)
            {
                audioSource.volume = PlayerPrefs.GetFloat("SFXVol", 1f);
                audioSource.PlayOneShot(deathSound);
            }

            Invoke("DeactivateRoutine", 4f); // Öldükten 4 saniye sonra yok ol
        }
    }
}