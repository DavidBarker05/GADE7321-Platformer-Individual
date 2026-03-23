using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputScript))]
public class PlayerLook : MonoBehaviour
{
    [SerializeField]
    Transform cameraTarget;
    [SerializeField, Range(0.01f, 0.5f)]
    float deadZone = 0.05f;
    [SerializeField]
    [Min(0f)]
    float mouseHorizontalSensitivity = 0.2f;
    [SerializeField]
    [Min(0f)]
    float mouseVerticalSensitivity = 0.225f;
    [SerializeField]
    [Min(0f)]
    float controllerHorizontalSensitivity = 180f;
    [SerializeField]
    [Min(0f)]
    float controllerVerticalSensitivity = 202.5f;
    [SerializeField]
    [Range(-90f, 0f)]
    float minVerticalAngle = -80f;
    [SerializeField]
    [Range(0f, 90f)]
    float maxVerticalAngle = 80f;

    float pitch = 0f;
    float yaw = 0f;
    PlayerInputScript pIS;

    void Awake()
    {
        pIS = GetComponent<PlayerInputScript>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mX = pIS.LookInput.x * pIS.LookInput.x < deadZone * deadZone ? 0f : pIS.LookInput.x;
        float mY = pIS.LookInput.y * pIS.LookInput.y < deadZone * deadZone ? 0f : pIS.LookInput.y;
        float hSens = pIS.LookDevice is Gamepad ? controllerHorizontalSensitivity * Time.deltaTime : mouseHorizontalSensitivity;
        float vSens = pIS.LookDevice is Gamepad ? controllerVerticalSensitivity * Time.deltaTime : mouseVerticalSensitivity;
        yaw += mX * hSens;
        pitch = Mathf.Clamp(pitch - mY * vSens, minVerticalAngle, maxVerticalAngle);
        if (cameraTarget != null) cameraTarget.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
