using System;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public enum WeaponTypes
{
    Sniper,
    RocketLauncher
}
public class WeaponAttributes
{
    public int MagSize;
    public float FireTime; // secconds per shot
    public float ReloadTime;


    public int SpawnCount; //amount of bullets / projectiles

    public float Inaccuracy; // in degrees

    public float Damage;
    public float HeadshotMulti;
    public bool Hitscan;

    public int ModelIndex; // weapon model
    public int TrailIndex;
    public int HitIndex;
    public int FlashIndex;

    public float HitscanRange;

    public float ProjectileGravity;
    public float ProjectileImpulse;
    public float ProjectileDrag;
    public float ProjectileLifetime;

    public bool AreaDamage;

    public float AreaRadius;
    public float AreaImpulse;

    //recoil
   public Vector3 Recoil_camJump;
   public float   Recoil_camJumpRecovery;
   public float   Recoil_camShakeStr;
   public float   Recoil_camShakeDuration;
   public float   Recoil_camJumpTime;

   public Vector3 Recoil_weaponJumpRot;
   public float   Recoil_weaponJumpRecovery;
   public Vector3 Recoil_weaponPosJump;
   public float   Recoil_weaponJumpTime;
   public float   Recoil_weaponShakeStr;
   public float   Recoil_weaponShakeDuration;

    public WeaponAttributes(WeaponTypes type)
    {
        switch(type)
        {
            case WeaponTypes.Sniper:
                MagSize = 1;
                FireTime = 1;
                ReloadTime = 0;
                SpawnCount = 1;
                Inaccuracy = 0;
                Damage = 75;
                HeadshotMulti = 1.5f;
                Hitscan = true;
                ModelIndex = 0;
                HitIndex = 0;
                ModelIndex = 0;
                TrailIndex = 0;
                FlashIndex = 0;
                HitIndex = 0;
                HitscanRange = 100;
                AreaDamage = false;

                Recoil_camJump = new Vector3(-25 , 0 , 0);
                Recoil_camJumpRecovery = 0.75f;
                Recoil_camJumpTime = 0.1f;
                Recoil_camShakeDuration = 0.25f;
                Recoil_camShakeStr = 3;

                Recoil_weaponJumpRecovery = 0.9f;
                Recoil_weaponJumpRot = new Vector3(-25 , 0 , 0);
                Recoil_weaponJumpTime = 0.2f;
                Recoil_weaponPosJump = new Vector3(0 , 0 , -0.05f);
                Recoil_weaponShakeDuration = 0.25f;
                Recoil_weaponShakeStr = 5f;
                break;

            case WeaponTypes.RocketLauncher:
                MagSize = 4;
                FireTime = 1;
                ReloadTime = 2;
                SpawnCount = 1;
                Inaccuracy = 0;
                Damage = 50;
                HeadshotMulti = 1.0f;
                Hitscan = false;
                ModelIndex = 1;
                TrailIndex = 1;
                HitIndex = 1;
                FlashIndex = 1;

                ProjectileGravity = 0;
                ProjectileImpulse = 10;
                ProjectileDrag = 0;
                ProjectileLifetime = 5;

                AreaDamage = true;
                AreaImpulse = 10;
                AreaRadius = 5;


                Recoil_camJump = new Vector3(-2 , 0 , 0);
                Recoil_camJumpRecovery = 0.75f;
                Recoil_camJumpTime = 0.1f;
                Recoil_camShakeDuration = 0.25f;
                Recoil_camShakeStr = 1;

                Recoil_weaponJumpRecovery = 0.9f;
                Recoil_weaponJumpRot = new Vector3(-10 , 0 , 0);
                Recoil_weaponJumpTime = 0.2f;
                Recoil_weaponPosJump = new Vector3(0 , 0 , -0.05f);
                Recoil_weaponShakeDuration = 0.25f;
                Recoil_weaponShakeStr = 5f;
                break;
        }
    }
}
public class Bullet
{
    public Vector3 Pos;
    public Vector3 Forward;
    public Quaternion Rot;
    public Vector3 vel;
    public bool hit;
    public Weapon ParentWeapon;
}
public class Weapon
{
    public WeaponTypes weaponType;
    public WeaponAttributes weaponAttributes;

    public int mag; //dynamic vars. Attributes are static
    public float remainingFireTime;
    public float remainingReloadTime;

    public List<Bullet> Tick(float deltaTime , bool reload , bool shoot , Transform Head )
    {
        Debug.Log("reload time " + remainingReloadTime + " firetime " + remainingFireTime + " mag " + mag);

        if(remainingFireTime > 0)
            remainingFireTime -= deltaTime;
       

        if(remainingReloadTime > 0)
        {
            remainingReloadTime -= deltaTime;

            if(remainingReloadTime < 0)
            {
                mag = weaponAttributes.MagSize;
            }
        }

        if(mag <= 0 && remainingReloadTime <= 0)
            remainingReloadTime = weaponAttributes.ReloadTime;
        

        if(!shoot || remainingReloadTime > 0 || mag == 0 || remainingFireTime > 0) //if not shooting return null
            return null;

        remainingFireTime = weaponAttributes.FireTime;
        mag--;

       // if(mag <= 0)
         //   remainingReloadTime = weaponAttributes.ReloadTime;


        List<Bullet> bullets = new List<Bullet>();
        for(int i = 0 ; i < weaponAttributes.SpawnCount ; i++) //create bullet objects to be instantiated by the bullet handel as monobehaviours 
        {
            Bullet bullet = new Bullet();
            bullet.Pos = Head.position;

            //make rotation with randomization
            var originalRot = Head.transform.rotation;
            //randomize
            Vector3 newRot = originalRot.eulerAngles;
            newRot.x = newRot.x + Random.Range(-weaponAttributes.Inaccuracy , weaponAttributes.Inaccuracy);
            newRot.y = newRot.y + Random.Range(-weaponAttributes.Inaccuracy , weaponAttributes.Inaccuracy);
            newRot.z = newRot.z + Random.Range(-weaponAttributes.Inaccuracy , weaponAttributes.Inaccuracy);

            Head.transform.rotation =  Quaternion.Euler(newRot);
            var forward = Head.transform.forward;
            var rot = Head.rotation;
            //set back to original
            Head.rotation = originalRot;

            bullet.Rot = rot;
            bullet.Forward = forward;
            bullet.ParentWeapon = this;
            bullet.vel = Head.forward * weaponAttributes.ProjectileImpulse;
            bullets.Add(bullet);
        }

        return bullets;
    }

    public Weapon(WeaponTypes type)
    {
        weaponAttributes = new WeaponAttributes(type);
        mag = weaponAttributes.MagSize;
        remainingFireTime = 0;
        remainingReloadTime = weaponAttributes.ReloadTime;
    }
}

