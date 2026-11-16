using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // Define os estados possíveis da câmera
    public enum CameraMode { Menu, OrbitaManual, Free, Still }

    [Header("Modo Inicial")]
    public CameraMode currentMode = CameraMode.Menu;

    [Header("Alvo de Rotação")]
    public Transform target;

    [Header("Ajuste de Tela")]
    public float leftShift = 7.0f;

    [Header("Configurações do Modo Menu (Auto)")]
    public float autoRotationSpeed = 15.0f;
    public float autoHeight = 5.0f;
    public float autoDistance = 15.0f;

    [Header("Velocidade dos Controles Manuais")]
    public float xSpeed = 30.0f;
    public float ySpeed = 30.0f;
    public float panSpeed = 0.5f;
    public float zoomSpeed = 2.0f;
    public float keyboardMoveSpeed = 10.0f;

    [Header("Limites da Câmera")]
    public float distanceMin = 2.0f;
    public float distanceMax = 50.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    private float x = 0.0f;
    private float y = 0.0f;
    private float distance = 15.0f;
    private float autoAngle = 0.0f;
    private Vector3 initialTargetPosition;
    private float initialX;
    private float initialY;
    private float initialDistance;
    public bool allowMode1 = false;
    public bool allowMode2 = false;

    [Header("Modo Foco")]
    public float focusDistance = 8.0f;

    private Transform focusTarget;
    private bool isFocusMode = false;
    private float distanceBeforeFocus;

    void Start()
    {
        distanceMax = 200.0f;
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        if (target == null)
        {
            GameObject go = new GameObject("CameraTarget");
            go.transform.position = Vector3.zero;
            target = go.transform;
        }
        initialTargetPosition = target.position;
        Vector3 initialAngles = transform.eulerAngles;

        initialX = initialAngles.y;
        initialY = initialAngles.x;
        initialDistance = autoDistance;
        allowMode1 = (PlayerPrefs.GetInt("Config_RotateCamera1", 1) == 1);
        allowMode2 = (PlayerPrefs.GetInt("Config_RotateCamera2", 1) == 1);
        ApplyPositionAndRotation();
    }

    void LateUpdate()
    {
        if (target == null) return;

        switch (currentMode)
        {
            case CameraMode.Menu:
                ExecuteMenuMode();
                break;
            case CameraMode.OrbitaManual:
                ExecuteManualOrbitMode();
                break;
            case CameraMode.Free:
                ExecuteFreeMode();
                break;
            case CameraMode.Still:
                ExecuteStillMode();
                break;
        }
    }

    // Modo de rotação do menu principal
    void ExecuteMenuMode()
    {
        if (allowMode1)
        {
            autoAngle += autoRotationSpeed * Time.deltaTime;
        }
        float radiano = autoAngle * Mathf.Deg2Rad;

        float posX = target.position.x + Mathf.Sin(radiano) * autoDistance;
        float posZ = target.position.z + Mathf.Cos(radiano) * autoDistance;
        float posY = target.position.y + autoHeight;

        transform.position = new Vector3(posX, posY, posZ);
        //transform.LookAt(target.position);
        transform.LookAt(target.position + (transform.right * -leftShift));
        
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        distance = autoDistance;
        leftShift = 5f;
    }

    // Modo que o usuário vê no menu de parametros, podendo rotar em volta da função
    void ExecuteManualOrbitMode()
    {
        if (allowMode2 == true)
        {
            RotateWithMouse();
            CalculateZoom();
            ApplyPositionAndRotation();
        }
        leftShift = 7.0f;
    }

    // Modo de movimento livre
    void ExecuteFreeMode()
    {
        if (isFocusMode)
        {
            ApplyPositionAndRotation();
            leftShift = 0f;
            return;
        }

        MoveWithKeyboard();
        DragWithMiddleMouseButton();
        RotateWithMouse();
        CalculateZoom();

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            CenterCameraOnObject();
        }

        ApplyPositionAndRotation();
        leftShift = 0f;
    }

    void ExecuteStillMode()
    {
        // Nada
    }

    void RotateWithMouse()
    {
        bool isPointerPressed = Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed;
        if (isPointerPressed && !Mouse.current.middleButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            x += mouseDelta.x * xSpeed * 0.005f;
            y -= mouseDelta.y * ySpeed * 0.005f;
            y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
        }
    }

    void CalculateZoom()
    {
        Vector2 scrollDelta = Mouse.current.scroll.ReadValue();
        if (scrollDelta.y != 0)
        {
            float scrollAmount = Mathf.Sign(scrollDelta.y) * zoomSpeed;
            distance = Mathf.Clamp(distance - scrollAmount, distanceMin, distanceMax);
        }
    }

    void MoveWithKeyboard()
    {
        Vector3 inputDir = Vector3.zero;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) inputDir += transform.forward;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) inputDir -= transform.forward;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) inputDir -= transform.right;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) inputDir += transform.right;
        if (Keyboard.current.spaceKey.isPressed || Keyboard.current.eKey.isPressed)
            inputDir += Vector3.up;

        if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.qKey.isPressed)
            inputDir += Vector3.down;


        if (inputDir != Vector3.zero)
        {
            target.position += inputDir.normalized * keyboardMoveSpeed * Time.deltaTime;
        }
    }
    void CenterCameraOnObject()
    {
        target.position = initialTargetPosition;

        ApplyPositionAndRotation();
    }

    void DragWithMiddleMouseButton()
    {
        if (Mouse.current.middleButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            Vector3 panOffset = transform.right * (-mouseDelta.x * panSpeed * 0.02f) +
                               transform.up * (-mouseDelta.y * panSpeed * 0.02f);
            target.position += panOffset;
        }
    }

    void ApplyPositionAndRotation()
    {
        Quaternion rotation = Quaternion.Euler(y, x, 0);

        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);

        // Centro real da órbita
        Vector3 orbitCenter = isFocusMode && focusTarget != null
            ? focusTarget.position
            : target.position;

        // Deslocamento lateral proporcional à distância
        float shift = leftShift * (distance / 15.0f);

        // Aplica o deslocamento no referencial da câmera
        Vector3 offset = rotation * Vector3.right * -shift;

        // Calcula a posição final
        Vector3 position = orbitCenter + offset + (rotation * negDistance);

        transform.rotation = rotation;
        transform.position = position;
    }

    public void ResetCameraPosition()
    {
        target.position = initialTargetPosition;

        x = initialX;
        y = initialY;

        distance = initialDistance;
        autoAngle = 0f;

        ApplyPositionAndRotation();
    }

    public void ChangeCameraMode(int novoModo)
    {
        currentMode = (CameraMode)novoModo;
    }

    public void changeMode1On()
    {
        allowMode1 = true;
    }

    public void changeMode1Off()
    {
        allowMode1 = false;
    }

    public void changeMode2On()
    {
        allowMode2 = true;
    }

    public void changeMode2Off()
    {
        allowMode2 = false;
    }

    public bool IsFocusMode()
    {
        return isFocusMode;
    }

    public void FocusOn(Transform newTarget)
    {
        if (newTarget == null) return;

        // Guarda a distância somente na primeira ativação
        if (!isFocusMode)
        {
            distanceBeforeFocus = distance;
        }

        focusTarget = newTarget;
        isFocusMode = true;

        distance = focusDistance;

        ApplyPositionAndRotation();
    }

    public void DisableFocus()
    {
        if (!isFocusMode) return;

        isFocusMode = false;
        focusTarget = null;

        distance = distanceBeforeFocus;

        ApplyPositionAndRotation();
    }

    public bool configFromMain = false;

    public void ConfigToMainTrue()
    {
        configFromMain = true;
    }

    public void ConfigReturnToMainCheck()
    {
        if(configFromMain)
        {
            configFromMain = false;
            ChangeCameraMode(0);
        }
    }
}
