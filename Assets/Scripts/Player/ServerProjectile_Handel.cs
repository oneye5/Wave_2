using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.VisualScripting;
using UnityEngine;

public class ServerProjectile_Handel : NetworkBehaviour
{
    [SerializeField] float lifetime;
       [HideInInspector] public int SourceIndex;
    [SerializeField] Rigidbody rb;
    float gravity;
       public string senderAuth;
    public string senderObjId;
    public void Init()
    {
        if(!(IsHost || IsServer))
        {
            Component.Destroy(this);
            return;
        }
         
        var attributes = WeaponStatsCache.weaponAttributes[SourceIndex];
        rb.AddForce(transform.forward * attributes.ProjectileImpulse , ForceMode.Impulse);
        gravity = Mathf.Abs( attributes.ProjectileGravity);
        

        StartCoroutine(kill(attributes.ProjectileLifetime));
    }
    private void FixedUpdate()
    {
        if(gravity != 0)
        rb.AddForce(new Vector3(0 , -gravity , 0) , ForceMode.Acceleration);
    }
    private void OnTriggerEnter(Collider hit)
    {
        if(hit.tag == "Player")
            return;

        if(WeaponStatsCache.weaponAttributes[SourceIndex].AreaDamage)
        {
            Splode();
            return;
        }
        else
        {
            DirectHit(hit);
            return;
        }
    }

    private void Splode()
    {
        
        //get players in radius
        var hits =Physics.SphereCastAll(new Ray(transform.position , transform.forward) , WeaponStatsCache.weaponAttributes[SourceIndex].AreaRadius);
       Debug.DrawLine(transform.position,transform.position + new Vector3(0, WeaponStatsCache.weaponAttributes[SourceIndex].AreaRadius,0),Color.red,5.0f);
        Debug.Log("exploding projectile, hits count " + hits.Length );
        List<GameObject> HitPlayers = new List<GameObject>();
        foreach(var hit in hits)
        {
            Debug.Log(hit.collider.name);
            if(hit.collider.tag == "BodyHitbox")
            {
                HitPlayers.Add( hit.collider.gameObject);
                Debug.Log("player hit by projectile");
                continue;
            }
            if(hit.collider.tag == "HeadHitbox")
            {
                HitPlayers.Add(hit.collider.gameObject);
                Debug.Log("player hit by projectile");
                continue;
            }
            if(hit.collider.tag == "Player")
            {
                HitPlayers.Add(hit.collider.gameObject);
                Debug.Log("player hit by projectile");
                continue;
            }
        }


        //check los to player
        foreach(var x in HitPlayers)
        {
            Debug.Log("checking los to Player");
            LayerMask mask = new LayerMask();
            mask.value = LayerMask.GetMask("STATIC_MAP");
            bool blocked =Physics.Linecast(transform.position , x.transform.position,mask);
            Debug.DrawLine(transform.position , x.transform.position , Color.green , 5.0f);
            if(blocked)
                continue;
            else //player is hit by explosion
            {
               var toID = x.GetComponentInParent<NetworkObject>().NetworkObjectId.ToString();
                float distance = Vector3.Distance(x.transform.position , transform.position);
                float damage = WeaponStatsCache.weaponAttributes[SourceIndex].Damage;

                float distNormalized = (WeaponStatsCache.weaponAttributes[SourceIndex].AreaRadius - distance) / WeaponStatsCache.weaponAttributes[SourceIndex].AreaRadius;
                distNormalized = Mathf.Abs(distNormalized) + 0.25f;
                distNormalized = Mathf.Clamp(distNormalized , 0 , 1);
                
                damage = damage * distNormalized;
                damage = Mathf.Round(damage);

                ServerGameManagerRef.Instance.playerHit_ServerRpc(damage , senderObjId , toID,1);

                string pos = NetworkSerializer.serialize_vector3(transform.position);
                NetworkSerializer.SpawnerInstance.force_playerExplosion_ServerRpc(pos , WeaponStatsCache.weaponAttributes[SourceIndex].AreaImpulse.ToString() , WeaponStatsCache.weaponAttributes[SourceIndex].AreaRadius.ToString()); 
            }
        }
       
    }
    private void DirectHit(Collider hit)
    {

    }
    IEnumerator kill(float delay)
    {
        yield return new WaitForSeconds(delay);

       
        GetComponent<NetworkObject>().Despawn();
        Destroy(this.gameObject);

    }
}
