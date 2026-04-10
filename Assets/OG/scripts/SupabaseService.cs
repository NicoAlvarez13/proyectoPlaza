using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class SupabaseService : MonoBehaviour
{
    public static SupabaseService Instance { get; private set; }

    private const string SUPABASE_URL = "https://tsgzlcpfuoqscoipudbx.supabase.co";
    private const string ANON_KEY     = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InRzZ3psY3BmdW9xc2NvaXB1ZGJ4Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzU4MDAxOTQsImV4cCI6MjA5MTM3NjE5NH0.nx_GbDxXU58MPJhEU-6_B3p6EA0_SHuTy91dgN5Y5t0";
    private const string TABLE        = "Jugadores";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InsertJugador(string usuario, string nombre, string apellido, int edad, string pais, int puntos,
                               Action onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(PostJugador(usuario, nombre, apellido, edad, pais, puntos, onSuccess, onError));
    }

    private IEnumerator PostJugador(string usuario, string nombre, string apellido, int edad, string pais, int puntos,
                                     Action onSuccess, Action<string> onError)
    {
        string json = $"{{" +
                      $"\"usuario\":\"{EscapeJson(usuario)}\"," +
                      $"\"nombre\":\"{EscapeJson(nombre)}\"," +
                      $"\"apellido\":\"{EscapeJson(apellido)}\"," +
                      $"\"edad\":{edad}," +
                      $"\"pais\":\"{EscapeJson(pais)}\"," +
                      $"\"puntos\":{puntos}" +
                      $"}}";

        string endpoint = $"{SUPABASE_URL}/rest/v1/{TABLE}";
        byte[] body = Encoding.UTF8.GetBytes(json);

        using var request = new UnityWebRequest(endpoint, "POST");
        request.uploadHandler   = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("apikey",        ANON_KEY);
        request.SetRequestHeader("Authorization", "Bearer " + ANON_KEY);
        request.SetRequestHeader("Content-Type",  "application/json");
        request.SetRequestHeader("Prefer",        "return=minimal");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("[Supabase] Insert OK");
            onSuccess?.Invoke();
        }
        else
        {
            string err = request.downloadHandler.text;
            Debug.LogError($"[Supabase] Error {request.responseCode}: {err}");
            onError?.Invoke(err);
        }
    }

    private static string EscapeJson(string s) =>
        s?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? "";
}
