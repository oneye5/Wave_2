using Gravitons.UI.Modal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BrowserUi : MonoBehaviour
{
    [SerializeField] HighLevelNetcode netcode;
    [SerializeField] GameObject lobbyItemPrefab;
    [SerializeField] Transform scrollContent;
    private List<GameObject> elements = new List<GameObject>();
    [SerializeField] private List<Lobby> elementLobbies = new List<Lobby>();

    public async void Refresh()
    {
        //delete all current elements
        foreach(GameObject element in elements)
        {
           Destroy(element);
        }
        elements.Clear();
        elementLobbies.Clear();

        List<Lobby> lobbies = await netcode.getLobbies();
        elementLobbies = lobbies;
        for(int i = 0 ; i < lobbies.Count() ; i++)
        {
            Lobby lobby = lobbies[i];
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
    public async void Join()
    {
        for(int i = 0 ; i < elements.Count ; i++)
        {
            if(!elements[i].GetComponent<selectableState>().isSelected)
                continue;
           await netcode.JoinGame(elementLobbies[i]);
            return;
        }
    }

}
