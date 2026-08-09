using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// متحكم الحركة والتحريك للاعب
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Canvas playerNameCanvas;
    [SerializeField] private TextMesh playerNameMesh;
    [SerializeField] private Transform headPosition;

    private Vector3 moveDirection = Vector3.zero;
    private bool isLocalPlayer = false;
    private string playerId = "";
    private string playerName = "";

    // الإدخال
    private bool isUsingJoystick = false;
    private FixedJoystick joystick;

    private NetworkManager networkManager;
    private PlayerNetworkSync networkSync;

    private void Start()
    {
        networkManager = NetworkManager.Instance;
        networkSync = GetComponent<PlayerNetworkSync>();

        // تحديد لاعب محلي أو بعيد
        isLocalPlayer = networkSync.IsLocalPlayer;

        // إعداد الكاميرا للاعب المحلي فقط
        if (isLocalPlayer)
        {
            SetupLocalPlayer();
        }
        else
        {
            SetupRemotePlayer();
        }

        Logger.Log($"PlayerController initialized: {playerName}", "PlayerController");
    }

    private void SetupLocalPlayer()
    {
        // البحث عن Joystick
        joystick = FindObjectOfType<FixedJoystick>();
        isUsingJoystick = joystick != null;

        // إضافة كاميرا
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.transform.parent = transform;
            cameraObj.transform.localPosition = new Vector3(0, 0.6f, -5);
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
        }
        else
        {
            mainCamera.transform.parent = transform;
            mainCamera.transform.localPosition = new Vector3(0, 0.6f, -5);
        }

        Logger.Log("Local player setup complete", "PlayerController");
    }

    private void SetupRemotePlayer()
    {
        // تعطيل الكاميرا والإدخال للاعب البعيد
        Logger.Log($"Remote player setup: {playerName}", "PlayerController");
    }

    private void Update()
    {
        if (!isLocalPlayer)
            return;

        HandleInput();
        UpdateNameDisplay();
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        ApplyMovement();
    }

    /// <summary>
    /// معالجة الإدخال (لاعب محلي فقط)
    /// </summary>
    private void HandleInput()
    {
        moveDirection = Vector3.zero;

        // إدخال لوحة المفاتيح
        if (Input.GetKey(KeyCode.W))
            moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.S))
            moveDirection -= transform.forward;
        if (Input.GetKey(KeyCode.A))
            moveDirection -= transform.right;
        if (Input.GetKey(KeyCode.D))
            moveDirection += transform.right;

        // إدخال Joystick (الموبايل)
        if (isUsingJoystick && joystick != null)
        {
            moveDirection = new Vector3(joystick.Horizontal, 0, joystick.Vertical);
        }

        // تطبيع الاتجاه
        if (moveDirection.magnitude > 0)
        {
            moveDirection.Normalize();
            UpdateRotation();
        }

        // تحديث الـ Animation
        if (animator != null)
        {
            animator.SetFloat("Speed", moveDirection.magnitude);
        }
    }

    /// <summary>
    /// تطبيق الحركة على الـ Rigidbody
    /// </summary>
    private void ApplyMovement()
    {
        if (rb == null)
            return;

        // حساب السرعة المستهدفة
        Vector3 targetVelocity = moveDirection * moveSpeed;
        targetVelocity.y = rb.velocity.y; // الحفاظ على الـ gravity

        rb.velocity = targetVelocity;

        // إرسال الموضع والاتجاه إلى السيرفر
        if (isLocalPlayer && networkSync != null)
        {
            networkSync.SendPositionUpdate(transform.position, transform.rotation);
        }
    }

    /// <summary>
    /// تحديث دوران اللاعب
    /// </summary>
    private void UpdateRotation()
    {
        Vector3 lookDir = moveDirection;
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    /// <summary>
    /// تحديث عرض اسم اللاعب
    /// </summary>
    private void UpdateNameDisplay()
    {
        if (playerNameMesh != null)
        {
            playerNameMesh.text = playerName;
            // توجيه النص نحو الكاميرا
            if (playerNameCanvas != null)
            {
                playerNameCanvas.transform.LookAt(Camera.main.transform);
            }
        }
    }

    /// <summary>
    /// تعيين بيانات اللاعب
    /// </summary>
    public void SetPlayerData(string id, string name)
    {
        playerId = id;
        playerName = name;
        gameObject.name = $"Player_{name}";
    }

    /// <summary>
    /// تحديث موضع اللاعب البعيد
    /// </summary>
    public void UpdateRemotePosition(Vector3 position, Quaternion rotation)
    {
        if (isLocalPlayer)
            return;

        // تحريك سلس للاعب البعيد
        transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 10f);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 5f);
    }

    // Getters
    public string PlayerId => playerId;
    public string PlayerName => playerName;
    public bool IsLocalPlayer => isLocalPlayer;
    public Vector3 CurrentVelocity => rb != null ? rb.velocity : Vector3.zero;
    public Transform HeadPosition => headPosition;
}
