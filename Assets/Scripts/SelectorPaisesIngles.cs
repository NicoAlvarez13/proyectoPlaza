using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq; // Ayuda a ordenar la lista

public class SelectorPaisesIngles : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_Dropdown dropdownPaises;

    void Start()
    {
        CargarPaises();
    }

    void CargarPaises()
    {
        // 1. Limpiamos las opciones que trae el dropdown por defecto (Option A, Option B...)
        dropdownPaises.ClearOptions();

        // 2. Definimos la lista de países (Aquí he puesto una lista resumida, puedes agregar más)
        List<string> listaPaises = new List<string>()
        {
            "Argentina", "Bolivia", "Brazil", "Chile", "Colombia",
            "Costa Rica", "Cuba", "Ecuador", "El Salvador", "Spain",
            "United States", "Guatemala", "Honduras", "Mexico",
            "Nicaragua", "Panama", "Paraguay", "Peru", "Puerto Rico",
            "Dominican Republic", "Uruguay", "Venezuela",
            "Germany", "France", "Italy", "United Kingdom", "Japan", "China", "Rest of the World"
            // Puedes buscar en internet "List of countries array C#" y pegar el resto aquí
        };

        // 3. Ordenamos alfabéticamente para facilitar la búsqueda al usuario
        listaPaises.Sort();

        List<TMP_Dropdown.OptionData> opcionesConBandera = new List<TMP_Dropdown.OptionData>();

        foreach (string nombrePais in listaPaises)
        {
            // 3. Cargamos la imagen desde la carpeta: Assets/Resources/Flags/NombrePais
            // Nota: Resources.Load NO lleva la extensión del archivo (.png)
            Sprite banderaSprite = Resources.Load<Sprite>("Flags/" + nombrePais);

            if (banderaSprite == null)
            {
                Debug.LogWarning("No flag was found for: " + nombrePais);
            }

            // 4. Creamos la opción combinando Texto + Imagen
            TMP_Dropdown.OptionData nuevaOpcion = new TMP_Dropdown.OptionData(nombrePais, banderaSprite);
            opcionesConBandera.Add(nuevaOpcion);
        }

        // 5. Agregamos la opción por defecto al principio
        // Puedes poner una imagen genérica de 'mundo' o null
        Sprite mundoSprite = Resources.Load<Sprite>("Flags/World");
        opcionesConBandera.Insert(0, new TMP_Dropdown.OptionData("Select your country...", mundoSprite));

        // 6. Inyectamos la lista compleja al Dropdown
        dropdownPaises.AddOptions(opcionesConBandera);
    }

    // Método público para obtener el país elegido desde otro script (ej: al guardar la cuenta)
    public string ObtenerPaisSeleccionado()
    {
        // Si el índice es 0, es que no eligió nada (o eligió la instrucción)
        if (dropdownPaises.value == 0)
        {
            return null; // O devuelve string vacía ""
        }

        // Retorna el texto de la opción seleccionada
        return dropdownPaises.options[dropdownPaises.value].text;
    }
}
