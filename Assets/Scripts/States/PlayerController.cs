using System.Xml;
using UnityEngine;

// State Pattern - Temel Arayüz
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

        // Baþlangýç durumu
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

    public void TakeDamage(float damage)
    {
        if (currentState == deadState) return;

        currentHealth -= damage;
        anim.SetTrigger("Hit");
        uiManager.UpdateUI(this);

        if (currentHealth <= 0)
        {
            TransitionToState(deadState);
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