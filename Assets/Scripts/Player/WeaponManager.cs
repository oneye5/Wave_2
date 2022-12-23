using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    [HideInInspector] public BulletVisuals visuals;
    [SerializeField] LayerMask targetMask;
    [SerializeField] PlayerUiManager uiManager;
    private Camera cam;
    private void Start()
    {
        visuals = GetComponentInChildren<BulletVisuals>();
        visuals.ChangeWeapon(0);
        cam = Camera.main;
    }
    public List<Weapon> weapons = new List<Weapon>();
    int ActiveWeapon; // refers to index of weapons
    public void AddWeapon(WeaponTypes type)
    {
        Weapon w = new Weapon(type);
        weapons.Add(w);
    }
    public void Tick(PlayerInput input , Transform head) //handels reloading & firing
    {
        //tick all weapons that are not active
        for(int i = 0 ; i < weapons.Count ; i++)
        {
            if(i == ActiveWeapon)
                continue;


            weapons[i].Tick(Time.deltaTime , reload: false , shoot: false , head); //cannot be reloaded or shot due to the weapon not being active
                                                                                   //so those args are hard coded to be false
        }

        var bullets = weapons[ActiveWeapon].Tick(Time.deltaTime , input.reload , input.fire , head);
        if(bullets == null)
            return;


        //only executes if bullet/s exist
        foreach(Bullet b in bullets)
        {
            if(b.ParentWeapon.weaponAttributes.Hitscan)
            {
                handelHitscan(b , cam.transform);
            }
            else
            {
                handelProjectile(b , cam.transform);
            }
        }
    }



    private void handelHitscan(Bullet b , Transform head)
    {
        RaycastHit hit;

        if(Physics.Raycast(head.position , b.Forward , out hit , weapons[ActiveWeapon].weaponAttributes.HitscanRange , targetMask)) //if hit
        {
            Debug.Log("object hit " + hit.transform.gameObject.name);

            if(hit.transform.gameObject.tag == "HeadHitbox")
            {
                float damage = b.ParentWeapon.weaponAttributes.Damage * b.ParentWeapon.weaponAttributes.HeadshotMulti;
                string thisId = NetworkObjectId.ToString();
                string otherId = hit.transform.gameObject.GetComponentInParent<NetworkObject>().NetworkObjectId.ToString();

                ServerGameManagerRef.Instance.playerHit_ServerRpc(damage , thisId , otherId);
                uiManager.showHitMarker(true);
            }
            if(hit.transform.gameObject.tag == "BodyHitbox")
            {

                float damage = b.ParentWeapon.weaponAttributes.Damage;
                string thisId = NetworkObjectId.ToString();
                string otherId = hit.transform.gameObject.GetComponentInParent<NetworkObject>().NetworkObjectId.ToString();

                ServerGameManagerRef.Instance.playerHit_ServerRpc(damage , thisId , otherId);
                uiManager.showHitMarker(false);
            }


            b.hit = true;
            b.Pos = hit.point;
        }
        else
        {
            b.hit = false;
            Vector3 offset = head.transform.forward * b.ParentWeapon.weaponAttributes.HitscanRange;
            b.Pos = offset + head.transform.position;
        }
        visuals.createEffects(b);
    }
    private void handelProjectile(Bullet b , Transform head)
    {

    }
}
