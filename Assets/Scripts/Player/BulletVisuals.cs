using DG.Tweening;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class BulletVisuals : NetworkBehaviour
{

    [Header("bullet vars")]
    public List<GameObject> HitEffects;
    public List<GameObject> TrailEffects;
    public List<GameObject> ProjectileModels;

    [SerializeField] private float TrailTimeMulti;
    [Header("gun vars")]
    [SerializeField] private List<GameObject> MuzzelFlash;
    [SerializeField] private List<Vector3> MuzzelFlashOffset;
    [SerializeField] private List<GameObject> ModelPrefabs;


    public GameObject ModelParrent;
    private Camera cam;

    private GameObject CurrentWeapon;
    private NetworkSpawn spawner;
    private void Start()
    {
       spawner = FindObjectOfType<NetworkSpawn>();
        cam = Camera.main;
    }
    public void createEffectsHitscan(Bullet b)
    {
        //spawn
        string sObj;
        string sPos;
        string sRot;
        string sender;
        GameObject obj;
        Vector3 pos;
        Quaternion rot;

        //spawn effects over network
        Vector3 gunPos = CurrentWeapon.GetComponentInChildren<TransformGetter>().get().position;
        Vector3 flashOffset =  MuzzelFlashOffset[b.ParentWeapon.weaponAttributes.FlashIndex] ;
        flashOffset = new Vector3(flashOffset.x * transform.forward.x , flashOffset.y * transform.forward.y , flashOffset.z * transform.forward.z);
        //fix later
        flashOffset = Vector3.zero;

        obj = MuzzelFlash[b.ParentWeapon.weaponAttributes.FlashIndex];
        pos = flashOffset + gunPos;
        rot = b.Rot; 
        sObj = NetworkSerializer.serialize_obj(obj); //convert into strings for use over network
        sPos = NetworkSerializer.serialize_vector3(pos);
        sRot = NetworkSerializer.serialize_quaternion(rot);
        sender = AuthenticationService.Instance.PlayerId;
        spawner.localSpawnObject(sObj , sPos , sRot , sender); //spawn 


        if(b.hit)
        {
            obj = HitEffects[b.ParentWeapon.weaponAttributes.HitIndex];
            pos = b.Pos;
            rot = b.Rot;
            sObj = NetworkSerializer.serialize_obj(obj);
            sPos = NetworkSerializer.serialize_vector3(pos);
            sRot = NetworkSerializer.serialize_quaternion(rot);
            spawner.localSpawnObject(sObj , sPos , sRot , sender);
        }


        obj = TrailEffects[b.ParentWeapon.weaponAttributes.TrailIndex];
        pos = flashOffset + gunPos;
        rot = b.Rot; 
        sObj = NetworkSerializer.serialize_obj(obj);
        sPos = NetworkSerializer.serialize_vector3(pos);
        sRot = NetworkSerializer.serialize_quaternion(rot);
        spawner.localSpawnObject(sObj , sPos , sRot , sender);


        //RECOIL ______________
        var attributes = b.ParentWeapon.weaponAttributes;
        recoilClimb(cam.transform , attributes.Recoil_camJump , attributes.Recoil_camJumpTime, attributes.Recoil_camJumpRecovery);
        recoilPunchRandom(cam.transform, attributes.Recoil_camShakeStr, attributes.Recoil_camShakeDuration);

        recoilClimb(ModelParrent.transform , attributes.Recoil_weaponJumpRot , attributes.Recoil_weaponJumpTime, attributes.Recoil_weaponJumpRecovery);
        recoilMove(ModelParrent.transform , attributes.Recoil_weaponPosJump , attributes.Recoil_weaponJumpRecovery);
        recoilPunchRandom(ModelParrent.transform , attributes.Recoil_weaponShakeStr , attributes.Recoil_weaponShakeDuration);
    }
    public void createEffectsProjectile(Bullet b)
    {
        //spawn
        string sObj;
        string sPos;
        string sRot;
        string sender;
        GameObject obj;
        Vector3 pos;
        Quaternion rot;

        //spawn effects over network
        Vector3 gunPos = CurrentWeapon.GetComponentInChildren<TransformGetter>().get().position;
        Vector3 flashOffset = MuzzelFlashOffset[b.ParentWeapon.weaponAttributes.FlashIndex];
        flashOffset = new Vector3(flashOffset.x * transform.forward.x , flashOffset.y * transform.forward.y , flashOffset.z * transform.forward.z);
        //fix later
        flashOffset = Vector3.zero;

        obj = MuzzelFlash[b.ParentWeapon.weaponAttributes.FlashIndex];
        pos = flashOffset + gunPos;
        rot = b.Rot;
        sObj = NetworkSerializer.serialize_obj(obj); //convert into strings for use over network
        sPos = NetworkSerializer.serialize_vector3(pos);
        sRot = NetworkSerializer.serialize_quaternion(rot);
        sender = AuthenticationService.Instance.PlayerId;

        obj = MuzzelFlash[b.ParentWeapon.weaponAttributes.FlashIndex];
        pos = flashOffset + gunPos;
        rot = b.Rot;
        sObj = NetworkSerializer.serialize_obj(obj); //convert into strings for use over network
        sPos = NetworkSerializer.serialize_vector3(pos);
        sRot = NetworkSerializer.serialize_quaternion(rot);
        sender = AuthenticationService.Instance.PlayerId;
        spawner.localSpawnObject(sObj , sPos , sRot , sender); //spawn flash

        //spawn projectile ====================================================================================================================
        obj = ProjectileModels[b.ParentWeapon.weaponAttributes.ProjectileIndex];
        pos = flashOffset + gunPos;
        rot = b.Rot;
        sObj = NetworkSerializer.serialize_obj(obj); //convert into strings for use over network
        sPos = NetworkSerializer.serialize_vector3(pos);
        sRot = NetworkSerializer.serialize_quaternion(rot);
        sender = AuthenticationService.Instance.PlayerId;
        spawner.networkSpawnObject_ServerRpc(sObj , sPos , sRot , sender , (int)(1000 +((int) b.ParentWeapon.weaponType)),NetworkObjectId.ToString());




        //RECOIL ______________
        var attributes = b.ParentWeapon.weaponAttributes;
        recoilClimb(cam.transform , attributes.Recoil_camJump , attributes.Recoil_camJumpTime , attributes.Recoil_camJumpRecovery);
        recoilPunchRandom(cam.transform , attributes.Recoil_camShakeStr , attributes.Recoil_camShakeDuration);

        recoilClimb(ModelParrent.transform , attributes.Recoil_weaponJumpRot , attributes.Recoil_weaponJumpTime , attributes.Recoil_weaponJumpRecovery);
        recoilMove(ModelParrent.transform , attributes.Recoil_weaponPosJump , attributes.Recoil_weaponJumpRecovery);
        recoilPunchRandom(ModelParrent.transform , attributes.Recoil_weaponShakeStr , attributes.Recoil_weaponShakeDuration);
    }

    public void ChangeWeapon(int index)
    {
        Destroy(CurrentWeapon);
        if(index == -1)
            return;


        CurrentWeapon = Instantiate(
            ModelPrefabs[index] ,
            new Vector3(0 , 0 , 0) ,
            Quaternion.Euler(Vector3.zero) ,
            ModelParrent.transform);

        CurrentWeapon.transform.localPosition = Vector3.zero;
        CurrentWeapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
    public void recoilClimb(Transform t,Vector3 climb,float jumpTime,float recovery)
    {
        t.localRotation = Quaternion.Euler(Vector3.zero);
        t.DOBlendableLocalRotateBy( climb , jumpTime);
        t.DOBlendableLocalRotateBy( -climb , recovery);
      
    }
    public void recoilMove(Transform t,Vector3 jump,float recovery)
    {
        t.localPosition += jump;
        t.DOBlendableLocalMoveBy(-jump , recovery);
    }
    public void recoilPunchRandom(Transform t, float climb,float duration)
    {
        Vector3 random = Random.rotation.eulerAngles;
        random = random / 360;
        t.DOBlendablePunchRotation(random * climb , duration);
    }
}
