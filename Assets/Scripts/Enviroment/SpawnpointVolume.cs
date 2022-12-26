using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnpointVolume : MonoBehaviour
{
    public  void addVol(SpawnpointVolumes v)
    {
        var newV = new spawnVolume();
        newV.pos = transform.position;
        newV.scale = transform.localScale;
        if(gameObject.layer == LayerMask.NameToLayer("Team1"))
        {
            newV.team = 0;
        }
        else
            newV.team = 1;
        v.SpawnVolumes.Add(newV);
        Destroy(gameObject);
    }
}
