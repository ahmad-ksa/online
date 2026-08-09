using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// نظام إدارة الصوت (Voice Chat) مع Proximity
/// </summary>
public class VoiceChatManager : MonoBehaviour
{
    private static VoiceChatManager instance;

    [SerializeField] private float proximityRange = 20f; // مدى الصوت
    [SerializeField] private float updateInterval = 0.1f;

    private bool isMicrophoneEnabled = true;
    private bool isListening = true;
    private float micVolume = 1f;
    private AudioSource audioSource;
    private Microphone microphone;
    private float timeSinceLastUpdate = 0f;

    private NetworkManager networkManager;
    private PlayerManager playerManager;
    private List<string> nearbyPlayers = new List<string>();

    // Events
    public event Action<string> OnPlayerVoiceStarted; // playerName
    public event Action<string> OnPlayerVoiceEnded;
    public event Action<bool> OnMicrophoneStatusChanged;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        networkManager = NetworkManager.Instance;
        playerManager = PlayerManager.Instance;

        InitializeAudio();

        Logger.Log("VoiceChatManager initialized", "VoiceChatManager");
    }

    private void InitializeAudio()
    {
        // إنشاء AudioSource للاستماع
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = false;
            audioSource.playOnAwake = false;
        }

        // التحقق من توفر الميكروفون
        if (Microphone.devices.Length > 0)
        {
            Logger.Log($"Microphone found: {Microphone.devices[0]}", "VoiceChatManager");
        }
        else
        {
            Logger.LogWarning("No microphone detected!", "VoiceChatManager");
            isMicrophoneEnabled = false;
        }
    }

    private void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;

        if (timeSinceLastUpdate >= updateInterval)
        {
            UpdateNearbyPlayers();
            timeSinceLastUpdate = 0f;
        }

        // معالجة إدخال الميكروفون
        if (isMicrophoneEnabled && Input.GetKey(KeyCode.V))
        {
            SendVoiceData();
        }
    }

    /// <summary>
    /// تفعيل/تعطيل الميكروفون
    /// </summary>
    public void SetMicrophoneEnabled(bool enabled)
    {
        if (enabled && Microphone.devices.Length == 0)
        {
            Logger.LogError("No microphone available!", "VoiceChatManager");
            return;
        }

        isMicrophoneEnabled = enabled;
        OnMicrophoneStatusChanged?.Invoke(enabled);

        Logger.Log($"Microphone {(enabled ? "enabled" : "disabled")}", "VoiceChatManager");
    }

    /// <summary>
    /// تعيين مستوى الصوت
    /// </summary>
    public void SetMicrophoneVolume(float volume)
    {
        micVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = micVolume;
        }
    }

    /// <summary>
    /// تحديث اللاعبين القريبين
    /// </summary>
    private void UpdateNearbyPlayers()
    {
        nearbyPlayers.Clear();

        // البحث عن اللاعبين القريبين
        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        Vector3 myPosition = transform.position;

        foreach (var player in allPlayers)
        {
            if (player.IsLocalPlayer)
                continue;

            float distance = Vector3.Distance(myPosition, player.transform.position);
            if (distance <= proximityRange)
            {
                nearbyPlayers.Add(player.PlayerId);
            }
        }
    }

    /// <summary>
    /// إرسال بيانات الصوت (محاكاة)
    /// </summary>
    private async void SendVoiceData()
    {
        try
        {
            if (!isMicrophoneEnabled || nearbyPlayers.Count == 0)
                return;

            var voiceMsg = new NetworkMessage
            {
                messageType = "VOICE_DATA",
                data = new Dictionary<string, object>
                {
                    { "nearbyPlayers", string.Join(",", nearbyPlayers) },
                    { "volume", micVolume },
                    { "duration", Time.deltaTime }
                }
            };

            await networkManager.SendMessage(voiceMsg);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to send voice data: {ex.Message}", "VoiceChatManager");
        }
    }

    /// <summary>
    /// استقبال صوت من لاعب قريب
    /// </summary>
    public void ReceiveVoiceData(string fromPlayerId, string fromPlayerName, byte[] audioData)
    {
        try
        {
            OnPlayerVoiceStarted?.Invoke(fromPlayerName);

            // TODO: تشغيل بيانات الصوت
            Logger.LogDebug($"Receiving voice from {fromPlayerName}", "VoiceChatManager");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to receive voice data: {ex.Message}", "VoiceChatManager");
        }
    }

    /// <summary>
    /// الحصول على مؤشر نشاط الصوت
    /// </summary>
    public float GetVoiceActivityLevel()
    {
        // TODO: حساب مستوى نشاط الصوت من الميكروفون
        return 0f;
    }

    // Getters
    public bool IsMicrophoneEnabled => isMicrophoneEnabled;
    public float MicVolume => micVolume;
    public float ProximityRange => proximityRange;
    public List<string> NearbyPlayers => nearbyPlayers;
    public static VoiceChatManager Instance => instance;
}
