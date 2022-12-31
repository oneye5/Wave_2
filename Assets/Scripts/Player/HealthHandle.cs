using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.CullingGroup;

public class HealthHandle : NetworkBehaviour
{
    public float defaultHealth;
    public NetworkVariable<float> health;
    public float publicHealth;
    public void init()
    {
        if(IsHost || IsServer)
            health = new NetworkVariable<float>(100 , NetworkVariableReadPermission.Everyone , NetworkVariableWritePermission.Server);
    }
    public void Update()
    {
        if(IsHost||IsServer)
        publicHealth = health.Value;    
    }
}
