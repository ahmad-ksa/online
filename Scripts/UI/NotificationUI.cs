using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// واجهة الإشعار العام
/// </summary>
public class NotificationUI : MonoBehaviour
{
    [SerializeField] private Text messageText;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button closeButton;

    public void SetMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        // إغلاق تلقائي بعد مدة
        StartCoroutine(AutoClose());
    }

    private IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(displayDuration);
        Close();
    }

    public void Close()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        if (canvasGroup == null)
        {
            Destroy(gameObject);
            yield break;
        }

        float elapsedTime = 0f;
        float fadeDuration = 0.5f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        Destroy(gameObject);
    }
}
