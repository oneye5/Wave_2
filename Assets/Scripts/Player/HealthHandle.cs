using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HealthHandle : NetworkBehaviour
{
    public float defaultHealth;
    public NetworkVariable<float> health = new NetworkVariable<float>(100,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public float publicHealth;
 
    private void Awake()
    {
        if(IsHost||IsServer)
        health.Value = defaultHealth;
    }
    private void Update()
    {
        publicHealth = this.health.Value;
    }
}
