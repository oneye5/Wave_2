using Gravitons.UI.Modal;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class ServerGameManager : NetworkBehaviour
{
    private void Start()
    {
        ServerGameManagerRef.Instance = this;
    }
    [ServerRpc(RequireOwnership = false)]
    public void playerHit_ServerRpc(float damage , string from , string to)
    {
        //get players
        var playerTo = GetNetworkObject(ulong.Parse(to));
        var playerFrom = GetNetworkObject(ulong.Parse(from));

        var healthComp = playerTo.GetComponentInChildren<HealthHandle>();
        healthComp.health.Value -= damage;

        if(healthComp.health.Value < 0)
        {
            healthComp.health.Value = healthComp.defaultHealth;
            playerKilled_ClientRpc(to);
        }

        playerHit_ClientRpc(damage , from , to);
    }
    [ClientRpc]
    public void playerHit_ClientRpc(float damage,string from, string to)
    {
        var playerTo = GetNetworkObject(ulong.Parse(to));
        var playerFrom = GetNetworkObject(ulong.Parse(from));

        if(!playerTo.IsOwner)
            return;

       var visuals =  playerTo.GetComponentInChildren<BulletVisuals>();
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
    
}
public static class ServerGameManagerRef
{
    public static ServerGameManager Instance;
}