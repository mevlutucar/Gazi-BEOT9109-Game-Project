using UnityEngine;
using System.Collections;

public interface IPlayerState
{
    void EnterState(PlayerController player);
    void UpdateState(PlayerController player);
    void ExitState(PlayerController player);
}

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public Animator anim;
    public CharacterController controller;
    public CameraController cameraController;
    public UIManager uiManager;

    [Header("Weapon Models (3D)")]
    public GameObject rifleModel;

    [Header("Shooting & FX")]
    public Transform muzzlePoint;
    public ParticleSystem muzzleFlashFX;

    [Header("Weapon UI Icons")]
    public GameObject punchIconUI;
    public GameObject rifleIconUI;

    [Header("Bone Alignment (LateUpdate)")]
    public Transform rootBone;
    [Range(1f, 50f)] public float boneTransitionSpeed = 20f;

    private Vector3 defaultRiflePos;
    private Quaternion defaultRifleRot;
    private float rifleIKWeight = 0f;
    private float rootIKWeight = 0f;

    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxStamina = 100f;
    public float currentStamina;
    public int ammoCount = 30;

    [Header("Weapon Settings")]
    public float fireRate = 0.2f;
    internal float nextFireTime = 0f;

    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    public float turnSpeed = 150f;

    [Header("Camera FOV Settings")]
    public float normalFOV = 60f; // Eskiden 75'ti, artýk daha doðal bir açýda
    public float runFOV = 50f;    // Koþarken hafif yakýnlaþýr

    [Header("Audio Clips")]
    public AudioClip walkSound, runSound, jumpSound, shootSound, emptyMagSound;
    public AudioClip hitSound, deathSound;

    [Header("Audio Volumes")]
    [Range(0f, 1f)] public float walkVolume = 1f;
    [Range(0f, 1f)] public float runVolume = 1f;
    [Range(0f, 1f)] public float jumpVolume = 1f;
    [Range(0f, 1f)] public float shootVolume = 1f;
    [Range(0f, 1f)] public float emptyMagVolume = 1f;
    [Range(0f, 1f)] public float hitVolume = 1f;
    [Range(0f, 1f)] public float deathVolume = 1f;

    [Header("Death Settings")]
    public float deathDelay = 2.5f;

    private AudioSource movementAudioSource;
    private AudioSource actionAudioSource;

    internal Vector3 velocity;
    internal bool isGrounded;
    internal IPlayerState currentState;

    public UnarmedState unarmedState = new UnarmedState();
    public ArmedState armedState = new ArmedState();
    public AimingState aimingState = new AimingState();
    public DeadState deadState = new DeadState();

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        movementAudioSource = gameObject.AddComponent<AudioSource>();
        movementAudioSource.loop = true;

        actionAudioSource = gameObject.AddComponent<AudioSource>();
        actionAudioSource.loop = false;

        currentHealth = 90;
        currentStamina = maxStamina;

        if (rifleModel != null)
        {
            defaultRiflePos = rifleModel.transform.localPosition;
            defaultRifleRot = rifleModel.transform.localRotation;
        }

        TransitionToState(unarmedState);
        uiManager.UpdatePlayerBars(currentHealth, maxHealth, currentStamina, maxStamina);
    }

    void Update()
    {
        if (GameManager.Instance.isPaused || currentState == deadState) return;

        isGrounded = controller.isGrounded;
        anim.SetBool("IsGrounded", isGrounded);

        if (isGrounded && actionAudioSource.isPlaying && actionAudioSource.clip == jumpSound)
        {
            actionAudioSource.Stop();
        }

        currentState.UpdateState(this);
        ApplyGravity();
        RecoverStamina();
    }

    void LateUpdate()
    {
        if (GameManager.Instance.isPaused || currentState == deadState || rifleModel == null || rootBone == null) return;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(1);

        bool applyRifleOffset = stateInfo.IsName("Rifle Run") ||
                                stateInfo.IsName("Firing Rifle") ||
                                stateInfo.IsName("Rifle Aiming Idle");

        bool applyRootRotation = stateInfo.IsName("Firing Rifle") ||
                                 stateInfo.IsName("Rifle Aiming Idle");

        float targetRifleWeight = applyRifleOffset ? 1f : 0f;
        float targetRootWeight = applyRootRotation ? 1f : 0f;

        rifleIKWeight = Mathf.MoveTowards(rifleIKWeight, targetRifleWeight, boneTransitionSpeed * Time.deltaTime);
        rootIKWeight = Mathf.MoveTowards(rootIKWeight, targetRootWeight, boneTransitionSpeed * Time.deltaTime);

        Vector3 customRiflePos = new Vector3(-7f, -4.1f, 5.9f);
        Quaternion customRifleRot = Quaternion.Euler(-13.81f, -121.439f, -29.97f);

        rifleModel.transform.localPosition = Vector3.Lerp(defaultRiflePos, customRiflePos, rifleIKWeight);
        rifleModel.transform.localRotation = Quaternion.Slerp(defaultRifleRot, customRifleRot, rifleIKWeight);

        if (rootIKWeight > 0.01f)
        {
            Quaternion customRootRot = Quaternion.Euler(0f, 45f, 0f);
            rootBone.localRotation = Quaternion.Slerp(rootBone.localRotation, customRootRot, rootIKWeight);
        }
    }

    public void TransitionToState(IPlayerState newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }

    private void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void FireWeapon()
    {
        int.TryParse(uiManager.rifleBulletTxt.text, out ammoCount);

        if (Time.time >= nextFireTime)
        {
            if (ammoCount > 0 && currentStamina >= 10f)
            {
                nextFireTime = Time.time + fireRate;
                StartCoroutine(FireSequence());
            }
            else
            {
                PlaySound(emptyMagSound, emptyMagVolume);
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private IEnumerator FireSequence()
    {
        anim.SetTrigger("Fire");
        ammoCount--;
        currentStamina -= 10f;

        uiManager.UpdateAmmoText(ammoCount);
        uiManager.UpdatePlayerBars(currentHealth, maxHealth, currentStamina, maxStamina);

        rifleIKWeight = 1f;
        rootIKWeight = 1f;

        yield return new WaitForEndOfFrame();

        PlaySound(shootSound, shootVolume);

        if (muzzleFlashFX != null) muzzleFlashFX.Play();

        Ray ray = cameraController.playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint = ray.GetPoint(100f);

        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.CompareTag("Player") && !hit.collider.transform.root.CompareTag("Player"))
            {
                targetPoint = hit.point;
                break;
            }
        }

        if (muzzlePoint != null)
        {
            Vector3 directionWithoutSpread = targetPoint - muzzlePoint.position;
            ObjectPooler.Instance.SpawnFromPool("Bullet", muzzlePoint.position, Quaternion.LookRotation(directionWithoutSpread));
        }
    }

    public void PlaySound(AudioClip clip, float localVolume)
    {
        if (clip != null)
        {
            float globalSfxVol = PlayerPrefs.GetFloat("SFXVol", 1f);

            actionAudioSource.clip = clip;
            actionAudioSource.volume = localVolume * globalSfxVol;
            actionAudioSource.Play();
        }
    }

    private void HandleMovementAudio(AudioClip clip, float localVolume)
    {
        if (clip != null)
        {
            float globalSfxVol = PlayerPrefs.GetFloat("SFXVol", 1f);

            if (movementAudioSource.clip != clip)
            {
                movementAudioSource.clip = clip;
                movementAudioSource.Play();
            }
            else if (!movementAudioSource.isPlaying)
            {
                movementAudioSource.Play();
            }

            movementAudioSource.volume = localVolume * globalSfxVol;
        }
    }

    private void StopMovementAudio()
    {
        if (movementAudioSource.isPlaying)
        {
            movementAudioSource.Stop();
        }
    }

    public void HandleMovementAndRotation()
    {
        // 1. DÖNÜÞ: A/D tuþlarý yengeç yürüyüþünden çýkarýldý. Artýk farenin saða/sola hareketiyle BÝRLEÞEREK karakteri döndürüyor.
        float mouseX = Input.GetAxis("Mouse X") * cameraController.mouseSensitivity * Time.deltaTime;
        float keyTurn = Input.GetAxisRaw("Horizontal") * turnSpeed * Time.deltaTime;

        transform.Rotate(0f, mouseX + keyTurn, 0f);

        // 2. ÝLERÝ/GERÝ HAREKET: W ve S tuþlarý
        float moveInput = Input.GetAxisRaw("Vertical");

        // Yalnýzca ileri veya geri gidiliyorsa yürüme animasyonunu tetikle
        bool isMoving = moveInput != 0;
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && isMoving && moveInput > 0 && currentStamina > 0;
        float currentSpeed = isRunning ? runSpeed : (isMoving ? walkSpeed : 0f);

        if (isMoving)
        {
            Vector3 moveDirection = transform.forward * moveInput;
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        anim.SetFloat("Speed", isMoving ? currentSpeed : 0f);

        // Yeni FOV sistemi devreye girdi (Inspector'dan ayarlanabilir normalFOV ve runFOV deðerleri)
        if (currentState != aimingState)
        {
            cameraController.SetAimTarget(false, isRunning ? runFOV : normalFOV);
        }

        if (isRunning)
        {
            HandleMovementAudio(runSound, runVolume);
            currentStamina -= Time.deltaTime * 10f;
            uiManager.UpdatePlayerBars(currentHealth, maxHealth, currentStamina, maxStamina);
        }
        else if (isMoving)
        {
            HandleMovementAudio(walkSound, walkVolume);
        }
        else
        {
            StopMovementAudio();
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && currentStamina >= 15f)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            anim.SetTrigger("Jump");
            PlaySound(jumpSound, jumpVolume);
            currentStamina -= 15f;
            uiManager.UpdatePlayerBars(currentHealth, maxHealth, currentStamina, maxStamina);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            TakeDamage(10f);
        }
        else if (other.CompareTag("Axe"))
        {
            TakeDamage(20f);
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentState == deadState) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);
        uiManager.UpdatePlayerBars(currentHealth, maxHealth, currentStamina, maxStamina);

        if (currentHealth <= 0f)
        {
            TransitionToState(deadState);
        }
        else
        {
            anim.SetTrigger("Hit");
            PlaySound(hitSound, hitVolume);
        }
    }

    public void TriggerDeathSequence()
    {
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        StopMovementAudio();
        anim.SetTrigger("Die");
        PlaySound(deathSound, deathVolume);

        controller.enabled = false;

        yield return new WaitForSeconds(deathDelay);

        Time.timeScale = 0f;
        uiManager.ShowDeathPanel();
    }

    private void RecoverStamina()
    {
        if (currentStamina < maxStamina && currentState != deadState && !Input.GetKey(KeyCode.LeftShift))
        {
            currentStamina += Time.deltaTime * 30f;
            uiManager.UpdatePlayerBars(currentHealth, maxHealth, currentStamina, maxStamina);
        }
    }
}