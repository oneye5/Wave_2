using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class NetworkSpawn : NetworkBehaviour
{
    #region misc
    public NetworkObjects nObjects;

    private void Awake()
    {
        NetworkSerializer.SpawnerInstance = this;
    }
    #endregion
    #region serverSide spawning
    [ServerRpc(RequireOwnership = false)] //most types are not allowed so strings are what i use
    public void networkSpawnObject_ServerRpc( string SobjNetworkIndex , string Spos ,    string Srot , string SsenderAuthID = "-1", int DataInt = 0,string SsenderNetworkObject = "-1") //dataInt contains a prefix which defines where to send the data  
    {
        Debug.Log("SERVER SPAWNING " + DataInt);
       var obj = NetworkSerializer.deSerialize_objIndex(SobjNetworkIndex);
        var pos = NetworkSerializer.deSerialize_vector3(Spos);
        var rot = NetworkSerializer.deSerialize_quaternion(Srot);

         var x = Instantiate(obj, pos, rot);
        x.GetComponent<NetworkObject>().Spawn();

        if(DataInt != 0) // 1000 = projectile 
        {
            if(DataInt.ToString()[0] == '1') //projectile
            {
                DataInt -= 1000;
                var cmp =  x.GetComponent<ServerProjectile_Handel>();
                cmp.SourceIndex = DataInt;
                cmp.senderAuth = SsenderAuthID;
                cmp.senderObjId = SsenderNetworkObject;
                cmp.Init();
            }
          
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void force_playerExplosion_ServerRpc(string Spos,string Sforce,string Sradius)
    {
        force_playerExplosion_ClientRpc(Spos, Sforce, Sradius);
    }
    [ClientRpc]
    public void force_playerExplosion_ClientRpc(string Spos , string Sforce , string Sradius)
    {
        var pos = NetworkSerializer.deSerialize_vector3(Spos);
        float force =  float.Parse( Sforce);
        float radius = float.Parse( Sradius);
        float upModifier = 0;
        MainPlayer.Instance.rb.AddExplosionForce(force,pos,radius,upModifier);
    }
    #endregion
    #region local spawning
    [ClientRpc]
    private void localSpawnObject_ClientRpc(string SobjNetworkIndex , string Spos , string Srot , string SclientId)
    {
      if(SclientId == AuthenticationService.Instance.PlayerId) //if is sender, return. object has already been spawned by itself
          return;

        var obj = NetworkSerializer.deSerialize_objIndex(SobjNetworkIndex);
        var pos = NetworkSerializer.deSerialize_vector3(Spos);
        var rot = NetworkSerializer.deSerialize_quaternion(Srot);

        var x = Instantiate(obj , pos , rot);
    }


    [ServerRpc(RequireOwnership = false)]
    private void localSpawnObject_ServerRpc(string SobjNetworkIndex , string Spos , string Srot  , string SclientId)
    {
        localSpawnObject_ClientRpc(SobjNetworkIndex , Spos , Srot , SclientId); 
    }



    public void localSpawnObject(string SobjNetworkIndex , string Spos , string Srot , string SclientId = "-1")
    { 
        if(SclientId == AuthenticationService.Instance.PlayerId) //if sent from this client, spawn instantly
        {
            var obj = NetworkSerializer.deSerialize_objIndex(SobjNetworkIndex);
            var pos = NetworkSerializer.deSerialize_vector3(Spos);
            var rot = NetworkSerializer.deSerialize_quaternion(Srot);

            var x = Instantiate(obj , pos , rot);
        }
        localSpawnObject_ServerRpc(SobjNetworkIndex, Spos , Srot ,  SclientId); //data sent to server, server sends to every client
    }
    #endregion
}
public static class NetworkSerializer
{
    public static NetworkSpawn SpawnerInstance;
    public static GameObject deSerialize_objIndex(string input)
    {
        int i = int.Parse(input);
        var objs = SpawnerInstance.nObjects.objects;
        if(i > objs.Count())
        {
            Debug.Log("NetworkSerializer.DeserializeObj : input larger than object list");
            return null;
        }
        if(i == -1)
            return null;

        return objs[i];
    }
    public static Vector3 deSerialize_vector3(string input)
    {
        List<string> args = input.Split(',').ToList();
        Vector3 Out = new Vector3(
            float.Parse(args[0]) ,
            float.Parse(args[1]) ,
            float.Parse(args[2])
            );
        return Out;
    }
    public static Quaternion deSerialize_quaternion(string input)
    {

        List<string> args = input.Split(',').ToList();
        Quaternion Out = new Quaternion(
            float.Parse(args[0]) ,
            float.Parse(args[1]) ,
            float.Parse(args[2]),
            float.Parse(args[3])
            );
        return Out;
    }

    //serializer

    public static string serialize_obj(GameObject input)
    {
        int i = SpawnerInstance.nObjects.objects.IndexOf(input);
        if(i < 0)
        {
            Debug.Log("NetworkSerializer.serializeObj : GameObject is not a network object");
            return "-1";
        }

        return i.ToString();
    }
    public static string serialize_vector3(Vector3 input)
    {
        string output = input.ToString();
        output = output.Replace("(" , "");
        output = output.Replace(")" , "");
        return output;
    }
    public static string serialize_quaternion(Quaternion input)
    {
        string output = input.ToString();
        output = output.Replace("(" , "");
        output = output.Replace(")" , "");
        return output;
    }
}

