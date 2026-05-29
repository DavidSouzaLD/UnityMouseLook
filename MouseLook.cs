using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public struct ClampAngle
    {
        public bool use;

        [Range(-360f, 0f)]
        public float min;

        [Range(0f, 360f)]
        public float max;
    }

    #region Inspector Settings

    [Header("Settings")]
    [SerializeField] private float sensitivity = 0.3f;
    [SerializeField] private float smoothing = 20f;

    [Header("Axis Transforms")]
    [Tooltip("Usually the Camera or a Camera Holder")]
    [SerializeField] private Transform rotAxisX;

    [Tooltip("Usually the Character Root")]
    [SerializeField] private Transform rotAxisY;

    [Header("Clamp")]
    [SerializeField]
    private ClampAngle verticalClamp = new()
    {
        use = true,
        min = -90f,
        max = 90f
    };

    [SerializeField]
    private ClampAngle horizontalClamp = new()
    {
        use = false,
        min = -360f,
        max = 360f
    };

    #endregion

    #region Private State

    private float _initSensitivity;
    private Vector3 _rotation;
    private Vector3 _smoothedRotation;

    public ClampAngle VerticalClamp { get => verticalClamp; set => verticalClamp = value; }

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        _initSensitivity = sensitivity;

        if (rotAxisX == null || rotAxisY == null)
        {
            Debug.LogWarning("MouseLook: rotAxisX or rotAxisY is not assigned in the Inspector!", this);
        }

        SetLockCursorAndPause(true);
    }

    private void LateUpdate()
    {
        CalculateRotation();
        ApplyRotation();
    }

    #endregion

    #region Core Logic

    private void CalculateRotation()
    {
        Vector2 input = GInput.GetAxis("Character", "Look"); // Vector2 axis entry (Vertical / Horizontal) 
        Vector2 lookInput = input * (sensitivity * Time.deltaTime * 100f);

        _rotation.x -= lookInput.y;
        _rotation.y += lookInput.x;

        if (VerticalClamp.use)
        {
            _rotation.x = Mathf.Clamp(_rotation.x, VerticalClamp.min, VerticalClamp.max);
        }

        if (horizontalClamp.use)
        {
            _rotation.y = Mathf.Clamp(_rotation.y, horizontalClamp.min, horizontalClamp.max);
        }

        if (smoothing > 0f)
        {
            _smoothedRotation = Vector3.Lerp(_smoothedRotation, _rotation, smoothing * Time.deltaTime);
        }
        else
        {
            _smoothedRotation = _rotation;
        }
    }

    private void ApplyRotation()
    {
        if (rotAxisX != null)
            rotAxisX.localRotation = Quaternion.Euler(_smoothedRotation.x, 0f, 0f);

        if (rotAxisY != null)
            rotAxisY.rotation = Quaternion.Euler(0f, _smoothedRotation.y, 0f);
    }

    #endregion

    #region Public API

    public void SetSensitivityScale(float scale)
    {
        if (scale < 0f) return;

        sensitivity *= scale;
        sensitivity = Mathf.Clamp(sensitivity, 0f, 10f);
    }

    public void ResetSensitivity()
    {
        sensitivity = _initSensitivity;
    }

    public void SetLockCursorAndPause(bool isLocked, bool pause = false)
    {
        if (isLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (pause)
        {
            SetSensitivityScale(0f);
        }
        else
        {
            ResetSensitivity();
        }
    }

    #endregion
}
