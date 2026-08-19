using System;
using UnityEngine;

[Serializable]
public class Player
{
    public int id;
    public string name;
    public int[] cards;
}

[Serializable]
public class PlayerList
{
    public Player[] players;
}

public class PlayerManager : MonoBehaviour
{
    public Player[] players;

    private int currentPlayerIndex = 0;


    public void LoadPlayersFromJson(string json)
    {
        try
        {
            Debug.Log(
                "JSON recibido:"
            );

            Debug.Log(json);


            // My JSON Server devuelve directamente
            // el array de jugadores.

            string wrappedJson =
                "{\"players\":" +
                json +
                "}";


            PlayerList playerList =
                JsonUtility.FromJson<PlayerList>(
                    wrappedJson
                );


            if (playerList == null ||
                playerList.players == null)
            {
                Debug.LogError(
                    "No se pudieron cargar los jugadores."
                );

                return;
            }


            players =
                playerList.players;


            currentPlayerIndex = 0;


            Debug.Log(
                "Jugadores cargados: " +
                players.Length
            );
        }
        catch (Exception error)
        {
            Debug.LogError(
                "Error procesando jugadores: " +
                error.Message
            );
        }
    }


    public Player GetCurrentPlayer()
    {
        if (players == null ||
            players.Length == 0)
        {
            return null;
        }

        return players[
            currentPlayerIndex
        ];
    }


    public Player NextPlayer()
    {
        if (players == null ||
            players.Length == 0)
        {
            return null;
        }


        currentPlayerIndex++;


        if (currentPlayerIndex >=
            players.Length)
        {
            currentPlayerIndex = 0;
        }


        return GetCurrentPlayer();
    }


    public Player PreviousPlayer()
    {
        if (players == null ||
            players.Length == 0)
        {
            return null;
        }


        currentPlayerIndex--;


        if (currentPlayerIndex < 0)
        {
            currentPlayerIndex =
                players.Length - 1;
        }


        return GetCurrentPlayer();
    }
}