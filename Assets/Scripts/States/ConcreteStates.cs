using UnityEngine;

public class UnarmedState : IPlayerState
{
    public void EnterState(PlayerController player)
    {
        player.anim.SetBool("HasRifle", false);
        player.uiManager.SetWeaponIcon(false);
    }

    public void UpdateState(PlayerController player)
    {
        HandleMovement(player);

        // Silah Çekme
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            player.anim.SetTrigger("EquipWeapon");
            player.TransitionToState(player.armedState);
        }
    }

    public void ExitState(PlayerController player) { }

    private void HandleMovement(PlayerController player)
    {
        float moveZ = 0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveZ = 1f;

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && moveZ > 0 && player.currentStamina > 0;
        float currentSpeed = isRunning ? player.runSpeed : (moveZ > 0 ? player.walkSpeed : 0f);

        Vector3 move = player.transform.forward * moveZ;
        player.controller.Move(move * currentSpeed * Time.deltaTime);

        // Animasyon ve Kamera FOV Kontrolü
        player.anim.SetFloat("Speed", currentSpeed);
        player.cameraController.SetFOV(isRunning ? 50f : 65f);

        // Ses ve Stamina
        if (isRunning)
        {
            player.PlaySound(player.runSound);
            player.currentStamina -= Time.deltaTime * 10f;
            player.uiManager.UpdateUI(player);
        }
        else if (moveZ > 0)
        {
            player.PlaySound(player.walkSound);
        }

        // Zýplama
        if (Input.GetKeyDown(KeyCode.Space) && player.isGrounded && player.currentStamina >= 15f)
        {
            player.velocity.y = Mathf.Sqrt(player.jumpForce * -2f * player.gravity);
            player.anim.SetTrigger("Jump");
            player.PlaySound(player.jumpSound);
            player.currentStamina -= 15f;
            player.uiManager.UpdateUI(player);
        }
    }
}

public class ArmedState : IPlayerState
{
    public void EnterState(PlayerController player)
    {
        player.anim.SetBool("HasRifle", true);
        player.uiManager.SetWeaponIcon(true);
    }

    public void UpdateState(PlayerController player)
    {
        HandleMovement(player);

        // Silahý Kaldýrma
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            player.anim.SetTrigger("HolsterWeapon");
            player.TransitionToState(player.unarmedState);
        }

        // Niþan Alma Moduna Geçiþ
        if (Input.GetMouseButton(1)) // Sað Týk
        {
            player.TransitionToState(player.aimingState);
        }

        // Ateþ Etme (Kalçadan)
        if (Input.GetMouseButtonDown(0))
        {
            FireWeapon(player);
        }
    }

    public void ExitState(PlayerController player) { }

    private void HandleMovement(PlayerController player)
    {
        // UnarmedState içindeki HandleMovement mantýðýnýn aynýsý burada da çaðrýlýr.
        // Kod tekrarýný önlemek için PlayerController içinde ortak bir fonksiyona alýnabilir.
        float moveZ = 0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveZ = 1f;

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && moveZ > 0 && player.currentStamina > 0;
        float currentSpeed = isRunning ? player.runSpeed : (moveZ > 0 ? player.walkSpeed : 0f);

        Vector3 move = player.transform.forward * moveZ;
        player.controller.Move(move * currentSpeed * Time.deltaTime);

        player.anim.SetFloat("Speed", currentSpeed);
        player.cameraController.SetFOV(isRunning ? 50f : 65f);

        // Zýplama vb. kontroller ayný þekilde eklenebilir.
    }

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
        if (Input.GetMouseButtonUp(1)) // Sað Týk Býrakýldý
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
        player.cameraController.SetFOV(65f); // Normale dönüþ
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

    public void UpdateState(PlayerController player) { /* Ölü karakter hareket edemez */ }
    public void ExitState(PlayerController player) { }
}