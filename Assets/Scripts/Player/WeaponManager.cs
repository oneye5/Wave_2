using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    BulletVisuals visuals;
    private void Start()
    {
      visuals = GetComponentInChildren<BulletVisuals>();
        visuals.ChangeWeapon(0);
    }
    List<Weapon> weapons = new List<Weapon>();
    int ActiveWeapon; // refers to index of weapons
    public void AddWeapon(WeaponTypes type)
    {
        Weapon w = new Weapon(type);
        weapons.Add(w);
    }
    public void Tick(PlayerInput input,Transform head) //handels reloading & firing
    {
       //tick all weapons that are not active
       for(int i = 0; i < weapons.Count ; i++)
        {
            if(i == ActiveWeapon)
                continue;


            weapons[i].Tick(Time.deltaTime ,reload: false ,shoot: false , head); //cannot be reloaded or shot due to the weapon not being active
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
                handelHitscan(b , head);
            }
            else
            {
                handelProjectile(b , head);
            }
        }
    }

    

    private void handelHitscan(Bullet b,Transform head)
    {
        RaycastHit hit;
        if(Physics.Raycast(head.position ,b.Forward, out hit , weapons[ActiveWeapon].weaponAttributes.HitscanRange)) //if hit
        {
            //handle damage
         //   DamageHandel d;
        //  if(hit.transform.gameObject.TryGetComponent<DamageHandel>(out d)) // if damageable
      //    {
     //       d.Hit(b);
        //  }


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
    private void handelProjectile(Bullet b, Transform head)
    {

    }
}
