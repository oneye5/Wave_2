using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnpointVolumes : MonoBehaviour
{
    public List<spawnVolume> SpawnVolumes;
    private void Awake()
    {
        SpawnVolumes = new List<spawnVolume>();
        var volumes = GetComponentsInChildren<SpawnpointVolume>();
        foreach (var volume in volumes)
        {
            volume.addVol(this);
        }
    }
}
public class spawnVolume
{
  public  Vector3 pos;
   public Vector3 scale;
    public int team;
}