using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class BrowserUi : MonoBehaviour
{
    HighLevelNetcode netcode;
    [SerializeField] GameObject lobbyItemPrefab;
    [SerializeField] Transform scrollContent;
    private List<GameObject> elements = new List<GameObject>();
    private void Start()
    {
        netcode = NetworkManager.Singleton.gameObject.GetComponent<HighLevelNetcode>();
    }

    public async void Refresh()
    {
        //delete all current elements
        foreach(GameObject element in elements)
        {
           Destroy(element);
        }
        elements.Clear();

        var lobbies = await netcode.getLobbies();
        foreach (var lobby in lobbies)
        {
            var obj = Instantiate(lobbyItemPrefab , scrollContent); 
            var refs = obj.GetComponent<TextReferences>();

            // order = : name,map,mode,region,players
            refs.text[0].text = lobby.Name;
            refs.text[1].text = netcode.mapPrefabs[
                int.Parse(
                    lobby.Data[HighLevelNetcode.KEY_MAP].Value
                    )].name;

            int enumIndex = int.Parse(lobby.Data[HighLevelNetcode.KEY_MODE].Value);
            refs.text[2].text = ((HighLevelNetcode.gameMode)enumIndex).ToString();
            refs.text[3].text = lobby.Data[HighLevelNetcode.KEY_REGION].Value;

            string playerStr = lobby.Players.Count.ToString();
            playerStr += "/";
            playerStr += lobby.MaxPlayers.ToString();
            refs.text[4].text = playerStr;
            
            elements.Add(obj);
        }
    }
}
