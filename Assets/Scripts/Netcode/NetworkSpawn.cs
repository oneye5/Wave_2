using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class NetworkSpawn : NetworkBehaviour
{
    public NetworkObjects nObjects;

    private void Awake()
    {
        NetworkSerializer.spawner = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void spawnObject_ServerRpc(
        string SobjNetworkIndex ,
        string Spos ,
        string Srot ,
        string SobjParentNetworkIndex = "-1",
        string SclientId = "-1") //most types are not allowed so strings are what i use
    {//                     the NetworkSerializer class handels all the string conversions
       var obj = NetworkSerializer.deSerialize_objIndex(SobjNetworkIndex);
        var pos = NetworkSerializer.deSerialize_vector3(Spos);
        var rot = NetworkSerializer.deSerialize_quaternion(Srot);

        //find the parent
        GameObject parent = null;
        if(SobjParentNetworkIndex != "-1")
        {
         //   parent = NetworkSerializer.deSerialize_objIndex(SobjParentNetworkIndex);
           
        }


         var x = Instantiate(obj, pos, rot);
        //x.GetComponent<NetworkObject>().SpawnWithOwnership(ulong.Parse(SclientId));
        x.GetComponent<NetworkObject>().Spawn();
    }

}
public static class NetworkSerializer
{
    public static NetworkSpawn spawner;
    public static GameObject deSerialize_objIndex(string input)
    {
        int i = int.Parse(input);
        var objs = spawner.nObjects.objects;
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
        int i = spawner.nObjects.objects.IndexOf(input);
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

