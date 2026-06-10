using System.Collections;
using UnityEngine;

public class PlayerFadeController : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Animator animator;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private bool disableAfterFade = true;
    [SerializeField] private bool disableAnimatorOnFadeOut = true;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void FadeOut()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        if (disableAnimatorOnFadeOut && animator != null)
            animator.enabled = false;

        fadeRoutine = StartCoroutine(FadeRoutine(1f, 0f));
    }

    public void FadeIn()
    {
        gameObject.SetActive(true);
        SetRenderersVisible(true);

        if (animator != null)
            animator.enabled = true;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(0f, 1f));
    }

    private IEnumerator FadeRoutine(float from, float to)
    {
        float elapsed = 0f;

        SetRenderersVisible(true);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float alpha = Mathf.Lerp(from, to, t);

            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(to);

        if (to <= 0f && disableAfterFade)
            SetRenderersVisible(false);

        fadeRoutine = null;
    }

    private void SetAlpha(float alpha)
    {
        foreach (Renderer r in renderers)
        {
            if (r == null) continue;

            foreach (Material mat in r.materials)
            {
                Color color = mat.color;
                color.a = alpha;
                mat.color = color;
            }
        }
    }

    private void SetRenderersVisible(bool visible)
    {
        foreach (Renderer r in renderers)
        {
            if (r != null)
                r.enabled = visible;
        }
    }
}