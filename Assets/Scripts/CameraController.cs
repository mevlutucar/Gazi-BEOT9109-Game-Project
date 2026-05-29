using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera playerCamera;
    private float fovLerpSpeed = 5f;

    [Header("Aiming Settings")]
    public float normalRotX = 17.854f; // Normal duruþtaki kamera X açýsý
    public float aimRotX = 2f;         // Niþan alýrken kameranýn ineceði X açýsý

    private float targetFOV = 65f;
    private float targetRotX;

    void Start()
    {
        targetRotX = normalRotX;
    }

    void Update()
    {
        // 1. FOV (Yakýnlaþma) Geçiþi
        if (playerCamera.fieldOfView != targetFOV)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);
        }

        // 2. X Rotasyonu (Kafa Eðimi) Geçiþi
        Vector3 currentRot = transform.localEulerAngles;
        // Unity açýlarý 0-360 arasýdýr, eksi açýlara yumuþak inmesi için matematiksel düzeltme:
        float currentX = currentRot.x > 180f ? currentRot.x - 360f : currentRot.x;
        float newX = Mathf.Lerp(currentX, targetRotX, Time.deltaTime * fovLerpSpeed);

        transform.localEulerAngles = new Vector3(newX, currentRot.y, currentRot.z);
    }

    // PlayerController içinden FOV ve Rotasyonu ayný anda tetiklemek için merkez fonksiyon
    public void SetAimTarget(bool isAiming, float targetFovValue)
    {
        targetFOV = targetFovValue;
        targetRotX = isAiming ? aimRotX : normalRotX;
    }
}