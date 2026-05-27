using UnityEngine;

public class UnarmedState : IPlayerState
{
    public void EnterState(PlayerController player)
    {
        player.anim.SetBool("HasRifle", false);
        player.uiManager.SetWeaponIcon(false);

        // Silahý gizle
        if (player.rifleObject != null) player.rifleObject.SetActive(false);
    }

    public void UpdateState(PlayerController player)
    {
        player.HandleMovementAndRotation(); // Yeni merkez fonksiyon

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

        // Silahý aktif et
        if (player.rifleObject != null) player.rifleObject.SetActive(true);
    }

    public void UpdateState(PlayerController player)
    {
        player.HandleMovementAndRotation(); // Yeni merkez fonksiyon

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            player.anim.SetTrigger("HolsterWeapon");
            player.TransitionToState(player.unarmedState);
        }

        if (Input.GetMouseButton(1)) // Sað Týk
        {
            player.TransitionToState(player.aimingState);
        }

        if (Input.GetMouseButtonDown(0))
        {
            FireWeapon(player);
        }
    }

    public void ExitState(PlayerController player) { }

    private void FireWeapon(PlayerController player)
    {
        if (player.ammoCount > 0)
        {
            player.anim.SetTrigger("Fire");
            player.PlaySound(player.shootSound);
            player.ammoCount--;
            player.uiManager.UpdateUI(player);
        }
        else
        {
            player.PlaySound(player.emptyMagSound);
        }
    }
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
        player.HandleMovementAndRotation(); // Niþan alýrken de hareket edebilsin

        if (Input.GetMouseButtonUp(1))
        {
            player.TransitionToState(player.armedState);
        }

        if (Input.GetMouseButtonDown(0))
        {
            FireWeaponAiming(player);
        }
    }

    public void ExitState(PlayerController player)
    {
        player.anim.SetBool("IsAiming", false);
        player.cameraController.SetFOV(65f);
        player.uiManager.ToggleCrosshair(false);
    }

    private void FireWeaponAiming(PlayerController player)
    {
        if (player.ammoCount > 0)
        {
            player.anim.SetTrigger("Fire");
            player.PlaySound(player.shootSound);
            player.ammoCount--;
            player.uiManager.UpdateUI(player);
        }
        else
        {
            player.PlaySound(player.emptyMagSound);
        }
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