using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInBoth : MonoBehaviour
{
    public GameObject canvasIntro;
    public GameObject canvasIdiomas;
    public GameObject canvasDatos;
    public GameObject canvasDatosIngles;
    public GameObject canvasCodigo;
    public GameObject canvasCodigoIngles;
    public GameObject canvasPersonaje;
    public GameObject canvasPersonajeIngles;
    public GameObject canvasConteo;
    public GameObject canvasConteoIngles;

    public CanvasGroup image1;
    public CanvasGroup image2;

    public Transform image1Transform;
    public Transform image2Transform;

    public float fadeDuration = 2f;
    public float zoomStartFactor = 0.8f; // 80% del tamaño original

    private Vector3 originalScale1;
    private Vector3 originalScale2;

    void Start()
    {
        // Guardamos la escala original
        originalScale1 = image1Transform.localScale;
        originalScale2 = image2Transform.localScale;

        StartCoroutine(FadeAndZoomIn());

        canvasIntro.SetActive(true);
        canvasIdiomas.SetActive(false);
        canvasDatos.SetActive(false);
        canvasDatosIngles.SetActive(false);
        canvasCodigo.SetActive(false);
        canvasCodigoIngles.SetActive(false);
        canvasPersonaje.SetActive(false);
        
        StartCoroutine(CambiarInterfaz());
    }

    System.Collections.IEnumerator CambiarInterfaz()
    {
        yield return new WaitForSeconds(3f);

        canvasIntro.SetActive(false);
        canvasIdiomas.SetActive(true);
    }

    public void IrAlCanvasDatos()
    {
        canvasIdiomas.SetActive(false);
        canvasDatos.SetActive(true);
    }

    public void IrAlCanvasDatosIngles()
    {
        canvasIdiomas.SetActive(false);
        canvasDatosIngles.SetActive(true);
    }

    public void IrAlCanvasCodigo()
    {
        canvasDatos.SetActive(false);
        canvasCodigo.SetActive(true);
    }

    public void IrAlCanvasCodigoIngles()
    {
        canvasDatosIngles.SetActive(false);
        canvasCodigoIngles.SetActive(true);
    }

    public void IrAlCanvasPersonaje()
    {
        canvasCodigo.SetActive(false);
        canvasPersonaje.SetActive(true);
    }

    public void IrAlCanvasPersonajeIngles()
    {
        canvasCodigoIngles.SetActive(false);
        canvasPersonajeIngles.SetActive(true);
    }

    public void IrAlCanvasConteo()
    {
        canvasPersonaje.SetActive(false);
        canvasConteo.SetActive(true);
    }

    public void IrAlCanvasConteoIngles()
    {
        canvasPersonajeIngles.SetActive(false);
        canvasConteoIngles.SetActive(true);
    }

    IEnumerator FadeAndZoomIn()
    {
        float elapsed = 0f;

        // Inicializamos: empezamos con alpha 0 y zoom más pequeño
        image1.alpha = 0f;
        image2.alpha = 0f;

        image1Transform.localScale = originalScale1 * zoomStartFactor;
        image2Transform.localScale = originalScale2 * zoomStartFactor;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            float alpha = Mathf.Lerp(0f, 1f, t);

            // Escala interpolada desde su tamaño reducido al original
            image1Transform.localScale = Vector3.Lerp(originalScale1 * zoomStartFactor, originalScale1, t);
            image2Transform.localScale = Vector3.Lerp(originalScale2 * zoomStartFactor, originalScale2, t);

            image1.alpha = alpha;
            image2.alpha = alpha;

            yield return null;
        }

        // Finalizamos con valores precisos
        image1.alpha = 1f;
        image2.alpha = 1f;

        image1Transform.localScale = originalScale1;
        image2Transform.localScale = originalScale2;
    }
}