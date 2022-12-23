using Gravitons.UI.Modal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiPageManager : MonoBehaviour
{
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
