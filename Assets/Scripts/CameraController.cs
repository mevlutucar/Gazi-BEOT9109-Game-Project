using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera playerCamera;
    private float targetFOV = 65f;
    private float fovLerpSpeed = 5f;

    void Update()
    {
        if (playerCamera.fieldOfView != targetFOV)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);
        }
    }

    public void SetFOV(float newFOV)
    {
        targetFOV = newFOV;
    }
}