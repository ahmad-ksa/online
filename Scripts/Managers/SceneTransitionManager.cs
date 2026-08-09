using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// مدير انتقالات المشاهد
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager instance;

    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// الانتقال إلى مشهد جديد مع Fade Animation
    /// </summary>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithFade(sceneName));
    }

    private System.Collections.IEnumerator LoadSceneWithFade(string sceneName)
    {
        // Fade Out
        yield return StartCoroutine(Fade(1f));

        // Load Scene
        SceneManager.LoadScene(sceneName);

        // Fade In
        yield return StartCoroutine(Fade(0f));
    }

    private System.Collections.IEnumerator Fade(float targetAlpha)
    {
        if (fadePanel == null)
            yield break;

        float elapsedTime = 0f;
        float startAlpha = fadePanel.alpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        fadePanel.alpha = targetAlpha;
    }

    public static SceneTransitionManager Instance => instance;
}
