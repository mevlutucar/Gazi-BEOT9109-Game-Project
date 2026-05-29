using UnityEngine;

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
    public GameObject rifleModel; // Inspector'dan SM_Wep_Rifle_01 objesini buraya ata

    [Header("Weapon UI Icons")]
    public GameObject punchIconUI; // UI Canvas'taki Punch objesi
    public GameObject rifleIconUI; // UI Canvas'taki Rifle objesi

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

    [Header("Audio Clips")]
    public AudioClip walkSound, runSound, jumpSound, shootSound, emptyMagSound;
    public AudioClip hitSound, deathSound;

    private AudioSource movementAudioSource;
    private AudioSource actionAudioSource;

    internal Vector3 velocity;
    internal bool isGrounded;
    internal IPlayerState currentState;

    // States
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

        currentHealth = maxHealth;
        currentStamina = maxStamina;

        TransitionToState(unarmedState);
        uiManager.UpdatePlayerBars(currentHealth, maxHealth, currentStamina, maxStamina);
    }

    void Update()
    {
        if (GameManager.Instance.isPaused) return;

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
            // Ateþ etmek için hem mermi hem de en az 10 Stamina gereklidir.
            if (ammoCount > 0 && currentStamina >= 10f)
            {
                anim.SetTrigger("Fire");
                PlaySound(shootSound);
                ammoCount--;
                currentStamina -= 10f; // Ateþ edildiðinde stamina azalýr

                uiManager.UpdateAmmoText(ammoCount);
                uiManager.UpdatePlayerBars(currentHealth, maxHealth, currentStamina, maxStamina);
            }
            else
            {
                PlaySound(emptyMagSound); // Mermi bittiðinde veya stamina yetmediðinde boþ tetik sesi
            }

            nextFireTime = Time.time + fireRate;
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            actionAudioSource.clip = clip;
            actionAudioSource.Play();
        }
    }

    private void HandleMovementAudio(AudioClip clip)
    {
        if (clip != null)
        {
            if (movementAudioSource.clip != clip)
            {
                movementAudioSource.clip = clip;
                movementAudioSource.Play();
            }
            else if (!movementAudioSource.isPlaying)
            {
                movementAudioSource.Play();
            }
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
        float turnInput = Input.GetAxisRaw("Horizontal");
        float moveInput = Input.GetAxisRaw("Vertical");

        if (turnInput != 0)
        {
            float rotationAmount = turnInput * turnSpeed * Time.deltaTime;
            transform.Rotate(0f, rotationAmount, 0f);
        }

        bool isMoving = moveInput != 0;
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && isMoving && moveInput > 0 && currentStamina > 0;
        float currentSpeed = isRunning ? runSpeed : (isMoving ? walkSpeed : 0f);

        if (isMoving)
        {
            Vector3 moveDirection = transform.forward * moveInput;
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        anim.SetFloat("Speed", isMoving ? currentSpeed : 0f);
        cameraController.SetFOV(isRunning ? 50f : 65f);

        if (isRunning)
        {
            HandleMovementAudio(runSound);
            currentStamina -= Time.deltaTime * 10f;
            uiManager.UpdatePlayerBars(currentHealth, maxHealth, currentStamina, maxStamina);
        }
        else if (isMoving)
        {
            HandleMovementAudio(walkSound);
        }
        else
        {
            StopMovementAudio();
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && currentStamina >= 15f)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            anim.SetTrigger("Jump");
            PlaySound(jumpSound);
            currentStamina -= 15f;
            uiManager.UpdatePlayerBars(currentHealth, maxHealth, currentStamina, maxStamina);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            TakeDamage(10f);
        }
        else if (collision.gameObject.CompareTag("Axe"))
        {
            TakeDamage(20f);
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentState == deadState) return;

        currentHealth -= damage;
        // Can 0'ýn altýna düþmesin diye sýnýrlandýrýyoruz
        currentHealth = Mathf.Max(currentHealth, 0f);
        uiManager.UpdatePlayerBars(currentHealth, maxHealth, currentStamina, maxStamina);

        if (currentHealth <= 0f)
        {
            PlaySound(deathSound);
            Time.timeScale = 0f;
            TransitionToState(deadState);
        }
        else
        {
            anim.SetTrigger("Hit");
            PlaySound(hitSound);
        }
    }

    private void RecoverStamina()
    {
        if (currentStamina < maxStamina && currentState != deadState && !Input.GetKey(KeyCode.LeftShift))
        {
            currentStamina += Time.deltaTime * 5f;
            uiManager.UpdatePlayerBars(currentHealth, maxHealth, currentStamina, maxStamina);
        }
    }
}