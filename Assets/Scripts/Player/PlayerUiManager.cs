using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUiManager : MonoBehaviour
{
    private float timeTillSeccondTick;
    [SerializeField] HealthHandle healthHandle;
    [SerializeField] TMPro.TextMeshProUGUI healthText;


    public Image hitMarker;
    public Color hitCol;
    public Color critCol;
    public float hitMarkerTime;
    private float timeTillHide;

    public TMPro.TextMeshProUGUI fpsText;
    public TMPro.TextMeshProUGUI pingText;

    [SerializeField] GameObject playerListElement;
    [SerializeField] GameObject playerListContent;
    [SerializeField] GameObject playerListCanvas;
    List<GameObject> playerListElements = new List<GameObject>();
    List<TextReferences> playerList_TextRefs = new List<TextReferences>();
    List<string> playerAuthIdPerElem = new List<string>();
    private void Start()
    {
        hitMarker.color = Color.clear;

        if(healthHandle.IsOwner == false)
        {
            Debug.Log("health handle incorrect, getting correct one");
            healthHandle =  this.GetComponentInParent<NetworkObject>().GetComponentInChildren<HealthHandle>();
        }
    }
    public void Tick()
    {
        tickHitMarker();
        tickHealthText();
        oneSeccondTick();
        TickTabPlayerMenu();
    }
    public void oneSeccondTick()
    {
        timeTillSeccondTick -= Time.deltaTime;
        if(timeTillSeccondTick <= 0)
            timeTillSeccondTick = 1;
        else
            return;

        //loop code below 

        tickClientStats();
    }
    public void showHitMarker(bool crit)
    {
        if(crit)
            hitMarker.color = critCol;
        else
            hitMarker.color = hitCol;

        timeTillHide = hitMarkerTime;
    }
    public void tickHitMarker()
    {
        if(timeTillHide < 0)
            hitMarker.color = Color.clear;
        else
            timeTillHide -= Time.deltaTime;
    }
    public void tickHealthText()
    {
        
        healthText.text = healthHandle.publicHealth.ToString();
    }
    public void tickClientStats() //such as fps and ping
    {
        fpsText.text = Mathf.Round(1.0f/ Time.smoothDeltaTime).ToString();
        if(ServerGameManagerRef.Instance.gameStats != null && ServerGameManagerRef.Instance.gameStats.playerStats != null) //just to avoid errors
            if(ServerGameManagerRef.Instance.gameStats.playerStats.TryGetValue(AuthenticationService.Instance.PlayerId,out PlayerStatistics stats))
            {
                pingText.text = Mathf.RoundToInt(stats.ping).ToString();
                return;
            }


        pingText.text = "0";
    }
    public void TickTabPlayerMenu()
    {
        if(Input.GetKey(KeyCode.Tab))
        {
            Cursor.lockState = CursorLockMode.None;
            playerListCanvas.SetActive(true);
            RefreshPlayerList();
        }
        else if(playerListCanvas.activeInHierarchy) //if active turn off
        {
            playerListCanvas.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    public void RefreshPlayerList()
    {
        if(HighLevelNetcodeRef.Instance.currentLobby.Players.Count != playerListElements.Count) //if needed, recreate all elements
        {
            foreach(var obj in playerListElements)
            {
                Destroy(obj);
            }
            playerListElements.Clear();
            playerList_TextRefs.Clear();
            playerAuthIdPerElem.Clear();

            //all destroyed, now create

            var players = HighLevelNetcodeRef.Instance.currentLobby.Players;
            for(int i = 0 ; i < players.Count ; i++)
            {
                var player = players[i];
                var element = Instantiate(playerListElement , playerListContent.transform);
                var textRef = element.GetComponent<TextReferences>();
                var AuthID = player.Id;

                var playerStats = ServerGameManagerRef.Instance.gameStats.playerStats[AuthID];

                //text ref order is = NAME,Ping,KILLS,DEATHS,DAMAGE,DAMAGETAKEN
                textRef.text[0].text = AuthID; //to be replaced with name, names not yet implimented
                textRef.text[1].text = playerStats.ping.ToString();
                textRef.text[2].text = playerStats.kills.ToString();
                textRef.text[3].text = playerStats.deaths.ToString();
                textRef.text[4].text = playerStats.damageDone.ToString();
                textRef.text[5].text = playerStats.damageTaken.ToString();

                playerListElements.Add(element);
                playerList_TextRefs.Add(textRef);
                playerAuthIdPerElem.Add(AuthID);
            }

        }
        else //if elements do not need to be created or destroyed, just refresh them
        {
            for(int i = 0 ; i < playerListElements.Count ; i++)
            {
                var element = playerListElements[i];
                var textRef = playerList_TextRefs[i];
                var AuthID = playerAuthIdPerElem[i];

                var playerStats = ServerGameManagerRef.Instance.gameStats.playerStats[AuthID];

                //text ref order is = NAME,Ping,KILLS,DEATHS,DAMAGE,DAMAGETAKEN
                textRef.text[0].text = AuthID; //to be replaced with name, names not yet implimented
                textRef.text[1].text = playerStats.ping.ToString();
                textRef.text[2].text = playerStats.kills.ToString();
                textRef.text[3].text = playerStats.deaths.ToString();
                textRef.text[4].text = playerStats.damageDone.ToString();
                textRef.text[5].text = playerStats.damageTaken.ToString();
            }
        }
    }
}
