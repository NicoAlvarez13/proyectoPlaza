using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CuentaRegresiva : MonoBehaviour
{
    public Text cuentaregresivaText;
    public float delayBeforeStart = 1f;

    // --- ¡CAMBIO 1! ---
    // Borramos la referencia a "primerapreguntaCanvas"
    // public GameObject primerapreguntaCanvas; // ESTA LÍNEA SE VA

    // Y la reemplazamos por una referencia a nuestro "cerebro"
    [Header("Conexión")]
    public PreguntasManager quizManager; // Arrastra tu objeto QuizManager aquí


    // Start y Update están vacíos, los omito por espacio...
    void Start() { }
    void Update() { }

    private void OnEnable()
    {
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        yield return new WaitForSeconds(delayBeforeStart);

        for (int i = 3; i > 0; i--)
        {
            cuentaregresivaText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        cuentaregresivaText.text = "¡Comienza el juego!"; // (Corregí el '?' por '¡' )
        yield return new WaitForSeconds(1f);

        // --- ¡CAMBIO 2! ---
        // En lugar de activar un canvas específico...
        // primerapreguntaCanvas.SetActive(true); // ESTA LÍNEA SE VA

        // ...le decimos al QuizManager que empiece el juego.
        quizManager.StartQuiz();

        // Oculta este canvas (el del conteo)
        gameObject.SetActive(false);
    }
}
