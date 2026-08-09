using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// واجهة عنصر الصديق
/// </summary>
public class FriendItemUI : MonoBehaviour
{
    [SerializeField] private Text friendNameText;
    [SerializeField] private Image statusIndicator;
    [SerializeField] private Button messageButton;
    [SerializeField] private Button inviteButton;
    [SerializeField] private Image friendAvatarImage;

    private string friendId;
    private bool isOnline;

    public void SetFriend(string friendName, bool online)
    {
        friendNameText.text = friendName;
        isOnline = online;

        // تغيير لون المؤشر
        if (statusIndicator != null)
        {
            statusIndicator.color = online ? Color.green : Color.gray;
        }

        // تفعيل/تعطيل زر الدعوة
        if (inviteButton != null)
        {
            inviteButton.interactable = online; // يمكن الدعوة فقط إذا كان متصل
        }
    }

    public void SetFriendWithId(string id, string friendName, bool online)
    {
        friendId = id;
        SetFriend(friendName, online);
    }

    public string GetFriendId() => friendId;
    public string GetFriendName() => friendNameText.text;
    public bool IsOnline => isOnline;
}
