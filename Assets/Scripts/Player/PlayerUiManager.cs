using System.Collections;
using System.Collections.Generic;
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
    private void Start()
    {
        hitMarker.color = Color.clear;
    }
    public void Tick()
    {
        tickHitMarker();
        tickHealthText();
        oneSeccondTick();
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
        if(ServerGameManagerRef.Instance.gameStats.playerStats != null)
            if(ServerGameManagerRef.Instance.gameStats.playerStats.TryGetValue(AuthenticationService.Instance.PlayerId,out PlayerStatistics stats))
            {
                pingText.text = Mathf.RoundToInt(stats.ping).ToString();
                return;
            }


        pingText.text = "0";
    }
}
