using UnityEngine;
using TMPro; // Necesario para usar TextMeshPro

public class ValidadorFormulario : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputCodigoSala; // Arrastra aquí tu InputField de nombre
    public TMP_InputField inputNombre; // Arrastra aquí tu InputField de contraseña
    public TMP_InputField inputApellido;
    public TMP_InputField inputEdad;
    public TextMeshProUGUI textoError; // El texto que mostrará la advertencia
    public TextMeshProUGUI textoErrorInfo;
    public TextMeshProUGUI textoDelCodigoSala;

    [Header("Referencias Extra")]
    public FadeInBoth scriptDePantallas;

    void Start()
    {
        textoError.gameObject.SetActive(false);
        textoErrorInfo.gameObject.SetActive(false);

        // Suscribirse al evento "onSelect" de los inputs para ocultar el error
        // cuando el usuario intente corregirlo.
        inputNombre.onSelect.AddListener(delegate { OcultarError(); });
        inputCodigoSala.onSelect.AddListener(delegate { OcultarError(); });
        inputApellido.onSelect.AddListener(delegate { OcultarError(); });
        inputEdad.onSelect.AddListener(delegate { OcultarError(); });
    }

    // Esta función la llamaremos desde el Botón
    public void VerificarInputs()
    {
        // Verificamos si alguno de los campos está vacío o solo tiene espacios
        if (string.IsNullOrWhiteSpace(inputNombre.text) || string.IsNullOrWhiteSpace(inputApellido.text) || string.IsNullOrWhiteSpace(inputEdad.text))
        {
            MostrarError("Por favor, completa todos los campos.");
        }
        else
        {
            // Si todo está correcto, procedemos
            textoErrorInfo.gameObject.SetActive(false);
            LogicaExitosa();
        }
    }

    public void VerificarInputsSala(){

        if(string.IsNullOrWhiteSpace(inputCodigoSala.text))
        {
            MostrarError("Por favor, completa todos los campos.");
        }
        else
        {
            // Si todo está correcto, procedemos
            textoError.gameObject.SetActive(false);
            LogicaExitosaSala();
        }
    }

    void MostrarError(string mensaje)
    {
        textoError.text = mensaje;
        textoErrorInfo.text = mensaje;
        textoErrorInfo.gameObject.SetActive(true); // Hacemos visible el mensaje
        textoError.gameObject.SetActive(true); // Hacemos visible el mensaje
        textoDelCodigoSala.gameObject.SetActive(false);
    }

    void LogicaExitosa()
    {
        Debug.Log("Formulario enviado correctamente");
        scriptDePantallas.IrAlCanvasCodigo();
    }

    void LogicaExitosaSala()
    {
        Debug.Log("Formulario enviado correctamente");
        scriptDePantallas.IrAlCanvasPersonaje();
    }

    void OcultarError()
    {
        textoError.gameObject.SetActive(false);
        textoErrorInfo.gameObject.SetActive(false);
        textoDelCodigoSala.gameObject.SetActive(true);
    }
}