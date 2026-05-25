using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public Animator animator;

    bool hasRifle = false;

    public GameObject rifleObject;

    void Start()
    {
        rifleObject.SetActive(false);
    }

    void Update()
    {
        HandleMovement();
        HandleRifle();
        HandleAiming();
        HandleFiring();
    }

    void HandleMovement()
    {
        bool walk =
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.UpArrow);

        bool run =
            Input.GetKey(KeyCode.LeftShift) &&
            walk;

        animator.SetBool("IsWalking", walk);
        animator.SetBool("IsRunning", run);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetBool("IsJumping", true);
        }
    }

    void HandleRifle()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            hasRifle = !hasRifle;

            animator.SetBool("HasRifle", hasRifle);

            if (hasRifle)
            {
                animator.SetTrigger("EquipRifle");
            }
            else
            {
                animator.SetTrigger("UnequipRifle");
            }
        }
    }

    void HandleAiming()
    {
        if (!hasRifle)
        {
            animator.SetBool("IsAiming", false);
            return;
        }

        bool aiming = Input.GetMouseButton(1);

        animator.SetBool("IsAiming", aiming);
    }

    void HandleFiring()
    {
        if (!hasRifle)
        {
            animator.SetBool("IsFiring", false);
            return;
        }

        bool firing = Input.GetMouseButton(0);

        animator.SetBool("IsFiring", firing);
    }

    public void EndJump()
    {
        animator.SetBool("IsJumping", false);
    }

    public void HitReaction()
    {
        animator.SetTrigger("Hit");
    }

    // Animation Event
    public void ShowRifle()
    {
        rifleObject.SetActive(true);
    }

    // Animation Event
    public void HideRifle()
    {
        rifleObject.SetActive(false);
    }
}

