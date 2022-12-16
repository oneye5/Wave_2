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
using UnityEngine;

public class HighLevelNetcode : MonoBehaviour
{
    public const string KEY_JOINCODE = "RELAYKEY";
    public const string KEY_MAP = "MAP";
    public const string KEY_MODE = "MODE";

    const int defaultPlayerCount = 8;


    bool inGame = false;
    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    //high level stuffs
    public async void QuickPlay() //joins a lobby, if none exist, create one
    {
        //add more advanced way of picking a lobby in the future
        var lobbies = await getLobbies();
        if(lobbies.Count != 0)
        {
            await joinLobby(lobbies[0]);
            return;
        }

        QuickCreateGame();
    }
    public async Task<bool> JoinGame(Lobby l)
    {
        var joinCode = l.Data[KEY_JOINCODE].Value;
        l = await joinLobby(l);
        await joinRelay(joinCode);
        NetworkManager.Singleton.StartClient();

        return true;
    }
    public async void CreateGame(int maxPlayers , string lobbyname , int map , int mode)
    {
        var joinCode = await createRelay();
        await createLobby(joinCode);
        NetworkManager.Singleton.StartHost();
    }
    public async void QuickCreateGame()
    {
        var joinCode = await createRelay();
        await createLobby(joinCode);
        NetworkManager.Singleton.StartHost();
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


    // relay system
    public async Task<String> createRelay() //returns the join code
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
            return joinCode;
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
    //lobby system
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
    public async Task<Lobby> createLobby(string relayCode , int maxPlayers =defaultPlayerCount, string lobbyName = "defaultLobbyName",int map =0,int mode= 0)
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

}
