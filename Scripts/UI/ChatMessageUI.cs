using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// واجهة رسالة الدردشة
/// </summary>
public class ChatMessageUI : MonoBehaviour
{
    [SerializeField] private Text senderNameText;
    [SerializeField] private Text messageContentText;
    [SerializeField] private Text timestampText;
    [SerializeField] private Image senderAvatarImage;
    [SerializeField] private LayoutElement layoutElement;

    public void SetMessage(string senderName, string content)
    {
        if (senderNameText != null)
            senderNameText.text = senderName;

        if (messageContentText != null)
        {
            messageContentText.text = content;
            // ضبط حجم العنصر حسب طول النص
            if (layoutElement != null)
            {
                layoutElement.preferredHeight = Mathf.Clamp(content.Length / 2, 50, 150);
            }
        }

        if (timestampText != null)
            timestampText.text = System.DateTime.Now.ToString("HH:mm");
    }

    public void SetMessageWithTimestamp(string senderName, string content, string timestamp)
    {
        SetMessage(senderName, content);
        if (timestampText != null)
            timestampText.text = timestamp;
    }
}
