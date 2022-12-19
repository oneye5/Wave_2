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
            NetworkManager.Singleton.AddNetworkPrefab(obj);
        }
    }
}
