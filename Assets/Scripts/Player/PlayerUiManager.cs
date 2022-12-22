using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUiManager : MonoBehaviour
{
    [SerializeField] HealthHandle healthHandle;
    [SerializeField] TMPro.TextMeshProUGUI healthText;


    public Image hitMarker;
    public Color hitCol;
    public Color critCol;
    public float hitMarkerTime;
    private float timeTillHide;
    private void Start()
    {
        hitMarker.color = Color.clear;
    }
    public void Tick()
    {
        tickHitMarker();
        tickHealthText();
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
}
