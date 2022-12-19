using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletVisuals : NetworkBehaviour
{

    [Header("bullet vars")]
    [SerializeField] private List<GameObject> HitEffects;
    [SerializeField] private List<GameObject> TrailEffects;
    [SerializeField] private List<GameObject> ProjectileModels;

    [SerializeField] private float TrailTimeMulti;
    [Header("gun vars")]
    [SerializeField] private List<GameObject> MuzzelFlash;
    [SerializeField] private List<Vector3> MuzzelFlashOffset;
    [SerializeField] private List<GameObject> ModelPrefabs;
    public GameObject ModelParrent;

    private GameObject CurrentWeapon;
    private NetworkSpawn spawner;
    private void Start()
    {
       spawner = FindObjectOfType<NetworkSpawn>();
    }
    public void createEffects(Bullet b)
    {
        string sObj;
        string sPos;
        string sRot;
        string sender;
        GameObject obj;
        Vector3 pos;
        Quaternion rot;

        //spawn effects over network


        obj = MuzzelFlash[b.ParentWeapon.weaponAttributes.FlashIndex];
        pos = MuzzelFlashOffset[b.ParentWeapon.weaponAttributes.FlashIndex] + transform.position;
        rot = b.Rot; 
        sObj = NetworkSerializer.serialize_obj(obj); //convert into strings for use over network
        sPos = NetworkSerializer.serialize_vector3(pos);
        sRot = NetworkSerializer.serialize_quaternion(rot);
        sender = this.NetworkObjectId.ToString();
        spawner.spawnObject_ServerRpc(sObj , sPos , sRot , sender); //spawn 


        if(b.hit)
        {
            obj = HitEffects[b.ParentWeapon.weaponAttributes.HitIndex];
            pos = b.Pos;
            rot = b.Rot;
            sObj = NetworkSerializer.serialize_obj(obj);
            sPos = NetworkSerializer.serialize_vector3(pos);
            sRot = NetworkSerializer.serialize_quaternion(rot);
            sender = this.NetworkObjectId.ToString();
            spawner.spawnObject_ServerRpc(sObj , sPos , sRot , sender);
        }


        obj = TrailEffects[b.ParentWeapon.weaponAttributes.TrailIndex];
        pos = this.transform.position;
        rot = b.Rot; 
        sObj = NetworkSerializer.serialize_obj(obj);
        sPos = NetworkSerializer.serialize_vector3(pos);
        sRot = NetworkSerializer.serialize_quaternion(rot);
        sender = this.NetworkObjectId.ToString();
        spawner.spawnObject_ServerRpc(sObj , sPos , sRot , sender);

    }

    public void ChangeWeapon(int index)
    {
        Destroy(CurrentWeapon);
        CurrentWeapon = Instantiate(
            ModelPrefabs[index] ,
            new Vector3(0 , 0 , 0) ,
            Quaternion.Euler(Vector3.forward) ,
            ModelParrent.transform);

        CurrentWeapon.transform.localPosition = Vector3.zero;
    }
}
