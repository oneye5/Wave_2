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

public class HighLevelNetcode : MonoBehaviour
{
    public const string KEY_JOINCODE = "JOINCODE";
    public const string KEY_MAP = "MAP";
    public const string KEY_MODE = "MODE";
    public const string KEY_REGION = "REGION";

    public const string DEFAULT_LOBBY_NAME = "DefaultName";

    const int defaultPlayerCount = 8;
    public List<GameObject> mapPrefabs;
    private GameObject currentMap;
    [SerializeField] Transform mapParent;
    [SerializeField] GameObject mainMenuUI;
    Lobby currentLobby;
    [SerializeField] float heartbeatDelay;
    float timeTillHeartbeat;
     void Start()
    {
         UnityServices.InitializeAsync();
         AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    //high level stuffs
    #region high level methods
    public async void QuickPlay() //joins a lobby, if none exist, create one
    {
        //add more advanced way of picking a lobby in the future
        var lobbies = await getLobbies();
        if(lobbies.Count != 0)
        {
            await JoinGame(lobbies[0]);
            return;
        }

        QuickCreateGame();
    }
    public async Task<bool> JoinGame(Lobby l)
    {
        if(l == null)
        {
            Debug.LogError("HighLevel_Netcode.JoinGame(Lobby) : Lobby was null!");
            return false;
        }

        l = await joinLobby(l);

        var joinCode = l.Data[KEY_JOINCODE].Value;
        Debug.Log("joining " + joinCode);
        await joinRelay(joinCode);
        NetworkManager.Singleton.StartClient();

        initGame(l);
        return true;
    }
    public async Task CreateGame(int maxPlayers , string lobbyname , int map , int mode)
    {
        var relayData = await createRelay();
        var l =   await createLobby(relayData.joinCode,relayData.alloc.Region);
        NetworkManager.Singleton.StartHost();

        initGame(l);
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
           // Debug.Log( Responce.Results[0].Data[KEY_JOINCODE].Value);
            return Responce.Results;
        }
        catch(LobbyServiceException x)
        {
            Debug.Log(x);
            return null;
        }
    }
    private async Task heartBeat()
    {
        if(currentLobby == null)
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
        LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
        Debug.Log("heartbeat");
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
            Debug.LogWarning(e);
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
         leaveLobby(currentLobby.Id , AuthenticationService.Instance.PlayerId);
        NetworkManager.Singleton.Shutdown();
        Destroy(currentMap);
        currentMap = null;
        currentLobby = null;
       mainMenuUI.SetActive(true);
    }    
    #endregion
    public enum gameMode
    {
        ffa = 0,
    }

    private void initGame(Lobby l) //spawns map
    {
        Debug.Log("shutting down connection");
        int mapIndex = int.Parse(l.Data[KEY_MAP].Value);

        if(currentMap != null)
            Destroy(currentMap);

        currentMap = Instantiate(mapPrefabs[mapIndex] , mapParent);
        mainMenuUI.SetActive(false);
        currentLobby = l;
        
    }


   
    private void Update()
    {
       heartBeat();
    }
    private void OnApplicationQuit()
    {
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