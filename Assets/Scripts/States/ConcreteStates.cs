using UnityEngine;

public class UnarmedState : IPlayerState
{
    public void EnterState(PlayerController player)
    {
        player.anim.SetBool("HasRifle", false);
        player.uiManager.SetWeaponIcon(false);
        if (player.rifleObject != null) player.rifleObject.SetActive(false);
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
        player.uiManager.SetWeaponIcon(true);
        if (player.rifleObject != null) player.rifleObject.SetActive(true);
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

        // GetMouseButton kullandýðýmýz için basýlý tuttukça fireRate aralýðýyla ateþ eder
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
        player.cameraController.SetFOV(40f);
        player.uiManager.ToggleCrosshair(true);
    }

    public void UpdateState(PlayerController player)
    {
        player.HandleMovementAndRotation();

        if (Input.GetMouseButtonUp(1))
        {
            player.TransitionToState(player.armedState);
        }

        // Niþan alýrken de ateþ hýzý kuralý geçerli
        if (Input.GetMouseButton(0))
        {
            player.FireWeapon();
        }
    }

    public void ExitState(PlayerController player)
    {
        player.anim.SetBool("IsAiming", false);
        player.cameraController.SetFOV(65f);
        player.uiManager.ToggleCrosshair(false);
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