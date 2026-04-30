using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionScript : MonoBehaviour
{
   
    private Image fade;
    private float fadeDuration = 0.2f;

    // Add to any scene
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        // Create the black scene transition instance
        var obj = new GameObject("SceneTransition[Persistent]");
        var sceneTrans = obj.AddComponent<SceneTransitionScript>();

        GameManager.Instance.SetTransitionInstance(sceneTrans);

    }


    // Persistent
    // Construct so that present in all scenes
    // Extremely sus (lol)
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        fade = gameObject.AddComponent<Image>();
        // Hide, default
        fade.color = new Color(0, 0, 0, 0);
    }





    // NOTE: this would be better via DotTween eventually maybe

    public void LoadScene(string sceneName)
    {
        StartCoroutine(Transition(sceneName));
    }

    private IEnumerator Transition(string sceneName)
    {
        // Fade to black
        yield return Fade(0, 1);

        // Load scene
        yield return SceneManager.LoadSceneAsync(sceneName);

        // Fade back
        yield return Fade(1, 0);
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += MyTime.deltaTime;
            float a = Mathf.Lerp(from, to, t / fadeDuration);

            Color c = fade.color;
            c.a = a;
            fade.color = c;

            yield return null;
        }
    }
}
