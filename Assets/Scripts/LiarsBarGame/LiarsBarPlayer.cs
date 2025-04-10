using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class LiarsBarPlayer : NetworkBehaviour
{
    //LiarsBarNetworkManager hostNetworkManager;
    [SyncVar]
    public bool isAlive;
    [SyncVar] 
    public bool hasNoCards;
    //[SyncVar]
    //public int cardIndex;

    private GameObject[] playerCards = new GameObject[5];
    [SyncVar]
    private GameObject[] cardPrefabs = new GameObject[5];
    [SyncVar]
    private Transform[] playerCardSpawns = new Transform[5];

    private bool isTurn;
    private int bulletsLeft, currentSelectedCard;
    private int[] playerCardsIndex = new int[5];
    private int[] selectedCards = new int[5];

    private int playerIndex;

    [SyncVar(hook = nameof(SetColor))]
    public Color color;
    public SpriteRenderer sr;


    [ClientRpc]
    public void OnGameStart(int newPlayerIndex)
    {
        if (!isLocalPlayer) return;
        playerIndex = newPlayerIndex;
        CommandSetIsAlive(true);
        CommandSetHasNoCards(true);
        isTurn = false;
        bulletsLeft = 4;
        currentSelectedCard = 0;
        for (int i = 0; i < playerCardsIndex.Length; i++)
        {
            playerCards[i] = null;
            playerCardsIndex[i] = -1;
            selectedCards[i] = -1;
        }
        CommandPlayerIsReady();
    }

    [ClientRpc]
    public void OnRoundReset()
    {
        if (!isLocalPlayer) return;
        CommandSetHasNoCards(true);
        isTurn = false;
        currentSelectedCard = 0;
        for (int i = 0; i < playerCardsIndex.Length; i++)
        {
            playerCards[i] = null;
            playerCardsIndex[i] = -1;
            selectedCards[i] = -1;
        }
        CommandPlayerIsReady();
    }

    [ClientRpc]
    public void GetCard(int playerIndex, int cardType)
    {
        if (!isLocalPlayer) return;
        for (int i = 0; i < playerCardsIndex.Length; i++)
        {
            if (playerCardsIndex[i] == -1)
            {
                playerCardsIndex[i] = cardType;
                CommandSpawnCardsForPlayer(playerIndex, cardType, i);
                if(i == 0)
                {
                    CommandSpawnSelectIconForPlayer(playerIndex);
                }
                //CommandSetPlayerCard(LiarsBarNetworkManager.singleton.cardPrefabs[cardType], LiarsBarNetworkManager.singleton.playerCardSpawns[i].position);
                CommandSetHasNoCards(false);
                return;
            }
        }
    }

    [ClientRpc]
    public void GetShot()
    {
        if (!isLocalPlayer) return;
        int shootChance = Random.Range(1, 101);
        switch (bulletsLeft)
        {
            case 4:
                if (shootChance <= 10)
                    CommandSetIsAlive(false);
                break;
            case 3:
                if (shootChance <= 20)
                    CommandSetIsAlive(false);
                break;
            case 2:
                if (shootChance <= 40)
                    CommandSetIsAlive(false);
                break;
            case 1:
                CommandSetIsAlive(false);
                break;
        }
        bulletsLeft--;
        if (isAlive)
            print("Sobrevivi");
    }

    public bool SurvivedShoot()
    {
        bool response = true;
        int shootChance = Random.Range(1, 101);
        switch (bulletsLeft)
        {
            case 4:
                if (shootChance <= 10)
                    response = false;
                break;
            case 3:
                if (shootChance <= 20)
                    response = false;
                break;
            case 2:
                if (shootChance <= 40)
                    response = false;
                break;
            case 1:
                response = false;
                break;
        }
        bulletsLeft--;
        if (!response)
            isAlive = false;
        return response;
    }

    [ClientRpc]
    public void IsTurn()
    {
        if (!isLocalPlayer) return;
        print("es mi turno");
        isTurn = true;
    }

    #region Unity Callbacks

    /// <summary>
    /// Add your validation code here after the base.OnValidate(); call.
    /// </summary>
    protected override void OnValidate()
    {
        base.OnValidate();
    }

    // NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.
    void Awake()
    {
    }

    void Start()
    {

    }

    private void Update()
    {
        if (!isLocalPlayer || !isTurn) return;
        foreach (char c in Input.inputString)
        {
            switch (c)
            {
                case 'a':
                    currentSelectedCard--;
                    if (currentSelectedCard < 0)
                        currentSelectedCard = playerCardsIndex.Length - 1;
                    CommandMoveSelectIcon(playerIndex, currentSelectedCard);
                    break;
                case 'd':
                    currentSelectedCard++;
                    if (currentSelectedCard >= playerCardsIndex.Length)
                        currentSelectedCard = 0;
                    CommandMoveSelectIcon(playerIndex, currentSelectedCard);
                    break;
                case 'f':
                    if (selectedCards[currentSelectedCard] == -1)
                    {
                        selectedCards[currentSelectedCard] = playerCardsIndex[currentSelectedCard];
                        CommandHighlightCard(playerIndex, currentSelectedCard, true);
                    }
                    else
                    {
                        selectedCards[currentSelectedCard] = -1;
                        CommandHighlightCard(playerIndex, currentSelectedCard, false);
                    }
                    break;
                case 'g':
                    CommandUpdateTable(playerIndex, selectedCards);
                    CommandSetHasNoCards(true);
                    for (int i = 0; i < playerCardsIndex.Length; i++)
                    {
                        if (selectedCards[i] != -1)
                        {
                            selectedCards[i] = -1;
                            playerCardsIndex[i] = -1;
                            playerCards[i] = null;
                            CommandDeleteCard(playerIndex, i);
                        }
                        else if (playerCardsIndex[i] != -1)
                        {
                            CommandSetHasNoCards(false);
                        }
                    }
                    isTurn = false;
                    break;
                case 'h':
                    CommandCallLiar();
                    isTurn = false;
                    break;
                default:
                    //nada
                    break;
            }
        }
    }

    [Command]
    private void CommandSetIsAlive(bool newIsAlive)
    {
        isAlive = newIsAlive;
        if (!isAlive)
            print("mori");
    }

    [Command]
    private void CommandSetHasNoCards(bool newHasNoCards)
    {
        hasNoCards = newHasNoCards;
    }

    [Command]
    private void CommandSpawnCardsForPlayer(int newPlayerIndex, int newCardType, int newCardIndex)
    {
        LiarsBarNetworkManager.singleton.SpawnCardsForPlayer(newPlayerIndex, newCardType, newCardIndex);
    }

    [Command]
    private void CommandSpawnSelectIconForPlayer(int newPlayerIndex)
    {
        LiarsBarNetworkManager.singleton.SpawnSelectIconForPlayers(newPlayerIndex);
    }

    [Command]
    private void CommandPlayerIsReady()
    {
        LiarsBarNetworkManager.singleton.WaitForPlayers();
    }

    [ClientRpc]
    public void TargetAllPlayerCard(GameObject card)
    {
        card.SetActive(false);//Aqui esta la logica para como se vera la carta privada en los otros clientes.
    }

    [TargetRpc]
    public void TargetMePlayerCard(GameObject myCard)
    {
        myCard.SetActive(true);//Aqui cambias lo que editaste para otros clientes, ya sean imagenes, colores, o visibilidad.
    }

    [ClientRpc]
    public void TargetAllPlayerSelectIcon(GameObject selectIcon)
    {
        selectIcon.SetActive(false);
    }

    [TargetRpc]
    public void TargetMePlayerSelectIcon(GameObject mySelectIcon)
    {
        mySelectIcon.SetActive(true);
    }

    [Command]
    private void CommandMoveSelectIcon(int newPlayerIndex, int newSelectIconPositionIndex)
    {
        LiarsBarNetworkManager.singleton.MoveSelectIconForPlayer(newPlayerIndex, newSelectIconPositionIndex);
    }

    [TargetRpc]
    public void TargetMeMoveSelectIcon(GameObject mySelectIcon, Vector2 newSelectIconPosition)
    {
        mySelectIcon.transform.position = newSelectIconPosition;
    }

    [Command]
    private void CommandHighlightCard(int newPlayerIndex, int newSelectedCardIndex, bool newIsHighlighted)
    {
        LiarsBarNetworkManager.singleton.HighlightCardForPlayer(newPlayerIndex, newSelectedCardIndex, newIsHighlighted);
    }

    [TargetRpc]
    public void TargetMeHighlightCard(GameObject myHighlightedCard, Color newCardColor)
    {
        myHighlightedCard.GetComponentInChildren<SpriteRenderer>().color = newCardColor;
    }

    [Command]
    private void CommandDeleteCard(int newPlayerIndex, int newSelectedCardIndex)
    {
        LiarsBarNetworkManager.singleton.DeleteCardForPlayer(newPlayerIndex, newSelectedCardIndex);
    }

    [TargetRpc]
    public void TargetMeDeleteCard(GameObject myDeletedCard)
    {
        Destroy(myDeletedCard);
    }

    [Command]
    private void CommandUpdateTable(int newPlayerIndex, int[] cardsToPlace)
    {
        LiarsBarNetworkManager.singleton.UpdateTable(newPlayerIndex, cardsToPlace);
    }

    [Command]
    private void CommandCallLiar()
    {
        LiarsBarNetworkManager.singleton.CallLiar();
    }

    [ClientRpc]
    public void TargetAllUpdateAnnouncement(GameObject newAnnouncementBoard, string newAnnouncementText)
    {
        newAnnouncementBoard.GetComponentInChildren<TextMeshPro>().text = newAnnouncementText;
    }

    #endregion



    [Command]
    private void CommandSetColor(Color newColor)
    {
        color = newColor;
    }
    private void SetColor(Color oldColor, Color newColor)
    {
        sr.color = newColor;
    }

    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer() { }

    /// <summary>
    /// Invoked on the server when the object is unspawned
    /// <para>Useful for saving object data in persistent storage</para>
    /// </summary>
    public override void OnStopServer() { }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() 
    {
        //CommandSetColor(GameObject.FindFirstObjectByType<PlayerInfo>().color);
    }

    /// <summary>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
    /// </summary>
    public override void OnStopClient() { }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer() { }

    /// <summary>
    /// Called when the local player object is being stopped.
    /// <para>This happens before OnStopClient(), as it may be triggered by an ownership message from the server, or because the player object is being destroyed. This is an appropriate place to deactivate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStopLocalPlayer() {}

    /// <summary>
    /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
    /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
    /// <para>When <see cref="NetworkIdentity.AssignClientAuthority">AssignClientAuthority</see> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnectionToClient parameter included, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStartAuthority() { }

    /// <summary>
    /// This is invoked on behaviours when authority is removed.
    /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStopAuthority() { }

    #endregion
}
