using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class HighLevelNetcode : NetworkBehaviour
{
    #region vars
    public const string KEY_JOINCODE = "JOINCODE";
    public const string KEY_MAP = "MAP";
    public const string KEY_MODE = "MODE";
    public const string KEY_REGION = "REGION";

    public const string DEFAULT_LOBBY_NAME = "DefaultName";

    const int defaultPlayerCount = 8;
    public List<GameObject> mapPrefabs;
    public GameObject currentMap;

    private ServerGameManager gameManager;

    [SerializeField] Transform mapParent;
    [SerializeField] GameObject mainMenuUI;
    
    public Lobby currentLobby;
    [SerializeField] float heartbeatDelay;
    float timeTillHeartbeat;


    #endregion
    void Start()
    {
        HighLevelNetcodeRef.Instance = this;
         UnityServices.InitializeAsync();
         AuthenticationService.Instance.SignInAnonymouslyAsync();
        gameManager = GetComponent<ServerGameManager>();
        timeTillHeartbeat = heartbeatDelay;
    }
    //game methods
    
    //high level stuffs, to do with setup
    #region high level methods
    public async void QuickPlay() //joins a lobby, if none exist, create one
    {
        Global_Ui_Manager_Ref.Instance.OpenLoadingScreen();
        //add more advanced way of picking a lobby in the future
        var lobbies = await getLobbies();
        int lobbyIndex = 0;

        findAndJoin:
        if(lobbies.Count > lobbyIndex)
        {
            try
            {
                await JoinGame(lobbies[lobbyIndex]);
                return;
            }
            catch
            {
                Debug.LogWarning("failed joining lobby");
                lobbyIndex++;
                goto findAndJoin;
            }
           
        }

        QuickCreateGame();
        Global_Ui_Manager_Ref.Instance.CloseLoadingScreen();
    }
    public async Task<bool> JoinGame(Lobby l)
    {
        try
        {
            if(l == null)
            {
                Debug.LogError("HighLevel_Netcode.JoinGame(Lobby) : Lobby was null!");
                return false;
            }
            Global_Ui_Manager_Ref.Instance.OpenLoadingScreen();

            await checkPlayerExists(l);
            l = await joinLobby(l);

            var joinCode = l.Data[KEY_JOINCODE].Value;
            Debug.Log("joining " + joinCode);
            await joinRelay(joinCode);


            initGame_Wmap(l);
            NetworkManager.Singleton.StartClient();

            ServerGameManagerRef.Instance.ServerStart();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }
    }
    public async Task CreateGame(int maxPlayers , string lobbyname , int map , int mode)
    {
        var relayData = await createRelay();
        var l =   await createLobby(relayData.joinCode,relayData.alloc.Region);
        NetworkManager.Singleton.StartHost();

        initGame_Wmap(l);
        ServerGameManagerRef.Instance.ServerStart();
    }
    public async void QuickCreateGame()
    {
     await  CreateGame(8 , DEFAULT_LOBBY_NAME , 0 , 0);
    }
    public async Task<List<Lobby>> getLobbies()
    {
        try
        {  
            var Responce = await Lobbies.Instance.QueryLobbiesAsync();
            return Responce.Results;
        }
        catch(LobbyServiceException x)
        {
            Debug.Log(x);
            return null;
        }
    }
    private async void heartBeat()
    {
        if(currentLobby == null || !(IsHost||IsServer))
            return;

        if(timeTillHeartbeat > 0)
        {
            timeTillHeartbeat -= Time.deltaTime;
            return;
        }
        else
        {
            timeTillHeartbeat = heartbeatDelay;
        }
        await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id).ConfigureAwait(false);
    }
    #endregion
    #region relay methods
    public async Task<RelayData> createRelay() //returns the join code
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(defaultPlayerCount);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);


            //crreate connection
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var relayData = new RelayServerData(allocation , "dtls");
            
            transport.SetRelayServerData(relayData);

            RelayData outData = new RelayData();
            outData.joinCode = joinCode;
            outData.alloc = allocation;

            Debug.Log("relay created " + outData.joinCode + " " + outData.alloc);
            return outData;
        }
        catch(RelayServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }
    public async Task joinRelay(string joinCode)
    {
        try
        {
            var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var relayData = new RelayServerData(allocation , "dtls");
            transport.SetRelayServerData(relayData);
        }
        catch(RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
    #endregion
    #region lobby methods
    public async Task<bool> checkPlayerExists(Lobby l)
    {
        foreach(Player x in l.Players)
        {
            if(x.Id == AuthenticationService.Instance.PlayerId)
            {
               await LobbyService.Instance.RemovePlayerAsync(l.Id , x.Id);
                return true;
            }
        }
        return false;
    }
    public async Task<Lobby> joinLobby(Lobby l)
    {
        try
        {
            var x = await Lobbies.Instance.JoinLobbyByIdAsync(l.Id);
            Debug.Log("joined " + x.Name + " max players " + x.MaxPlayers + " code " + x.LobbyCode);
            return x;
        }
        catch(LobbyServiceException e)
        {
            Debug.LogError(e);
            return null;
        }
    }
    public async Task<Lobby> createLobby(string relayCode,String region , int maxPlayers =defaultPlayerCount, string lobbyName = "defaultLobbyName",int map =0,int mode= 0)
    {
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions();
            //  options.Player = //to impliment
            options.IsPrivate = false;
            options.Data = new Dictionary<string , DataObject>
            {
                {KEY_JOINCODE,new DataObject(DataObject.VisibilityOptions.Member,relayCode) },
                {KEY_MAP,new DataObject(DataObject.VisibilityOptions.Public,map.ToString()) },
                   {KEY_MODE,new DataObject(DataObject.VisibilityOptions.Public,mode.ToString()) },
                   {KEY_REGION,new DataObject(DataObject.VisibilityOptions.Public,region.ToString()) },
            };


            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName , maxPlayers , options);

            Debug.Log("lobby created " + lobbyName + " " + maxPlayers);
            return lobby;
        }
        catch(LobbyServiceException e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    public async void leaveLobby(string lobbyID , string playerID)
    {
        try
        {
           await LobbyService.Instance.RemovePlayerAsync(lobbyID , playerID);
        }
        catch(LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }
    public async void leaveLobby(Lobby l , string playerID)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(l.Id , playerID);
        }
        catch(LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }
    public async void leaveLobby(Lobby l , Player p)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(l.Id , p.Id);
        }
        catch(LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }

    public void selfLeaveGame()
    {
        Debug.Log("leaving game");
         leaveLobby(currentLobby.Id , AuthenticationService.Instance.PlayerId);
        if(NetworkManager.Singleton != null)
        NetworkManager.Singleton.Shutdown();
        Destroy(currentMap);
        currentMap = null;
        currentLobby = null;
       mainMenuUI.SetActive(true);
        Debug.Log("game left");
        Cursor.lockState = CursorLockMode.None;
    }    
    #endregion
    public enum GameMode
    {
        ffa = 0,
    }

    private void initGame_Wmap(Lobby l) //spawns map
    {
        Debug.Log("INITIALIZING");
        int mapIndex = int.Parse(l.Data[KEY_MAP].Value);

        if(currentMap != null)
            Destroy(currentMap);

        currentMap = Instantiate(mapPrefabs[mapIndex] , mapParent);
        mainMenuUI.SetActive(false);
        currentLobby = l;
        
        Global_Ui_Manager_Ref.Instance.CloseLoadingScreen();
    }


   
    private void Update()
    {
       heartBeat();
    }
    private void OnApplicationQuit()
    {
        Debug.Log("exiting application");
        if(currentLobby == null)
            return;

        selfLeaveGame();
    }
}

public class RelayData
{
   public Allocation alloc;
   public string joinCode;
}
public static class HighLevelNetcodeRef
{
  public  static HighLevelNetcode Instance;
}