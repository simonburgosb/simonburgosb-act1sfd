using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("MANAGERS")]
    public ApiManager apiManager;
    public PlayerManager playerManager;


    [Header("API FALSA")]
    public string playersApiUrl;


    [Header("UI")]
    public TMP_Text playerNameText;
    public TMP_Text statusText;


    [Header("CARTAS")]
    public Transform cardsContainer;
    public GameObject cardPrefab;


    private string deckId;


    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        StartCoroutine(StartGame());
    }


    // ============================================================
    // INICIAR
    // ============================================================

    private IEnumerator StartGame()
    {
        ShowStatus("Conectando con la API...");

        yield return StartCoroutine(
            apiManager.GetPlayers(
                playersApiUrl,
                OnPlayersReceived,
                OnApiError
            )
        );
    }


    // ============================================================
    // RECIBIR JUGADORES
    // ============================================================

    private void OnPlayersReceived(string json)
    {
        playerManager.LoadPlayersFromJson(json);

        Player player =
            playerManager.GetCurrentPlayer();

        if (player == null)
        {
            ShowStatus("No se encontraron jugadores.");
            return;
        }

        if (playerNameText != null)
        {
            playerNameText.text =
                "Jugador: " + player.name;
        }

        LoadCurrentPlayer();
    }


    // ============================================================
    // CARGAR JUGADOR
    // ============================================================

    public void LoadCurrentPlayer()
    {
        ClearCards();

        Player player =
            playerManager.GetCurrentPlayer();

        if (player == null)
        {
            ShowStatus("No hay jugador seleccionado.");
            return;
        }

        if (playerNameText != null)
        {
            playerNameText.text =
                "Jugador: " + player.name;
        }

        if (player.cards == null ||
            player.cards.Length == 0)
        {
            ShowStatus(
                "El jugador no tiene cartas."
            );

            return;
        }

        ShowStatus(
            "Consultando cartas de " +
            player.name + "..."
        );

        StartCoroutine(
            LoadPlayerCards(player.cards)
        );
    }


    // ============================================================
    // CONVERTIR IDS Y CONSULTAR API EXTERNA
    // ============================================================

    private IEnumerator LoadPlayerCards(
        int[] cardIds)
    {
        string cardCodes = "";

        for (int i = 0; i < cardIds.Length; i++)
        {
            string code =
                ConvertIdToCardCode(cardIds[i]);

            if (string.IsNullOrEmpty(code))
            {
                Debug.LogWarning(
                    "ID de carta inválido: " +
                    cardIds[i]
                );

                continue;
            }

            if (cardCodes != "")
            {
                cardCodes += ",";
            }

            cardCodes += code;
        }

        Debug.Log(
            "IDs convertidos a códigos: " +
            cardCodes
        );

        ShowStatus(
            "Consultando API externa..."
        );

        // Crear un mazo que contenga exactamente
        // las cartas del jugador.

        yield return StartCoroutine(
            apiManager.CreatePartialDeck(
                cardCodes,
                OnDeckCreated,
                OnApiError
            )
        );
    }


    // ============================================================
    // MAZO CREADO
    // ============================================================

    private void OnDeckCreated(string json)
    {
        DeckData deckData;

        try
        {
            deckData =
                JsonUtility.FromJson<DeckData>(json);
        }
        catch (Exception error)
        {
            Debug.LogError(
                "Error procesando mazo: " +
                error.Message
            );

            ShowStatus(
                "Error procesando API de cartas."
            );

            return;
        }

        if (deckData == null ||
            string.IsNullOrEmpty(deckData.deck_id))
        {
            ShowStatus(
                "No se pudo crear el mazo."
            );

            return;
        }

        deckId = deckData.deck_id;

        Debug.Log(
            "Mazo creado: " + deckId
        );

        ShowStatus(
            "Obteniendo información de las cartas..."
        );

        // Ahora sacamos todas las cartas.

        Player player =
            playerManager.GetCurrentPlayer();

        StartCoroutine(
            apiManager.DrawCards(
                deckId,
                player.cards.Length,
                OnCardsReceived,
                OnApiError
            )
        );
    }


    // ============================================================
    // RECIBIR CARTAS
    // ============================================================

    private void OnCardsReceived(string json)
    {
        CardResponse response;

        try
        {
            response =
                JsonUtility.FromJson<CardResponse>(json);
        }
        catch (Exception error)
        {
            Debug.LogError(
                "Error procesando cartas: " +
                error.Message
            );

            ShowStatus(
                "Error procesando las cartas."
            );

            return;
        }

        if (response == null ||
            response.cards == null ||
            response.cards.Length == 0)
        {
            Debug.LogError(
                "La API no devolvió cartas."
            );

            ShowStatus(
                "No se recibieron cartas."
            );

            return;
        }


        Debug.Log(
            "Cartas recibidas: " +
            response.cards.Length
        );


        // Crear las cartas visualmente.

        for (int i = 0;
             i < response.cards.Length;
             i++)
        {
            CardData card =
                response.cards[i];

            GameObject cardObject =
                CreateCard(
                    i + 1,
                    card.value,
                    card.suit
                );

            if (cardObject != null &&
                !string.IsNullOrEmpty(card.image))
            {
                StartCoroutine(
                    DownloadCardImage(
                        card.image,
                        cardObject
                    )
                );
            }
        }

        ShowStatus(
            "Baraja cargada correctamente."
        );
    }


    // ============================================================
    // CREAR CARTA
    // ============================================================

    private GameObject CreateCard(
        int id,
        string value,
        string suit)
    {
        if (cardPrefab == null)
        {
            Debug.LogError(
                "No asignaste Card Prefab."
            );

            return null;
        }

        if (cardsContainer == null)
        {
            Debug.LogError(
                "No asignaste Cards Container."
            );

            return null;
        }

        GameObject newCard =
            Instantiate(
                cardPrefab,
                cardsContainer
            );

        CardUI cardUI =
            newCard.GetComponent<CardUI>();

        if (cardUI != null)
        {
            cardUI.SetCard(
                id,
                value,
                TranslateSuit(suit)
            );
        }

        return newCard;
    }


    // ============================================================
    // DESCARGAR IMAGEN
    // ============================================================

    private IEnumerator DownloadCardImage(
        string imageUrl,
        GameObject cardObject)
    {
        Debug.Log(
            "Descargando imagen: " +
            imageUrl
        );

        using (
            UnityWebRequest request =
            UnityWebRequestTexture.GetTexture(
                imageUrl
            )
        )
        {
            yield return request.SendWebRequest();

            if (
                request.result ==
                UnityWebRequest.Result.Success
            )
            {
                Texture2D texture =
                    DownloadHandlerTexture
                    .GetContent(request);

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

                CardUI cardUI =
                    cardObject.GetComponent<CardUI>();

                if (cardUI != null)
                {
                    cardUI.SetImage(sprite);
                }
            }
            else
            {
                Debug.LogError(
                    "Error descargando imagen: " +
                    request.error
                );
            }
        }
    }


    // ============================================================
    // ID → CÓDIGO DE CARTA
    // ============================================================

    private string ConvertIdToCardCode(int id)
    {
        if (id < 1 || id > 52)
        {
            return "";
        }

        string[] suits =
        {
            "S",
            "H",
            "D",
            "C"
        };

        string[] values =
        {
            "A",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "0",
            "J",
            "Q",
            "K"
        };

        int suitIndex =
            (id - 1) / 13;

        int valueIndex =
            (id - 1) % 13;

        return
            values[valueIndex]
            + suits[suitIndex];
    }


    // ============================================================
    // TRADUCIR PALO
    // ============================================================

    private string TranslateSuit(string suit)
    {
        switch (suit)
        {
            case "SPADES":
                return "PICAS";

            case "HEARTS":
                return "CORAZONES";

            case "DIAMONDS":
                return "DIAMANTES";

            case "CLUBS":
                return "TREBOLES";

            default:
                return suit;
        }
    }


    // ============================================================
    // SIGUIENTE USUARIO
    // ============================================================

    public void NextUser()
    {
        playerManager.NextPlayer();

        LoadCurrentPlayer();
    }


    // ============================================================
    // USUARIO ANTERIOR
    // ============================================================

    public void PreviousUser()
    {
        playerManager.PreviousPlayer();

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
            int i = cardsContainer.childCount - 1;
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

    private void ShowStatus(string message)
    {
        Debug.Log(message);

        if (statusText != null)
        {
            statusText.text = message;
        }
    }


    // ============================================================
    // ERROR
    // ============================================================

    private void OnApiError(string error)
    {
        ShowStatus(
            "Error de conexión: " + error
        );

        Debug.LogError(
            "API ERROR: " + error
        );
    }
}


// =================================================================
// MODELOS JSON
// =================================================================

[Serializable]
public class DeckData
{
    public bool success;
    public string deck_id;
    public bool shuffled;
    public int remaining;
}


[Serializable]
public class CardResponse
{
    public bool success;
    public string deck_id;
    public CardData[] cards;
    public int remaining;
}


[Serializable]
public class CardData
{
    public string image;
    public string value;
    public string suit;
    public string code;
}