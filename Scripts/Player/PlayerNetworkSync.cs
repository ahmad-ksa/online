using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

/// <summary>
/// تزامن بيانات اللاعب عبر الشبكة
/// </summary>
public class PlayerNetworkSync : MonoBehaviour
{
    private string playerId = "";
    private string playerName = "";
    private bool isLocalPlayer = false;

    private Vector3 lastSyncPosition = Vector3.zero;
    private Quaternion lastSyncRotation = Quaternion.identity;
    private float syncInterval = 0.1f; // إرسال كل 100ms
    private float timeSinceLastSync = 0f;

    private PlayerController playerController;
    private NetworkManager networkManager;
    private PlayerManager playerManager;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        networkManager = NetworkManager.Instance;
        playerManager = PlayerManager.Instance;

        // تحديد إذا كان لاعب محلي أو بعيد
        isLocalPlayer = CompareTag("LocalPlayer");

        if (isLocalPlayer)
        {
            // الحصول على بيانات اللاعب المحلي
            if (playerManager.CurrentProfile != null)
            {
                playerId = playerManager.CurrentProfile.playerId;
                playerName = playerManager.CurrentProfile.username;
            }

            playerController.SetPlayerData(playerId, playerName);
            Logger.Log($"Local player sync initialized: {playerName}", "PlayerNetworkSync");
        }

        lastSyncPosition = transform.position;
        lastSyncRotation = transform.rotation;
    }

    private void Update()
    {
        if (!isLocalPlayer)
            return;

        // تحديث التزامن
        timeSinceLastSync += Time.deltaTime;
        if (timeSinceLastSync >= syncInterval)
        {
            SendPositionUpdate(transform.position, transform.rotation);
            timeSinceLastSync = 0f;
        }
    }

    /// <summary>
    /// إرسال تحديث الموضع للسيرفر
    /// </summary>
    public void SendPositionUpdate(Vector3 position, Quaternion rotation)
    {
        // التحقق من التغيير
        float posDistance = Vector3.Distance(position, lastSyncPosition);
        float rotDifference = Quaternion.Angle(rotation, lastSyncRotation);

        if (posDistance < 0.01f && rotDifference < 1f)
            return; // لا تحديث إذا كان التغيير صغير جداً

        lastSyncPosition = position;
        lastSyncRotation = rotation;

        // إنشاء رسالة التحديث
        var updateMsg = new NetworkMessage
        {
            messageType = "PLAYER_POSITION_UPDATE",
            data = new Dictionary<string, object>
            {
                { "playerId", playerId },
                { "posX", position.x },
                { "posY", position.y },
                { "posZ", position.z },
                { "rotX", rotation.x },
                { "rotY", rotation.y },
                { "rotZ", rotation.z },
                { "rotW", rotation.w }
            }
        };

        // إرسال الرسالة (async)
        _ = networkManager.SendMessage(updateMsg);
    }

    /// <summary>
    /// استقبال تحديث موضع لاعب بعيد
    /// </summary>
    public void ReceivePositionUpdate(Vector3 position, Quaternion rotation)
    {
        if (isLocalPlayer)
            return;

        playerController.UpdateRemotePosition(position, rotation);
    }

    /// <summary>
    /// تعيين بيانات اللاعب البعيد
    /// </summary>
    public void SetRemotePlayerData(string id, string name)
    {
        playerId = id;
        playerName = name;
        isLocalPlayer = false;
        playerController.SetPlayerData(id, name);
    }

    // Getters
    public string PlayerId => playerId;
    public string PlayerName => playerName;
    public bool IsLocalPlayer => isLocalPlayer;
}
