using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyObjectDestroyer : NetworkBehaviour
{
    private void Start()
    {
        if(!IsOwner)
            Destroy(this.gameObject);
    }
}
