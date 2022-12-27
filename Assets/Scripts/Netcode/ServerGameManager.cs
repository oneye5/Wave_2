using Gravitons.UI.Modal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using static HighLevelNetcode;

public class ServerGameManager : NetworkBehaviour
{
    private void Start()
    {
        ServerGameManagerRef.Instance = this;
    }
    [ServerRpc(RequireOwnership = false)]
    public void playerHit_ServerRpc(float damage , string PlayerObjectID_from , string PlayerObjectID_to)
    {
        //get players
        var playerTo = GetNetworkObject(ulong.Parse(PlayerObjectID_to));
        var playerFrom = GetNetworkObject(ulong.Parse(PlayerObjectID_from));
      //  var playerToAuth = playerTo.GetComponent<PlayerManager>().AuthID.Value;
        var healthComp = playerTo.GetComponentInChildren<HealthHandle>();
      //  var playerFromAuth = playerFrom.GetComponent<PlayerManager>().AuthID.Value;
        healthComp.health.Value -= damage;

        if(healthComp.health.Value < 0)
        {
            healthComp.health.Value = healthComp.defaultHealth;
            playerKilled_ClientRpc(PlayerObjectID_to);
        //    gameStats.playerStats[playerFromAuth].kills++;
          //  gameStats.playerStats[playerToAuth].deaths++;
        }

      //  gameStats.playerStats[playerToAuth].damageTaken += damage;
       // gameStats.playerStats[playerFromAuth].damageDone += damage;
        playerHit_ClientRpc(damage , PlayerObjectID_from , PlayerObjectID_to);
    }
    [ClientRpc]
    public void playerHit_ClientRpc(float damage , string PlayerObjectID_from , string PlayerObjectID_to)
    {
        var playerTo = GetNetworkObject(ulong.Parse(PlayerObjectID_to));
      

        if(!playerTo.IsOwner)
            return;
     var playerFrom = GetNetworkObject(ulong.Parse(PlayerObjectID_from));

        var visuals = playerTo.GetComponentInChildren<BulletVisuals>();
        visuals.recoilPunchRandom(playerTo.GetComponentInChildren<Camera>().transform , damage / 10 , 0.25f);
    }
    [ClientRpc]
    public void playerKilled_ClientRpc(string id)
    {
        var player = GetNetworkObject(ulong.Parse(id));
        if(!player.IsOwner)
            return;

        player.GetComponent<PlayerManager>().ResetPlayer();

    }
    public void hostLeave()
    {
        HighLevelNetcodeRef.HighLevelNetcode.selfLeaveGame();
        ModalManager.Show("HOST HAS LEFT" , "GAME ENDED" , new[] { new ModalButton() { Text = "CLOSE" } });
    }
    [ServerRpc(RequireOwnership = false)] 
    public void PlayerJoin_ServerRpc(string lobbyID,ulong playerObjectID)
    {
        PlayerStatistics playerStats = new PlayerStatistics();
        playerStats.isHost = false;
        playerStats.playerObjectID = playerObjectID;
        gameStats.playerStats.Add(AuthenticationService.Instance.PlayerId , playerStats);
    }

    //server management vars
    public GameStatistics gameStats = null;
    public void ServerStart()
    {
        gameStats = new GameStatistics();
        gameStats.Created = DateTime.Now;
        gameStats.HostID = AuthenticationService.Instance.PlayerId;
        PlayerStatistics hostStats = new PlayerStatistics();

        hostStats.playerObjectID = Camera.main.GetComponentInParent<NetworkObject>().NetworkObjectId;
        hostStats.isHost = true;

        gameStats.playerStats.Add(AuthenticationService.Instance.PlayerId , hostStats);
    }

    //ping pong system
    [SerializeField] float pingRate;
    float timeTillPing=1;
    ulong? PlayerObjectID = null;
    void pingPong()
    {
        if(!(IsHost||IsServer) || HighLevelNetcodeRef.HighLevelNetcode.currentLobby == null)
            return;
        timeTillPing -= Time.fixedDeltaTime;
        if(timeTillPing <= 0)
        {
            Debug.Log("requesting ping pong");
            timeTillPing = pingRate;
            pingClientRpc(JsonConvert.SerializeObject(gameStats));
        }
    }
    [ClientRpc]
    private void pingClientRpc(string gameStatistic_Json)
    {
        Debug.Log("json string \n" + gameStatistic_Json);
        gameStats = JsonConvert.DeserializeObject<GameStatistics>(gameStatistic_Json);
        if(PlayerObjectID == null)
            PlayerObjectID = Camera.main.GetComponentInParent<NetworkObject>().NetworkObjectId;

        pongServerRpc(DateTime.Now.Ticks,AuthenticationService.Instance.PlayerId,PlayerObjectID.ToString());
    }
    [ServerRpc(RequireOwnership = false)]
    void pongServerRpc(long sentTime , string lobbyPlayerID,string playerObjectID)
    {
        

        var dateTimeSent = new DateTime(sentTime);
        var dif = DateTime.Now - dateTimeSent;
        var pingMs = dif.Milliseconds;
        Debug.Log("ping " + pingMs);
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

    //Local management
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
      spawnVolumes =  HighLevelNetcodeRef.HighLevelNetcode.currentMap.GetComponent<SpawnpointVolumes>().SpawnVolumes;
        if(gameMode != GameMode.ffa)
        {
            List<spawnVolume> newVolumes = new List<spawnVolume>();
            foreach(spawnVolume volume in spawnVolumes)
            {
                if(volume.team== Team)
                    newVolumes.Add(volume);
            }
            spawnVolumes = newVolumes;
        }
    }
    public void setGameMode()
    {
        int enumIndex = int.Parse(HighLevelNetcodeRef.HighLevelNetcode.currentLobby.Data[HighLevelNetcode.KEY_MODE].Value);
        gameMode = ((HighLevelNetcode.GameMode)enumIndex);
    }
    public Vector3 getSpawnPosition()
    {
       int index = Mathf.RoundToInt(UnityEngine.Random.value * spawnVolumes.Count );
        var vol = spawnVolumes[index];
        float x = UnityEngine.Random.Range(-vol.scale.x/2,vol.scale.x/2);
        float y = UnityEngine.Random.Range(-vol.scale.y / 2 , vol.scale.y / 2);
        float z = UnityEngine.Random.Range(-vol.scale.z / 2 , vol.scale.z / 2); ;

        Vector3 pos = vol.pos + new Vector3(x,y,z);
        return pos;
    }
    public void AssignTeam()
    {

    }
}
public class GameStatistics
{
    public DateTime Created;
    public Dictionary<string , PlayerStatistics> playerStats = new Dictionary<string, PlayerStatistics>(); //key is authservice.playerID
    public string HostID;
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