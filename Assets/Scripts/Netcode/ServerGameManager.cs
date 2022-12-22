using Unity.Netcode;

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
    }
    [ClientRpc]
    public void playerKilled_ClientRpc(string id)
    {
        var player = GetNetworkObject(ulong.Parse(id));
        if(!player.IsOwner)
            return;

        player.GetComponent<PlayerManager>().ResetPlayer();

    }
}
public static class ServerGameManagerRef
{
    public static ServerGameManager Instance;
}