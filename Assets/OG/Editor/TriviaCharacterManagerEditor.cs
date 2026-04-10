#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Dibuja en la Scene View los rectángulos de cada personaje sobre un canvas de referencia 1080x1920.
/// Activá el modo 2D en la Scene View y seleccioná el CharacterManager para verlos.
/// </summary>
[CustomEditor(typeof(TriviaCharacterManager))]
public class TriviaCharacterManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "► Abrí la Scene View en modo 2D (botón '2D' arriba a la izquierda).\n" +
            "► Seleccioná el GameObject CharacterManager en la Hierarchy.\n" +
            "► Los rectángulos de cada personaje aparecen sobre el canvas 1080×1920 (borde blanco).\n" +
            "► Modificá Left / Top / Width / Height en el Inspector de cada personaje y se actualizan en tiempo real.",
            MessageType.Info);
    }

    // Dibuja gizmos aunque el objeto NO esté seleccionado (NonSelected) para que siempre sean visibles
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    static void DrawGizmos(TriviaCharacterManager manager, GizmoType gizmoType)
    {
        const float REF_W = 1080f;
        const float REF_H = 1920f;
        const float SCALE = 0.01f; // 1px = 0.01 unidades de mundo

        // Marco del canvas de referencia (blanco)
        Gizmos.color = new Color(1f, 1f, 1f, 0.35f);
        Gizmos.DrawWireCube(Vector3.zero,
            new Vector3(REF_W * SCALE, REF_H * SCALE, 0.001f));

        // Accedemos al array serializado _characters
        SerializedObject so = new SerializedObject(manager);
        SerializedProperty charsProp = so.FindProperty("_characters");
        if (charsProp == null || !charsProp.isArray) return;

        Color[] palette =
        {
            new Color(1f,  0.3f, 0.3f, 0.9f),   // rojo   – Escalador
            new Color(0.3f, 1f,  0.5f, 0.9f),   // verde  – Buzeador
            new Color(0.3f, 0.8f, 1f,  0.9f),   // cyan   – Astronauta
        };

        for (int i = 0; i < charsProp.arraySize; i++)
        {
            var objRef = charsProp.GetArrayElementAtIndex(i).objectReferenceValue as TriviaCharacter;
            if (objRef == null) continue;

            SerializedObject cso = new SerializedObject(objRef);
            float left   = cso.FindProperty("_left").floatValue;
            float top    = cso.FindProperty("_top").floatValue;
            float width  = cso.FindProperty("_width").floatValue;
            float height = cso.FindProperty("_height").floatValue;

            // UI Toolkit: origen top-left, Y hacia abajo
            // World space: origen centro del canvas, Y hacia arriba
            float cx = (left  + width  * 0.5f - REF_W * 0.5f) * SCALE;
            float cy = (REF_H * 0.5f - top - height * 0.5f)   * SCALE;

            Color col = palette[i % palette.Length];

            // Relleno semitransparente
            Gizmos.color = new Color(col.r, col.g, col.b, 0.15f);
            Gizmos.DrawCube(new Vector3(cx, cy, 0f),
                new Vector3(width * SCALE, height * SCALE, 0.001f));

            // Borde sólido
            Gizmos.color = col;
            Gizmos.DrawWireCube(new Vector3(cx, cy, 0f),
                new Vector3(width * SCALE, height * SCALE, 0.001f));

            // Etiqueta con el nombre
            GUIStyle labelStyle = new GUIStyle
            {
                normal    = { textColor = col },
                fontStyle = FontStyle.Bold,
                fontSize  = 14
            };
            Handles.Label(
                new Vector3(cx - width * SCALE * 0.5f,
                            cy + height * SCALE * 0.5f + 0.05f, 0f),
                objRef.gameObject.name,
                labelStyle);
        }
    }
}
#endif
