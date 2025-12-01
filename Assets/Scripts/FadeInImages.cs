using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInImages : MonoBehaviour
{
    public CanvasGroup image1;
    public CanvasGroup image2;

    public Transform image1Transform;
    public Transform image2Transform;

    public float fadeDuration = 2f;
    public float startScale = 0.8f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FadeInBoth());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator FadeInBoth()
    {
        float elapsed = 0f;

        // Inicializar
        image1.alpha = 0f;
        image2.alpha = 0f;

        image1Transform.localScale = Vector3.one * startScale;
        image2Transform.localScale = Vector3.one * startScale;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            // Interpolación suave
            float alpha = Mathf.Lerp(0f, 1f, t);
            float scale = Mathf.Lerp(startScale, 1f, t);

            image1.alpha = alpha;
            image2.alpha = alpha;

            image1Transform.localScale = Vector3.one * scale;
            image2Transform.localScale = Vector3.one * scale;

            yield return null;
        }

        // Finalizar en valores exactos
        image1.alpha = 1f;
        image2.alpha = 1f;
        image1Transform.localScale = Vector3.one;
        image2Transform.localScale = Vector3.one;
    }
}
