using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkObjects : MonoBehaviour
{
    public List<GameObject> objects;
    private void Start()
    {
        foreach (var obj in objects)
        {
            if(obj.TryGetComponent<NetworkObject>(out var x))
            NetworkManager.Singleton.AddNetworkPrefab(obj);
        }
    }
}
