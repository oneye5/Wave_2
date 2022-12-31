using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global_Ui_Manager : MonoBehaviour
{
    public GameObject Parent;
    public UiPageManager pageManager;
    public void Update()
    {
        HandleMenuOpeningClosing();
    }
    public void HandleMenuOpeningClosing()
    {
       if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(Parent.activeInHierarchy)
            CloseMenu();
            else
                OpenMenu();
        }
    }
    public void OpenMenu()
    {
        pageManager.SwitchPage(0);
        Parent.SetActive(true);
    }
    public void CloseMenu()
    {
        Parent.SetActive(false);
    }
}
