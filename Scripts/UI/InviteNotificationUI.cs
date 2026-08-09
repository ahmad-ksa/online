using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// واجهة إشعار الدعوة
/// </summary>
public class InviteNotificationUI : MonoBehaviour
{
    [SerializeField] private Text inviteTextText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;
    [SerializeField] private Image senderAvatarImage;

    private string requestId;
    private string fromPlayerId;
    private FriendsManager friendsManager;

    private void Start()
    {
        friendsManager = FriendsManager.Instance;

        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAccept);

        if (declineButton != null)
            declineButton.onClick.AddListener(OnDecline);
    }

    public void SetInvite(string fromPlayerName)
    {
        if (inviteTextText != null)
            inviteTextText.text = $"{fromPlayerName} يريد إضافتك صديق!";
    }

    public void SetInviteWithId(string id, string fromId, string fromPlayerName)
    {
        requestId = id;
        fromPlayerId = fromId;
        SetInvite(fromPlayerName);
    }

    private async void OnAccept()
    {
        if (friendsManager != null && !string.IsNullOrEmpty(requestId))
        {
            bool success = await friendsManager.AcceptFriendRequest(requestId);
            if (success)
            {
                Logger.Log("Friend request accepted", "InviteNotificationUI");
                Destroy(gameObject);
            }
        }
    }

    private async void OnDecline()
    {
        if (friendsManager != null && !string.IsNullOrEmpty(requestId))
        {
            bool success = await friendsManager.DeclineFriendRequest(requestId);
            if (success)
            {
                Logger.Log("Friend request declined", "InviteNotificationUI");
                Destroy(gameObject);
            }
        }
    }
}
