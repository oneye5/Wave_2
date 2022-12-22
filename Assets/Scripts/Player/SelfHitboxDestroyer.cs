using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SelfHitboxDestroyer : NetworkBehaviour
{
    private void Start()
    {
        if(IsOwner)
        {
            var cmp = GetComponent<Collider>();
            Component.Destroy(cmp);
            Debug.Log("Hitbox destroyed");   
            Component.Destroy(this);
                     
        }
        else
        {
            Component.Destroy(this);
        }
    }
}
