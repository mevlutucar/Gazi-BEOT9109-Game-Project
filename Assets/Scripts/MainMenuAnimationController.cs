using UnityEngine;
using System.Collections;

public class MainMenuAnimationController : MonoBehaviour
{
    [Header("Karakter Animatörleri")]
    public Animator playerAnimator;
    public Animator chiefAnimator;

    [Header("Animasyon Ýsimleri")]
    public string playerAnimName = "Rifle Idle";
    public string chiefAnimName = "Looking";

    void Start()
    {
        // Animatörlerin sahne yüklenirken hazýr olmasý için 1 kare bekletiyoruz
        StartCoroutine(PlayAnimationsRoutine());
    }

    private IEnumerator PlayAnimationsRoutine()
    {
        yield return new WaitForEndOfFrame();

        // PLAYER KONTROLÜ (Artýk Base Layer yani 0. katmana bakýyor)
        if (playerAnimator != null && playerAnimator.runtimeAnimatorController != null)
        {
            playerAnimator.Play(playerAnimName, 0, 0f);
        }
        else
        {
            Debug.LogWarning("DÝKKAT: Player Animator boþ veya Controller takýlmamýþ!");
        }

        // CHIEF KONTROLÜ (Artýk Base Layer yani 0. katmana bakýyor)
        if (chiefAnimator != null && chiefAnimator.runtimeAnimatorController != null)
        {
            chiefAnimator.Play(chiefAnimName, 0, 0f);
        }
        else
        {
            Debug.LogWarning("DÝKKAT: Chief Animator boþ veya Controller takýlmamýþ!");
        }
    }
}