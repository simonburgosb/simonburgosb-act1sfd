using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ApiManager : MonoBehaviour
{
    // ============================================================
    // API FALSA - OBTENER JUGADORES
    // ============================================================

    public IEnumerator GetPlayers(
        string url,
        Action<string> onSuccess,
        Action<string> onError)
    {
        Debug.Log("Consultando API falsa:");
        Debug.Log(url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 15;

            yield return request.SendWebRequest();

            Debug.Log("Resultado: " + request.result);
            Debug.Log("Código HTTP: " + request.responseCode);

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;

                Debug.Log("JSON recibido:");
                Debug.Log(json);

                onSuccess?.Invoke(json);
            }
            else
            {
                Debug.LogError(
                    "Error HTTP: " + request.error
                );

                onError?.Invoke(request.error);
            }
        }
    }


    // ============================================================
    // API DE TERCEROS
    // CREAR MAZO CON LAS CARTAS ESPECÍFICAS
    // ============================================================

    public IEnumerator CreatePartialDeck(
        string cardCodes,
        Action<string> onSuccess,
        Action<string> onError)
    {
        string url =
            "https://deckofcardsapi.com/api/deck/new/shuffle/?cards="
            + cardCodes;

        Debug.Log("Consultando API de cartas:");
        Debug.Log(url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 15;

            yield return request.SendWebRequest();

            Debug.Log("Resultado API cartas: " + request.result);
            Debug.Log("Código HTTP: " + request.responseCode);

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;

                Debug.Log("Respuesta API de cartas:");
                Debug.Log(json);

                onSuccess?.Invoke(json);
            }
            else
            {
                Debug.LogError(
                    "Error API de cartas: " +
                    request.error
                );

                onError?.Invoke(request.error);
            }
        }
    }

    // ============================================================
    // OBTENER LAS CARTAS DEL MAZO
    // ============================================================

    public IEnumerator DrawCards(
        string deckId,
        int amount,
        Action<string> onSuccess,
        Action<string> onError)
    {
        string url =
            "https://deckofcardsapi.com/api/deck/"
            + deckId
            + "/draw/?count="
            + amount;

        Debug.Log("Obteniendo cartas:");
        Debug.Log(url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 15;

            yield return request.SendWebRequest();

            Debug.Log("Resultado Draw: " + request.result);
            Debug.Log("Código HTTP: " + request.responseCode);

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;

                Debug.Log("Cartas recibidas:");
                Debug.Log(json);

                onSuccess?.Invoke(json);
            }
            else
            {
                Debug.LogError(
                    "Error obteniendo cartas: " +
                    request.error
                );

                onError?.Invoke(request.error);
            }
        }
    }
}