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

    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    // Eski turnSmoothSpeed yerine, saniye baþýna kaç derece döneceðini belirten turnSpeed ekledik.
    public float turnSpeed = 150f;

    [Header("Audio Clips")]
    public AudioClip walkSound, runSound, jumpSound, shootSound, emptyMagSound;
    private AudioSource audioSource;

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
        audioSource = gameObject.AddComponent<AudioSource>();

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

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && !audioSource.isPlaying)
            audioSource.PlayOneShot(clip);
    }

    // GÜNCELLENMÝÞ HAREKET VE ROTASYON SÝSTEMÝ
    public void HandleMovementAndRotation()
    {
        // Girdileri alýyoruz
        float turnInput = Input.GetAxisRaw("Horizontal"); // A, D, Sol, Sað
        float moveInput = Input.GetAxisRaw("Vertical");   // W, S, Yukarý, Aþaðý

        // 1. Sadece Kendi Etrafýnda Dönme (Rotasyon) - Yürümeden baðýmsýz
        if (turnInput != 0)
        {
            float rotationAmount = turnInput * turnSpeed * Time.deltaTime;
            transform.Rotate(0f, rotationAmount, 0f);
        }

        // 2. Ýleri ve Geri Yürüme (Hareket)
        bool isMoving = moveInput != 0;

        // Sadece ileri giderken (moveInput > 0) koþmaya izin veriyoruz
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && isMoving && moveInput > 0 && currentStamina > 0;

        float currentSpeed = isRunning ? runSpeed : (isMoving ? walkSpeed : 0f);

        if (isMoving)
        {
            // Karakterin o an baktýðý yöne (Local Forward) göre hareket et
            Vector3 moveDirection = transform.forward * moveInput;
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        // Animasyon ve Kamera FOV Kontrolü
        // Geriye doðru yürürken de (moveInput < 0) animasyonun çalýþmasý için mutlak deðer (Abs) kullanýyoruz
        anim.SetFloat("Speed", isMoving ? currentSpeed : 0f);
        cameraController.SetFOV(isRunning ? 50f : 65f);

        // Ses ve Stamina Kontrolleri
        if (isRunning)
        {
            PlaySound(runSound);
            currentStamina -= Time.deltaTime * 10f;
            uiManager.UpdateUI(this);
        }
        else if (isMoving)
        {
            PlaySound(walkSound);
        }

        // Zýplama Kontrolü
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
        anim.SetTrigger("Hit");
        uiManager.UpdateUI(this);

        if (currentHealth <= 0) TransitionToState(deadState);
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