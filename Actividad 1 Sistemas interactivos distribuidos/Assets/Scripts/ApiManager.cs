using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ApiManager : MonoBehaviour
{
    // ============================================================
    // API FALSA - MY JSON SERVER
    // ============================================================

    public IEnumerator GetPlayers(
        string url,
        Action<string> onSuccess,
        Action<string> onError)
    {
        Debug.Log("Consultando API falsa:");
        Debug.Log(url);

        using (UnityWebRequest request =
               UnityWebRequest.Get(url))
        {
            request.timeout = 15;

            yield return request.SendWebRequest();

            Debug.Log("Resultado: " + request.result);
            Debug.Log("Código HTTP: " + request.responseCode);

            if (request.result ==
                UnityWebRequest.Result.Success)
            {
                string json =
                    request.downloadHandler.text;

                Debug.Log("Jugadores recibidos:");
                Debug.Log(json);

                onSuccess?.Invoke(json);
            }
            else
            {
                Debug.LogError(
                    "Error API falsa: " +
                    request.error
                );

                onError?.Invoke(request.error);
            }
        }
    }


    public IEnumerator GetCharacters(
        int[] characterIds,
        Action<string> onSuccess,
        Action<string> onError)
    {
        string url =
            "https://rickandmortyapi.com/graphql";


        // ============================================================
        // CREAR LISTA DE IDs
        // ============================================================

        string ids = "";

        for (int i = 0; i < characterIds.Length; i++)
        {
            if (i > 0)
            {
                ids += ",";
            }

            ids += characterIds[i];
        }


        // ============================================================
        // QUERY GRAPHQL
        // ============================================================

        string query =
            "query GetCharacters($ids: [ID!]!) " +
            "{ " +
            "charactersByIds(ids: $ids) " +
            "{ " +
            "id " +
            "name " +
            "image " +
            "} " +
            "}";


        // ============================================================
        // JSON
        // ============================================================

        string jsonBody =
            "{"
            + "\"query\":\""
            + query.Replace("\"", "\\\"")
            + "\","
            + "\"variables\":{"
            + "\"ids\":["
            + ids
            + "]"
            + "}"
            + "}";


        byte[] bodyRaw =
            System.Text.Encoding.UTF8.GetBytes(
                jsonBody
            );


        // ============================================================
        // INTENTAR HASTA 3 VECES
        // ============================================================

        int maxAttempts = 3;


        for (
            int attempt = 1;
            attempt <= maxAttempts;
            attempt++
        )
        {
            Debug.Log(
                "Consulta GraphQL. Intento " +
                attempt +
                "/" +
                maxAttempts
            );


            using (UnityWebRequest request =
                   new UnityWebRequest(
                       url,
                       "POST"))
            {
                request.uploadHandler =
                    new UploadHandlerRaw(bodyRaw);

                request.downloadHandler =
                    new DownloadHandlerBuffer();


                request.SetRequestHeader(
                    "Content-Type",
                    "application/json"
                );


                request.timeout = 30;


                yield return request.SendWebRequest();


                Debug.Log(
                    "Resultado GraphQL: " +
                    request.result
                );

                Debug.Log(
                    "Código HTTP: " +
                    request.responseCode
                );


                // ====================================================
                // PETICIÓN EXITOSA
                // ====================================================

                if (
                    request.result ==
                    UnityWebRequest.Result.Success
                )
                {
                    string response =
                        request.downloadHandler.text;


                    Debug.Log(
                        "Respuesta GraphQL:"
                    );

                    Debug.Log(response);


                    onSuccess?.Invoke(
                        response
                    );


                    yield break;
                }


                // ====================================================
                // ERROR 429
                // ====================================================

                if (
                    request.responseCode == 429
                )
                {
                    Debug.LogWarning(
                        "Rick and Morty API está " +
                        "limitando las peticiones."
                    );


                    if (attempt < maxAttempts)
                    {
                        Debug.Log(
                            "Esperando 5 segundos " +
                            "antes de volver a intentar..."
                        );


                        yield return new WaitForSeconds(
                            5f
                        );
                    }
                    else
                    {
                        onError?.Invoke(
                            "La API está temporalmente " +
                            "limitando las peticiones. " +
                            "Espera unos segundos y vuelve " +
                            "a ejecutar el proyecto."
                        );
                    }
                }
                else
                {
                    // =================================================
                    // OTRO ERROR
                    // =================================================

                    Debug.LogError(
                        "Error GraphQL: " +
                        request.error
                    );


                    onError?.Invoke(
                        request.error
                    );


                    yield break;
                }
            }
        }
    }


    // ============================================================
    // DESCARGAR IMAGEN
    // ============================================================

    public IEnumerator DownloadImage(
        string imageUrl,
        Action<Texture2D> onSuccess,
        Action<string> onError)
    {
        Debug.Log(
            "Descargando imagen:"
        );

        Debug.Log(imageUrl);


        using (UnityWebRequest request =
               UnityWebRequestTexture.GetTexture(
                   imageUrl))
        {
            request.timeout = 15;

            yield return request.SendWebRequest();


            if (request.result ==
                UnityWebRequest.Result.Success)
            {
                Texture2D texture =
                    DownloadHandlerTexture
                    .GetContent(request);

                onSuccess?.Invoke(texture);
            }
            else
            {
                Debug.LogError(
                    "Error descargando imagen: " +
                    request.error
                );

                onError?.Invoke(
                    request.error
                );
            }
        }
    }
}