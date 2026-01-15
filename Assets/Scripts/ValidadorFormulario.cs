using UnityEngine;
using TMPro; // Necesario para usar TextMeshPro

public class ValidadorFormulario : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputCodigoSala; // Arrastra aquí tu InputField de nombre
    public TMP_InputField inputNombre; // Arrastra aquí tu InputField de contraseña
    public TMP_InputField inputApellido;
    public TMP_InputField inputEdad;
    
    [Header("UI References - Textos Error")]
    public TextMeshProUGUI textoError;
    public TextMeshProUGUI textoErrorInfo;
    public TextMeshProUGUI textoDelCodigoSala;

    [Header("Dropdown de Países")]
    public TMP_Dropdown dropdownPaises;

    [Header("Feedback General")]
    public GameObject cartelError; // El objeto visual del error

    [Header("Referencias Extra")]
    public FadeInBoth scriptDePantallas;

    void Start()
    {
        OcultarError();

        // Suscribirse para ocultar error al escribir
        inputNombre.onSelect.AddListener(delegate { OcultarError(); });
        inputCodigoSala.onSelect.AddListener(delegate { OcultarError(); });
        inputApellido.onSelect.AddListener(delegate { OcultarError(); });
        inputEdad.onSelect.AddListener(delegate { OcultarError(); });
        // También ocultamos error si tocan el dropdown
        // (Nota: onValueChanged requiere un parámetro, usamos lambda _ => )
        dropdownPaises.onValueChanged.AddListener(_ => OcultarError());
    }

    // Esta función la llamaremos desde el Botón
    public void VerificarInputs()
    {
        // 1. Validar Textos (Nombre, Apellido, Edad)
        bool textosCompletos = !string.IsNullOrWhiteSpace(inputNombre.text) &&
                               !string.IsNullOrWhiteSpace(inputApellido.text) &&
                               !string.IsNullOrWhiteSpace(inputEdad.text);

        // 2. Validar Dropdown (Debe ser mayor a 0 para no ser "Seleccione país...")
        bool paisValido = dropdownPaises.value > 0;

        // 3. Evaluación Final (AMBOS deben ser verdaderos)
        if (textosCompletos && paisValido)
        {
            // ÉXITO
            LogicaExitosa();
        }
        else
        {
            // ERROR
            MostrarError("Por favor, completa todos los campos.");

            // Debug opcional para que sepas qué faltó
            if (!textosCompletos) Debug.Log("Faltan textos");
            if (!paisValido) Debug.Log("Falta el país");
        }
    }

    // Esta función es para la OTRA pantalla (la del código de sala)
    public void VerificarInputsSala()
    {
        if (string.IsNullOrWhiteSpace(inputCodigoSala.text))
        {
            MostrarError("Por favor, ingresa el código.");
        }
        else
        {
            LogicaExitosaSala();
        }
    }

    void MostrarError(string mensaje)
    {
        // Actualizamos el texto
        if (textoError != null) textoError.text = mensaje;
        if (textoErrorInfo != null) textoErrorInfo.text = mensaje;

        // Activamos todos los objetos de error visuales
        if (textoErrorInfo != null) textoErrorInfo.gameObject.SetActive(true);
        if (textoError != null) textoError.gameObject.SetActive(true);
        if (cartelError != null) cartelError.SetActive(true); // <--- Agregado para consistencia

        if (textoDelCodigoSala != null) textoDelCodigoSala.gameObject.SetActive(false);
    }

    void OcultarError()
    {
        if (textoError != null) textoError.gameObject.SetActive(false);
        if (textoErrorInfo != null) textoErrorInfo.gameObject.SetActive(false);
        if (cartelError != null) cartelError.SetActive(false); // <--- Agregado

        if (textoDelCodigoSala != null) textoDelCodigoSala.gameObject.SetActive(true);
    }

    void LogicaExitosa()
    {
        Debug.Log("Formulario enviado correctamente");
        OcultarError(); // Limpiamos errores por si acaso
        scriptDePantallas.IrAlCanvasCodigo();
    }

    void LogicaExitosaSala()
    {
        Debug.Log("Sala verificada correctamente");
        OcultarError();
        scriptDePantallas.IrAlCanvasPersonaje();
    }
}