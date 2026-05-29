using UnityEngine;

public class UnarmedState : IPlayerState
{
    public void EnterState(PlayerController player)
    {
        player.anim.SetBool("HasRifle", false);

        // Hem UI ikonlarýný hem de eldeki 3D modeli yönetiyoruz
        player.punchIconUI.SetActive(true);
        player.rifleIconUI.SetActive(false);
        if (player.rifleModel != null) player.rifleModel.SetActive(false);

        player.uiManager.ToggleCrosshairVisibility(false);
    }

    public void UpdateState(PlayerController player)
    {
        player.HandleMovementAndRotation();

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            player.anim.SetTrigger("EquipWeapon");
            player.TransitionToState(player.armedState);
        }
    }

    public void ExitState(PlayerController player) { }
}

public class ArmedState : IPlayerState
{
    public void EnterState(PlayerController player)
    {
        player.anim.SetBool("HasRifle", true);

        // Hem UI ikonlarýný hem de eldeki 3D modeli yönetiyoruz
        player.punchIconUI.SetActive(false);
        player.rifleIconUI.SetActive(true);
        if (player.rifleModel != null) player.rifleModel.SetActive(true);

        player.uiManager.ToggleCrosshairVisibility(true);
    }

    public void UpdateState(PlayerController player)
    {
        player.HandleMovementAndRotation();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            player.anim.SetTrigger("HolsterWeapon");
            player.TransitionToState(player.unarmedState);
        }

        if (Input.GetMouseButton(1))
        {
            player.TransitionToState(player.aimingState);
        }

        if (Input.GetMouseButton(0))
        {
            player.FireWeapon();
        }
    }

    public void ExitState(PlayerController player) { }
}

public class AimingState : IPlayerState
{
    public void EnterState(PlayerController player)
    {
        player.anim.SetBool("IsAiming", true);
        player.cameraController.SetFOV(25f); // 40f'den 25f'e düþürdük, çok net bir yakýnlaþtýrma saðlayacak
        player.uiManager.SetAimingState(true);
    }

    public void UpdateState(PlayerController player)
    {
        player.HandleMovementAndRotation();

        if (Input.GetMouseButtonUp(1))
        {
            player.TransitionToState(player.armedState);
        }

        if (Input.GetMouseButton(0))
        {
            player.FireWeapon();
        }
    }

    public void ExitState(PlayerController player)
    {
        player.anim.SetBool("IsAiming", false);
        player.cameraController.SetFOV(65f);
        player.uiManager.SetAimingState(false);
    }
}

public class DeadState : IPlayerState
{
    public void EnterState(PlayerController player)
    {
        player.anim.SetTrigger("Die");
        player.uiManager.ShowDeathPanel();
    }

    public void UpdateState(PlayerController player) { }
    public void ExitState(PlayerController player) { }
}