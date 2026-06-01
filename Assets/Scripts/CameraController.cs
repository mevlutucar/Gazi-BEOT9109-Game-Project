using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera playerCamera;
    private float fovLerpSpeed = 5f;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 150f;

    // ALT VE ÜST LÝMÝTLERÝ BURADAN ÝSTEDÝÐÝN GÝBÝ AYARLAYABÝLÝRSÝN
    public float minXAngleRotation = -25f; // Ne kadar YUKARI bakabileceði (Eskiden -40'tý, daraltýldý)
    public float maxXAngleRotation = 45f;  // Ne kadar AÞAÐI bakabileceði (Eskiden 60'tý, daraltýldý)

    private float targetFOV = 60f; // Baþlangýç hedefi küçültüldü
    private float xRotation = 17.854f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 1. Mouse Y (Yukarý/Aþaðý Bakma) Geçiþi
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;

        // Belirlediðin alt ve üst limitler kamerayý burada kilitler
        xRotation = Mathf.Clamp(xRotation, minXAngleRotation, maxXAngleRotation);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 2. FOV (Yakýnlaþma) Geçiþi
        if (playerCamera.fieldOfView != targetFOV)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);
        }
    }

    public void SetAimTarget(bool isAiming, float targetFovValue)
    {
        targetFOV = targetFovValue;
    }
}