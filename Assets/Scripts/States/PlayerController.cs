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

    [Header("Weapon References")]
    public GameObject rifleObject; // Inspector'dan SM_Wep_Rifle_01 buraya atanacak

    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxStamina = 100f;
    public float currentStamina;
    public int ammoCount = 30;

    [Header("Weapon Settings")]
    public float fireRate = 0.2f; // Ateþ etme bekleme süresi (Saniye)
    internal float nextFireTime = 0f; // Bir sonraki ateþ edebilme zamaný

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
        uiManager.UpdateUI(this);
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

    // MERKEZÝ ATEÞ ETME SÝSTEMÝ (BEKLEME SÜRELÝ)
    public void FireWeapon()
    {
        // Þu anki zaman, bir sonraki ateþ edebilme zamanýna eþit veya büyükse ateþ et
        if (Time.time >= nextFireTime)
        {
            if (ammoCount > 0)
            {
                anim.SetTrigger("Fire");
                PlaySound(shootSound);
                ammoCount--;
                uiManager.UpdateUI(this);
            }
            else
            {
                PlaySound(emptyMagSound);
            }

            // Mermi olsa da olmasa da tetiðin bekleme süresini (0.2 sn) ekliyoruz.
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
            uiManager.UpdateUI(this);
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
            uiManager.UpdateUI(this);
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentState == deadState) return;

        currentHealth -= damage;
        uiManager.UpdateUI(this);

        if (currentHealth <= 0)
        {
            PlaySound(deathSound);
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
            uiManager.UpdateUI(this);
        }
    }
}