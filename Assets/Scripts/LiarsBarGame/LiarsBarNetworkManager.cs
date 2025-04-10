using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using Mirror.Examples.Pong;
using System.Collections.Generic;
using TMPro;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class LiarsBarNetworkManager : NetworkManager
{
    //intente usar este singleton, pero en vez de que todos referenciaban este script, cada jugador referenciaba su propio network manager
    public static new LiarsBarNetworkManager singleton => (LiarsBarNetworkManager)NetworkManager.singleton;

    public Transform player1Spawn, player2Spawn, player3Spawn, player4Spawn;
    GameObject kingCard, queenCard, aceCard, jokerCard;
    public GameObject selectIconPrefab = null;
    public GameObject[] cardPrefabs = new GameObject[6];
    public Transform[] playerCardSpawns = new Transform[5];
    public GameObject announcementBoard = null;

    //private LiarsBarPlayer[] playersScripts;

    //0 rey, 1 reina, 2 ace, 3 joker
    private int playersAlive, selectedCard, currentPlayerTurn;
    private int[] cardsNumbers, mixedCards, lastPlacedCards;
    private int maxPlayers = 2;
    private int lastPlayerIndex, step, confirmedPlayers;
    private bool gameIsOn, timerIsOn;
    private float timeCount;

    public List<NetworkConnectionToClient> playerConnections = new List<NetworkConnectionToClient>();
    private GameObject[] selectIcons = new GameObject[4];
    private GameObject[] player1CardGameObjects = new GameObject[5];
    private GameObject[] player2CardGameObjects = new GameObject[5];
    private GameObject[] player3CardGameObjects = new GameObject[5];
    private GameObject[] player4CardGameObjects = new GameObject[5];

    public void DeleteCardForPlayer(int playerIndex, int selectedCardIndex)
    {
        switch (playerIndex)
        {
            case 0:
                if(player1CardGameObjects[selectedCardIndex] != null)
                {
                    playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().TargetMeDeleteCard(player1CardGameObjects[selectedCardIndex]);
                    Destroy(player1CardGameObjects[selectedCardIndex]);
                    player1CardGameObjects[selectedCardIndex] = null;
                }
                break;
            case 1:
                if (player2CardGameObjects[selectedCardIndex] != null)
                {
                    playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().TargetMeDeleteCard(player2CardGameObjects[selectedCardIndex]);
                    Destroy(player2CardGameObjects[selectedCardIndex]);
                    player2CardGameObjects[selectedCardIndex] = null;
                }
                break;
            case 2:
                if (player3CardGameObjects[selectedCardIndex] != null)
                {
                    playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().TargetMeDeleteCard(player3CardGameObjects[selectedCardIndex]);
                    Destroy(player3CardGameObjects[selectedCardIndex]);
                    player3CardGameObjects[selectedCardIndex] = null;
                }
                break;
            case 3:
                if (player4CardGameObjects[selectedCardIndex] != null)
                {
                    playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().TargetMeDeleteCard(player4CardGameObjects[selectedCardIndex]);
                    Destroy(player4CardGameObjects[selectedCardIndex]);
                    player4CardGameObjects[selectedCardIndex] = null;
                }
                break;
        }
    }

    public void HighlightCardForPlayer(int playerIndex, int selectedCardIndex, bool isHighlighted)
    {
        Color newCardColor = Color.white;
        if (isHighlighted)
        {
            newCardColor = Color.green;
        }
        switch (playerIndex)
        {
            case 0:
                player1CardGameObjects[selectedCardIndex].GetComponentInChildren<SpriteRenderer>().color = newCardColor;
                playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().TargetMeHighlightCard(player1CardGameObjects[selectedCardIndex], newCardColor);
                break;
            case 1:
                player2CardGameObjects[selectedCardIndex].GetComponentInChildren<SpriteRenderer>().color = newCardColor;
                playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().TargetMeHighlightCard(player2CardGameObjects[selectedCardIndex], newCardColor);
                break;
            case 2:
                player3CardGameObjects[selectedCardIndex].GetComponentInChildren<SpriteRenderer>().color = newCardColor;
                playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().TargetMeHighlightCard(player3CardGameObjects[selectedCardIndex], newCardColor);
                break;
            case 3:
                player4CardGameObjects[selectedCardIndex].GetComponentInChildren<SpriteRenderer>().color = newCardColor;
                playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().TargetMeHighlightCard(player4CardGameObjects[selectedCardIndex], newCardColor);
                break;
        }
    }

    public void MoveSelectIconForPlayer(int playerIndex, int selectIconPositionIndex)
    {
        Vector2 selectIconPos = new Vector2(playerCardSpawns[selectIconPositionIndex].position.x, playerCardSpawns[selectIconPositionIndex].position.y - 1f);
        selectIcons[playerIndex].transform.position = selectIconPos;
        playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().TargetMeMoveSelectIcon(selectIcons[playerIndex], selectIconPos);
    }

    public void SpawnSelectIconForPlayers(int playerIndex)
    {
        if(selectIcons[playerIndex] == null)
        {
            Vector2 selectIconPos = new Vector2(playerCardSpawns[0].position.x, playerCardSpawns[0].position.y - 1f);
            GameObject selectIcon = Instantiate(selectIconPrefab, selectIconPos, Quaternion.identity);
            selectIcons[playerIndex] = selectIcon;
            selectIcon.SetActive(false);
            NetworkServer.Spawn(selectIcon);
            playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().TargetAllPlayerSelectIcon(selectIcon);
            playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().TargetMePlayerSelectIcon(selectIcon);
        }
    }

    public void SpawnCardsForPlayer(int playerIndex, int cardType, int cardSpot)
    {
        GameObject card = Instantiate(cardPrefabs[cardType], playerCardSpawns[cardSpot].position, Quaternion.identity);
        switch (playerIndex)
        {
            case 0:
                player1CardGameObjects[cardSpot] = card;
                break;
            case 1:
                player2CardGameObjects[cardSpot] = card;
                break;
            case 2:
                player3CardGameObjects[cardSpot] = card;
                break;
            case 3:
                player4CardGameObjects[cardSpot] = card;
                break;
        }
        card.SetActive(false);
        NetworkServer.Spawn(card);
        playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().TargetAllPlayerCard(card);
        playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().TargetMePlayerCard(card);
    }

    private void UpdateAnnouncementBoard(string newAnnouncement)
    {
        announcementBoard.GetComponentInChildren<TextMeshPro>().text = newAnnouncement;
        playerConnections[0].identity.gameObject.GetComponent<LiarsBarPlayer>().TargetAllUpdateAnnouncement(announcementBoard, newAnnouncement);
    }

    public void WaitSecondsForAnnouncement()
    {
        timeCount += Time.deltaTime;
        if(timeCount>= 5f)
        {
            SetRound();
            timerIsOn = false;
        }
    }

    public void WaitForPlayers()
    {
        confirmedPlayers++;
        if(confirmedPlayers >= playersAlive)
        {
            switch (step)
            {
                case 0:
                    SetRound();
                    break;
                case 1:
                    DistributeCards();
                    break;
            }
        }
    }

    public void UpdateTable(int playerIndex, int[] placedCards)
    {
        bool sendedEmptyCards = true;
        int totalCards = 0;
        for (int i = 0; i < placedCards.Length; i++)
        {
            if (placedCards[i] != -1)
            {
                sendedEmptyCards = false;
                totalCards++;
            }
        }
        if (sendedEmptyCards)
        {
            gameIsOn = true;
            return;
        }
        switch (selectedCard)
        {
            case 0:
                UpdateAnnouncementBoard("El jugador #" + (playerIndex + 1) + " ha colocado " + totalCards + " Cartas de Rey");
                break;
            case 1:
                UpdateAnnouncementBoard("El jugador #" + (playerIndex + 1) + " ha colocado " + totalCards + " Cartas de Reina");
                break;
            case 2:
                UpdateAnnouncementBoard("El jugador #" + (playerIndex + 1) + " ha colocado " + totalCards + " Cartas de As");
                break;
        }
        lastPlacedCards = placedCards;
        lastPlayerIndex = currentPlayerTurn;
        currentPlayerTurn++;
        if (currentPlayerTurn >= maxPlayers)
            currentPlayerTurn = 0;
        //todo este if es para checar si todos los jugadores se quedaron sin cartas, reiniciar el juego
        if (playerConnections[lastPlayerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().hasNoCards)
        {
            bool readyToEndRound = true;
            foreach (NetworkConnectionToClient playerConnection in playerConnections)
            {
                if (!playerConnection.identity.gameObject.GetComponent<LiarsBarPlayer>().hasNoCards)
                    readyToEndRound = false;
                
            }
            if (readyToEndRound)
            {
                SetRound();
                return;
            }
        }
        gameIsOn = true;
    }

    public void CallLiar()
    {
        if (lastPlayerIndex == -1)
        {
            gameIsOn = true;
            return;
        }
        //revelar cartas
        bool lastPlayerWasLying = false;
        foreach (int card in lastPlacedCards)
        {
            if (card != -1 && card != selectedCard && card != 3)
            {
                lastPlayerWasLying = true;
            }
        }
        if (lastPlayerWasLying)
        {
            UpdateAnnouncementBoard("El jugador #" + (currentPlayerTurn + 1) + " atrapo al jugador #" + (lastPlayerIndex + 1) + " mintiendo!");
            playerConnections[lastPlayerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().GetShot();
        }
        else
        {
            UpdateAnnouncementBoard("El jugador #" + (currentPlayerTurn + 1) + " llamó Mentira, pero el jugador #" + (lastPlayerIndex + 1) + " decia la verdad!");
            playerConnections[currentPlayerTurn].identity.gameObject.GetComponent<LiarsBarPlayer>().GetShot();
        }
        currentPlayerTurn++;
        if (currentPlayerTurn >= maxPlayers)
            currentPlayerTurn = 0;
        timeCount = 0f;
        timerIsOn = true;
    }

    private void StartLiarsBarGame()
    {
        step = 0;
        confirmedPlayers = 0;
        playersAlive = maxPlayers;
        selectedCard = -1;
        currentPlayerTurn = 0;
        cardsNumbers = new int[4];
        mixedCards = new int[20];
        lastPlacedCards = new int[5];
        lastPlayerIndex = -1;
        gameIsOn = false;
        for (int i = 0; i < playerConnections.Count; i++)
        {
            playerConnections[i].identity.gameObject.GetComponent<LiarsBarPlayer>().OnGameStart(i);
        }
        timerIsOn = false;
    }

    private void SetRound()
    {
        step = 1;
        confirmedPlayers = 0;
        foreach (NetworkConnectionToClient playerConnection in playerConnections)
        {
            if (playerConnection.identity.gameObject.GetComponent<LiarsBarPlayer>().isAlive)
                playerConnection.identity.gameObject.GetComponent<LiarsBarPlayer>().OnRoundReset();
        }
        for (int i = 0; i < maxPlayers; i++)
        {
            for (int t = 0; t < 5; t++)
            {
                DeleteCardForPlayer(i, t);
            }
        }
    }

    private void DistributeCards()
    {
        for (int i = 0; i < lastPlacedCards.Length; i++)
        {
            lastPlacedCards[i] = -1;
        }
        lastPlayerIndex = -1;
        selectedCard = UnityEngine.Random.Range(0, 3);
        switch (selectedCard)
        {
            case 0:
                UpdateAnnouncementBoard("Se ha seleccionado la carta de Rey");
                break;
            case 1:
                UpdateAnnouncementBoard("Se ha seleccionado la carta de Reina");
                break;
            case 2:
                UpdateAnnouncementBoard("Se ha seleccionado la carta de As");
                break;
        }
        switch (playersAlive)
        {
            case 4:
                cardsNumbers[0] = 6;
                cardsNumbers[1] = 6;
                cardsNumbers[2] = 6;
                cardsNumbers[3] = 2;
                break;
            case 3:
                cardsNumbers[0] = 4;
                cardsNumbers[1] = 4;
                cardsNumbers[2] = 4;
                cardsNumbers[3] = 2;
                cardsNumbers[selectedCard]++;
                break;
            case 2:
                cardsNumbers[0] = 3;
                cardsNumbers[1] = 3;
                cardsNumbers[2] = 3;
                cardsNumbers[3] = 1;
                break;
            case 1:
                for (int i = 0; i < playerConnections.Count; i++)
                {
                    if (playerConnections[i].identity.gameObject.GetComponent<LiarsBarPlayer>().isAlive)
                    {
                        UpdateAnnouncementBoard("Gano el jugador #" + i + "!");
                        return;
                    }
                }
                break;
        }
        for (int i = 0; i < (playersAlive * 5); i++)
        {
            int randomCardType = UnityEngine.Random.Range(0, 4);
            for (int k = 0; k < 4; k++)
            {
                if (cardsNumbers[randomCardType] <= 0)
                {
                    randomCardType++;
                    if (randomCardType >= 4)
                        randomCardType = 0;
                }
                else
                {
                    break;
                }
            }
            mixedCards[i] = randomCardType;
            cardsNumbers[randomCardType]--;

        }
        int playerIndex = 0;
        for (int i = 0; i < (playersAlive * 5); i++)
        {
            if (playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().isAlive)
            {
                playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().GetCard(playerIndex, mixedCards[i]);
                if (i == 4 || i == 9 || i == 14)
                    playerIndex++;
            }
            else
            {
                for (int k = 0; k < maxPlayers; k++)
                {
                    if (!playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().isAlive)
                    {
                        playerIndex++;
                        if (playerIndex >= maxPlayers)
                            playerIndex = 0;
                    }
                    else
                    {
                        break;
                    }
                }
                playerConnections[playerIndex].identity.gameObject.GetComponent<LiarsBarPlayer>().GetCard(playerIndex, mixedCards[i]);
            }
        }
        gameIsOn = true;
        //for (int i = 0; i < 4; i++)
        //{
        //    for (int t = 0; t < cardsNumbers[i]; t++)
        //    {
        //        int randomPlayer = UnityEngine.Random.Range(0, maxPlayers);
        //        print("salio el jugador: " + randomPlayer);
        //        for (int k = 0; k < maxPlayers; k++)
        //        {
        //            print("a");
        //            if (playerConnections[randomPlayer].identity.gameObject.GetComponent<LiarsBarPlayer>().hasAllCards || !playerConnections[randomPlayer].identity.gameObject.GetComponent<LiarsBarPlayer>().isAlive)
        //            {
        //                print("al parecer el jugador " + randomPlayer + " est llenito");
        //                randomPlayer++;
        //                if (randomPlayer >= maxPlayers)
        //                    randomPlayer = 0;
        //            }
        //            else
        //            {
        //                print("despues de este mensaje no deberia salir a");
        //                break;
        //            }
        //        }
        //        playerConnections[randomPlayer].identity.gameObject.GetComponent<LiarsBarPlayer>().GetCard(randomPlayer, i);
        //    }
        //}
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        playerConnections.Add(conn);
        //Transform start;
        //switch (numPlayers)
        //{
        //    case 0:
        //        start = player1Spawn;
        //        break;
        //    case 1:
        //        start = player2Spawn;
        //        break;
        //    case 2:
        //        start = player3Spawn;
        //        break;
        //    case 3:
        //        start = player4Spawn;
        //        break;
        //    default:
        //        //esto esta para que no me de error el start
        //        start = player1Spawn;
        //        print("algo salio mal");
        //        break;
        //}
        //GameObject player = Instantiate(playerPrefab, start.position, start.rotation, transform);
        //playersScripts[numPlayers] = player.GetComponent<LiarsBarPlayer>();
        //playersIDs[numPlayers] = NetworkConnection.LocalConnectionId;
        //playersIDs[numPlayers] = conn.connectionId;
        //playerConnections.Add(conn);
        //NetworkServer.AddPlayerForConnection(conn, player);
        //playerConnections[numPlayers] = conn;
        if(playerConnections.Count >= maxPlayers)
        {
            StartLiarsBarGame();
        }
    }

    #region Unity Callbacks

    public override void OnValidate()
    {
        base.OnValidate();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// Networking is NOT initialized when this fires
    /// </summary>
    public override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// </summary>
    public override void LateUpdate()
    {
        if (gameIsOn && !timerIsOn)
        {
            if (playerConnections[currentPlayerTurn].identity.gameObject.GetComponent<LiarsBarPlayer>().isAlive)
            {
                playerConnections[currentPlayerTurn].identity.gameObject.GetComponent<LiarsBarPlayer>().IsTurn();
                gameIsOn = false;
            }
            else
            {
                //print(currentPlayerTurn + " no estaba vivo XDDD");
                currentPlayerTurn++;
                if (currentPlayerTurn >= maxPlayers)
                    currentPlayerTurn = 0;
            }
        }
        if (timerIsOn)
        {
            WaitSecondsForAnnouncement();
        }
        base.LateUpdate();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// </summary>
    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    #endregion

    #region Start & Stop

    /// <summary>
    /// Set the frame rate for a headless server.
    /// <para>Override if you wish to disable the behavior or set your own tick rate.</para>
    /// </summary>
    public override void ConfigureHeadlessFrameRate()
    {
        base.ConfigureHeadlessFrameRate();
    }

    /// <summary>
    /// called when quitting the application by closing the window / pressing stop in the editor
    /// </summary>
    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
    }

    #endregion

    #region Scene Management

    /// <summary>
    /// This causes the server to switch scenes and sets the networkSceneName.
    /// <para>Clients that connect to this server will automatically switch to this scene. This is called automatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
    /// </summary>
    /// <param name="newSceneName"></param>
    public override void ServerChangeScene(string newSceneName)
    {
        base.ServerChangeScene(newSceneName);
    }

    /// <summary>
    /// Called from ServerChangeScene immediately before SceneManager.LoadSceneAsync is executed
    /// <para>This allows server to do work / cleanup / prep before the scene changes.</para>
    /// </summary>
    /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
    public override void OnServerChangeScene(string newSceneName) { }

    /// <summary>
    /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
    /// </summary>
    /// <param name="sceneName">The name of the new scene.</param>
    public override void OnServerSceneChanged(string sceneName) { }

    /// <summary>
    /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
    /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
    /// </summary>
    /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
    /// <param name="sceneOperation">Scene operation that's about to happen</param>
    /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) { }

    /// <summary>
    /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
    /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
    /// </summary>
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
    }

    #endregion

    #region Server System Callbacks

    /// <summary>
    /// Called on the server when a new client connects.
    /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerConnect(NetworkConnectionToClient conn) { }

    /// <summary>
    /// Called on the server when a client is ready.
    /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
    }

    /// <summary>
    /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
    /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    

    /// <summary>
    /// Called on the server when a client disconnects.
    /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
    }

    /// <summary>
    /// Called on server when transport raises an error.
    /// <para>NetworkConnection may be null.</para>
    /// </summary>
    /// <param name="conn">Connection of the client...may be null</param>
    /// <param name="transportError">TransportError enum</param>
    /// <param name="message">String message of the error.</param>
    public override void OnServerError(NetworkConnectionToClient conn, TransportError transportError, string message) { }

    /// <summary>
    /// Called on server when transport raises an exception.
    /// <para>NetworkConnection may be null.</para>
    /// </summary>
    /// <param name="conn">Connection of the client...may be null</param>
    /// <param name="exception">Exception thrown from the Transport.</param>
    public override void OnServerTransportException(NetworkConnectionToClient conn, Exception exception) { }

    #endregion

    #region Client System Callbacks

    /// <summary>
    /// Called on the client when connected to a server.
    /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
    /// </summary>
    public override void OnClientConnect()
    {
        base.OnClientConnect();
    }

    /// <summary>
    /// Called on clients when disconnected from a server.
    /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
    /// </summary>
    public override void OnClientDisconnect() { }

    /// <summary>
    /// Called on clients when a servers tells the client it is no longer ready.
    /// <para>This is commonly used when switching scenes.</para>
    /// </summary>
    public override void OnClientNotReady() { }

    /// <summary>
    /// Called on client when transport raises an error.</summary>
    /// </summary>
    /// <param name="transportError">TransportError enum.</param>
    /// <param name="message">String message of the error.</param>
    public override void OnClientError(TransportError transportError, string message) { }

    /// <summary>
    /// Called on client when transport raises an exception.</summary>
    /// </summary>
    /// <param name="exception">Exception thrown from the Transport.</param>
    public override void OnClientTransportException(Exception exception) { }

    #endregion

    #region Start & Stop Callbacks

    // Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
    // their functionality, users would need override all the versions. Instead these callbacks are invoked
    // from all versions, so users only need to implement this one case.

    /// <summary>
    /// This is invoked when a host is started.
    /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartHost() 
    {
        //base.OnStartHost();
    }

    /// <summary>
    /// This is invoked when a server is started - including when a host is started.
    /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartServer() { }

    /// <summary>
    /// This is invoked when the client is started.
    /// </summary>
    public override void OnStartClient() { }

    /// <summary>
    /// This is called when a host is stopped.
    /// </summary>
    public override void OnStopHost() { }

    /// <summary>
    /// This is called when a server is stopped - including when a host is stopped.
    /// </summary>
    public override void OnStopServer() { }

    /// <summary>
    /// This is called when a client is stopped.
    /// </summary>
    public override void OnStopClient() { }

    #endregion
}
