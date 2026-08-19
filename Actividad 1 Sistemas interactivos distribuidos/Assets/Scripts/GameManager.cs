using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("MANAGERS")]

    public ApiManager apiManager;

    public PlayerManager playerManager;


    [Header("API FALSA")]

    public string playersApiUrl =
        "https://my-json-server.typicode.com/simonburgosb/simonburgosb-act1sfd/players";


    [Header("UI")]

    public TMP_Text playerNameText;

    public TMP_Text statusText;


    [Header("CARTAS")]

    public Transform cardsContainer;

    public GameObject cardPrefab;


    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        StartCoroutine(
            LoadPlayers()
        );
    }


    // ============================================================
    // CARGAR JUGADORES
    // ============================================================

    private IEnumerator LoadPlayers()
    {
        ShowStatus(
            "Cargando jugadores..."
        );


        yield return StartCoroutine(
            apiManager.GetPlayers(
                playersApiUrl,
                OnPlayersReceived,
                OnApiError
            )
        );
    }


    // ============================================================
    // JUGADORES RECIBIDOS
    // ============================================================

    private void OnPlayersReceived(
        string json)
    {
        playerManager
            .LoadPlayersFromJson(json);


        Player player =
            playerManager
            .GetCurrentPlayer();


        if (player == null)
        {
            ShowStatus(
                "No se encontraron jugadores."
            );

            return;
        }


        LoadCurrentPlayer();
    }


    // ============================================================
    // CARGAR JUGADOR ACTUAL
    // ============================================================

    public void LoadCurrentPlayer()
    {
        ClearCards();


        Player player =
            playerManager
            .GetCurrentPlayer();


        if (player == null)
        {
            ShowStatus(
                "No hay jugador seleccionado."
            );

            return;
        }


        if (playerNameText != null)
        {
            playerNameText.text =
                "Jugador: " +
                player.name;
        }


        if (player.cards == null ||
            player.cards.Length == 0)
        {
            ShowStatus(
                "Este jugador no tiene cartas."
            );

            return;
        }


        ShowStatus(
            "Buscando personajes..."
        );


        StartCoroutine(
            LoadCharacters(
                player.cards
            )
        );
    }


    // ============================================================
    // CONSULTAR RICK AND MORTY
    // ============================================================

    private IEnumerator LoadCharacters(
        int[] cardIds)
    {
        yield return StartCoroutine(
            apiManager.GetCharacters(
                cardIds,
                OnCharactersReceived,
                OnApiError
            )
        );
    }


    // ============================================================
    // RECIBIR PERSONAJES
    // ============================================================

    private void OnCharactersReceived(
        string json)
    {
        Debug.Log(
            "Procesando personajes..."
        );


        GraphQLResponse response;


        try
        {
            response =
                JsonUtility.FromJson
                <GraphQLResponse>(
                    json
                );
        }
        catch (Exception error)
        {
            Debug.LogError(
                "Error procesando GraphQL: " +
                error.Message
            );

            ShowStatus(
                "Error procesando personajes."
            );

            return;
        }


        if (response == null ||
            response.data == null ||
            response.data.charactersByIds == null)
        {
            ShowStatus(
                "No se recibieron personajes."
            );

            return;
        }


        CharacterData[] characters =
            response.data.charactersByIds;


        Debug.Log(
            "Personajes recibidos: " +
            characters.Length
        );


        // --------------------------------------------------------
        // Crear las cartas
        // --------------------------------------------------------

        for (int i = 0;
             i < characters.Length;
             i++)
        {
            CreateCard(
                characters[i]
            );
        }


        ShowStatus(
            "Baraja cargada correctamente."
        );
    }


    // ============================================================
    // CREAR CARTA
    // ============================================================

    private void CreateCard(
        CharacterData character)
    {
        if (cardPrefab == null)
        {
            Debug.LogError(
                "No asignaste Card Prefab."
            );

            return;
        }


        GameObject cardObject =
            Instantiate(
                cardPrefab,
                cardsContainer
            );


        CardUI cardUI =
            cardObject
            .GetComponent<CardUI>();


        if (cardUI != null)
        {
            cardUI.SetCard(
                character.id,
                character.name
            );
        }


        // Descargar imagen

        StartCoroutine(
            apiManager.DownloadImage(
                character.image,
                texture =>
                {
                    Sprite sprite =
                        Sprite.Create(
                            texture,
                            new Rect(
                                0,
                                0,
                                texture.width,
                                texture.height
                            ),
                            new Vector2(
                                0.5f,
                                0.5f
                            )
                        );


                    if (cardUI != null)
                    {
                        cardUI.SetImage(
                            sprite
                        );
                    }
                },
                error =>
                {
                    Debug.LogError(
                        "Error imagen: " +
                        error
                    );
                }
            )
        );
    }


    // ============================================================
    // SIGUIENTE USUARIO
    // ============================================================

    public void NextUser()
    {
        playerManager
            .NextPlayer();

        LoadCurrentPlayer();
    }


    // ============================================================
    // USUARIO ANTERIOR
    // ============================================================

    public void PreviousUser()
    {
        playerManager
            .PreviousPlayer();

        LoadCurrentPlayer();
    }


    // ============================================================
    // LIMPIAR CARTAS
    // ============================================================

    private void ClearCards()
    {
        if (cardsContainer == null)
        {
            return;
        }


        for (
            int i =
            cardsContainer.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                cardsContainer
                .GetChild(i)
                .gameObject
            );
        }
    }


    // ============================================================
    // ESTADO
    // ============================================================

    private void ShowStatus(
        string message)
    {
        Debug.Log(message);


        if (statusText != null)
        {
            statusText.text =
                message;
        }
    }


    // ============================================================
    // ERROR
    // ============================================================

    private void OnApiError(
        string error)
    {
        ShowStatus(
            "Error: " + error
        );
    }
}


// =================================================================
// MODELOS GRAPHQL
// =================================================================

[Serializable]
public class GraphQLResponse
{
    public GraphQLData data;
}


[Serializable]
public class GraphQLData
{
    public CharacterData[] charactersByIds;
}


[Serializable]
public class CharacterData
{
    public int id;

    public string name;

    public string image;
}