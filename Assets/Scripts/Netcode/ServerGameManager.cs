using Gravitons.UI.Modal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Rendering;
using static HighLevelNetcode;

public class ServerGameManager : NetworkBehaviour
{
    #region hits and kills
    [ServerRpc(RequireOwnership = false)]
    public void playerHit_ServerRpc(float damage , string PlayerObjectID_from , string PlayerObjectID_to,int sendHitMarker_Bool = 0)
    {
        //get players
        Debug.Log("player to = " + PlayerObjectID_to);
        NetworkObject playerTo = GetNetworkObject(ulong.Parse(PlayerObjectID_to));
        var playerFrom = GetNetworkObject(ulong.Parse(PlayerObjectID_from));
        var playerToAuth = gameStats.getAuthKeyFromObjectID(PlayerObjectID_to);
        var healthComp = playerTo.GetComponentInChildren<HealthHandle>();
        var playerFromAuth = gameStats.getAuthKeyFromObjectID(PlayerObjectID_from);
        healthComp.health.Value -= damage;

        if(healthComp.health.Value <= 0)
        {
            healthComp.health.Value = healthComp.defaultHealth;
            playerKilled_ClientRpc(PlayerObjectID_to,playerFrom.NetworkObjectId.ToString());
            gameStats.playerStats[playerFromAuth].kills++;
            gameStats.playerStats[playerToAuth].deaths++;
            playerKIlled_ServerRpc(playerFromAuth , playerToAuth);
        }

        gameStats.playerStats[playerToAuth].damageTaken += damage;
        gameStats.playerStats[playerFromAuth].damageDone += damage;
        playerHit_ClientRpc(damage , PlayerObjectID_from , PlayerObjectID_to,healthComp.health.Value,sendHitMarker_Bool);

    }
    [ClientRpc]
    public void playerHit_ClientRpc(float damage , string PlayerObjectID_from , string PlayerObjectID_to ,float currentHealth,int sendHitmarker_Bool)
    {
        var playerTo = GetNetworkObject(ulong.Parse(PlayerObjectID_to));


        if(!playerTo.IsOwner)
            return;
        var playerFrom = GetNetworkObject(ulong.Parse(PlayerObjectID_from));
        if(playerFrom.IsOwner && sendHitmarker_Bool ==1)
        {
            playerFrom.gameObject.GetComponentInChildren<PlayerUiManager>().showHitMarker(false);
        }

        var pManager = playerTo.GetComponent<PlayerManager>();
        var visuals = pManager.weaponManager.visuals;
        var healthHandle = pManager.healthHandle;


        healthHandle.publicHealth = currentHealth;
        visuals.recoilPunchRandom(playerTo.GetComponentInChildren<Camera>().transform , damage / 10 , 0.25f);
    }

    [ClientRpc]
    public void playerKilled_ClientRpc(string To_networkObjID,string From_NetworkObjID)
    {
        var player = GetNetworkObject(ulong.Parse(To_networkObjID));
        if(!player.IsOwner)
            return;

        var pManager = player.GetComponent<PlayerManager>();
            
          pManager.ResetPlayer();
        pManager.healthHandle.publicHealth = pManager.healthHandle.defaultHealth;

    }
    [ServerRpc]
    public void playerKIlled_ServerRpc(string authFrom , string AuthTo)
    {
        gameStats.playerStats[authFrom].kills++;
        gameStats.playerStats[AuthTo].deaths++;
    }
    public void hostLeave()
    {
        HighLevelNetcodeRef.Instance.selfLeaveGame();
        ModalManager.Show("HOST HAS LEFT" , "GAME ENDED" , new[] { new ModalButton() { Text = "CLOSE" } });
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerJoin_ServerRpc(string lobbyID , ulong playerObjectID)
    {
        PlayerStatistics playerStats = new PlayerStatistics();
        playerStats.isHost = false;
        playerStats.playerObjectID = playerObjectID;
        if(!gameStats.playerStats.ContainsKey(lobbyID))
            gameStats.playerStats.Add(lobbyID , playerStats);
        Debug.Log("Player Joined " + lobbyID);

        var healthCmp = GetNetworkObject(playerObjectID).GetComponentInChildren<HealthHandle>();
        healthCmp.health.Value = healthCmp.defaultHealth;
    }
    #endregion
    #region misc management
    //server management vars
    public GameStatistics gameStats = null;
    public void ServerStart()
    {
        gameStats = new GameStatistics();

        if(IsHost || IsServer)
        {
            gameStats.Created = DateTime.Now;
            gameStats.HostID = AuthenticationService.Instance.PlayerId;
            PlayerStatistics hostStats = new PlayerStatistics();

            hostStats.playerObjectID = Camera.main.GetComponentInParent<NetworkObject>().NetworkObjectId;
            hostStats.isHost = true;

            gameStats.playerStats.Add(AuthenticationService.Instance.PlayerId , hostStats);
        }
    }
    private void Start()
    {
        ServerGameManagerRef.Instance = this;
    }
    #endregion
    #region ping pong system
    [SerializeField] float pingRate;
    float timeTillPing = 1;
    ulong? PlayerObjectID = null;
    void pingPong()
    {
        if(!(IsHost || IsServer) || HighLevelNetcodeRef.Instance.currentLobby == null)
            return;
        timeTillPing -= Time.fixedDeltaTime;
        if(timeTillPing <= 0)
        {
            timeTillPing = pingRate;
            pingClientRpc(JsonConvert.SerializeObject(gameStats));
        }
    }

    [ClientRpc]
    private void pingClientRpc(string gameStatistic_Json)
    {
       // Debug.Log("json string \n" + gameStatistic_Json);
        gameStats = JsonConvert.DeserializeObject<GameStatistics>(gameStatistic_Json);
        if(PlayerObjectID == null)
            PlayerObjectID = Camera.main.GetComponentInParent<NetworkObject>().NetworkObjectId;

        pongServerRpc(DateTime.Now.Ticks , AuthenticationService.Instance.PlayerId , PlayerObjectID.ToString());
    }

    [ServerRpc(RequireOwnership = false)]
    void pongServerRpc(long sentTime , string lobbyPlayerID , string playerObjectID)
    {


        var dateTimeSent = new DateTime(sentTime);
        var dif = DateTime.Now - dateTimeSent;
        var pingMs = dif.Milliseconds;
        if(gameStats.playerStats.TryGetValue(lobbyPlayerID , out var stats))
        {
            stats.ping = pingMs;
            stats.playerObjectID = ulong.Parse(playerObjectID);
        }
        else //if playerStats does not exist then create it
        {
            gameStats.playerStats.Add(lobbyPlayerID , new PlayerStatistics());
            gameStats.playerStats[lobbyPlayerID].ping = pingMs;
            gameStats.playerStats[lobbyPlayerID].playerObjectID = ulong.Parse(playerObjectID);
        }
    }
    private void FixedUpdate()
    {
        pingPong();
    }
    #endregion
    #region Local management
    GameMode gameMode;
    public int Team;
    public List<spawnVolume> spawnVolumes;
    public void GameStart()
    {
        setGameMode();
        if(gameMode != GameMode.ffa)
            AssignTeam();
        ResetSpawnpoints();
    }
    public void ResetSpawnpoints()
    {
        spawnVolumes = HighLevelNetcodeRef.Instance.currentMap.GetComponent<SpawnpointVolumes>().SpawnVolumes;
        if(gameMode != GameMode.ffa)
        {
            List<spawnVolume> newVolumes = new List<spawnVolume>();
            foreach(spawnVolume volume in spawnVolumes)
            {
                if(volume.team == Team)
                    newVolumes.Add(volume);
            }
            spawnVolumes = newVolumes;
        }
    }
    public void setGameMode()
    {
        int enumIndex = int.Parse(HighLevelNetcodeRef.Instance.currentLobby.Data[HighLevelNetcode.KEY_MODE].Value);
        gameMode = ((HighLevelNetcode.GameMode)enumIndex);
    }
    public Vector3 getSpawnPosition()
    {
        try
        {
            int index = Mathf.RoundToInt(UnityEngine.Random.value * spawnVolumes.Count);
            if(index > spawnVolumes.Count-1)
                index = spawnVolumes.Count-1;
            var vol = spawnVolumes[index];
            float x = UnityEngine.Random.Range(-vol.scale.x / 2 , vol.scale.x / 2);
            float y = UnityEngine.Random.Range(-vol.scale.y / 2 , vol.scale.y / 2);
            float z = UnityEngine.Random.Range(-vol.scale.z / 2 , vol.scale.z / 2); ;

            Vector3 pos = vol.pos + new Vector3(x , y , z);
            Debug.Log("spawnpoint found " + pos.ToString());
            Debug.Log("index is " + index);
            return pos;
        }
        catch(Exception e)
        {
            Debug.LogError("NO SPAWNPOINT FOUND, (GETSPAWNPOS)" + e.Message);
            return new Vector3(0 , 5 , 0);
        }
    }
    public void AssignTeam()
    {
        Debug.Log("assigning team");
    }
    #endregion
}
public class GameStatistics
{
    public DateTime Created;
    public Dictionary<string , PlayerStatistics> playerStats = new Dictionary<string , PlayerStatistics>(); //key is authservice.playerID
    public string HostID;
    public string getAuthKeyFromObjectID(string objID)
    {
        foreach(var item in playerStats)
        {
            if(item.Value.playerObjectID.ToString() == objID)
            {
                return item.Key;
            }
        }
        Debug.LogWarning(objID + " Item not found, returning null (getAuthKeyFromObjectID)");
        return null;
    }
}
public class PlayerStatistics
{
    public float damageDone;
    public float damageTaken;
    public int kills;
    public int deaths;
    public float ping;

    public ulong playerObjectID;

    public bool isHost;
}
public static class ServerGameManagerRef
{
    public static ServerGameManager Instance;
}