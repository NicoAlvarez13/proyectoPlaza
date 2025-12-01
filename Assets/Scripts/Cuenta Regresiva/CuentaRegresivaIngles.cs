using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CuentaRegresivaIngles : MonoBehaviour
{
    public Text cuentaregresivaText;
    public float delayBeforeStart = 1f;

    [Header("Conexión")]
    public PreguntasManager quizManager;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

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

        cuentaregresivaText.text = "The game begins!"; // (Corregí el '?' por '¡' )
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
