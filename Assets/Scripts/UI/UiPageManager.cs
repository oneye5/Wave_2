using Gravitons.UI.Modal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiPageManager : MonoBehaviour
{


    private void Start()
    {
        Debug.Log("starting test thing ");
        ModalManager.Show("Modal Title" , "Show your message here" , new[] { new ModalButton() { Text = "OK" } });
    }
    public List<GameObject> UiPages;
    public void SwitchPage(int i)
    {
        foreach(GameObject page in UiPages)
        {
            page.SetActive(false);
        }
        UiPages[i].SetActive(true);
    }
}
