using DG.Tweening;
using System.Collections;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

public class Global_Ui_Manager : MonoBehaviour
{

    public GameObject Parent;
    public UiPageManager pageManager;
    public bool menuActive;

    [SerializeField] TMPro.TMP_Dropdown windowMode;
    [SerializeField] Slider fov;
    [SerializeField] TMPro.TMP_Dropdown aaMode;
    [SerializeField] TMPro.TMP_Dropdown materialQuality;
    [SerializeField] Toggle ssr;
    [SerializeField] Toggle bloom;

    [SerializeField] GameObject LoadingScreen;
    private UnityEngine.UI.Image[] LoadingScreenElements;
    bool loadingScreenChanging = false;
    private void Awake()
    {
        Global_Ui_Manager_Ref.Instance = this;
        LoadingScreenElements = LoadingScreen.GetComponentsInChildren<Image>();
        CloseLoadingScreen();
    }
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
        menuActive = true;
    }
    public void CloseMenu()
    {
        Parent.SetActive(false);
        menuActive = false;
    }
    public void LeaveToMenu()
    {
        if(HighLevelNetcodeRef.Instance.currentLobby != null)
            HighLevelNetcodeRef.Instance.selfLeaveGame();

        ServerGameManagerRef.Instance.gameStats = null;
        CloseMenu();
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void ApplyGraphicsSettings()
    {

        GRAPHICS_SETTINGS_MANAGER_REF.Instance.fullscreenMode = windowMode.value;
        if(GRAPHICS_SETTINGS_MANAGER_REF.Instance.fullscreenMode == 2) //do not allow max window
            GRAPHICS_SETTINGS_MANAGER_REF.Instance.fullscreenMode = 3;
        GRAPHICS_SETTINGS_MANAGER_REF.Instance.aaMode = aaMode.value;

        GRAPHICS_SETTINGS_MANAGER_REF.Instance.bloom = bloom.isOn;
        GRAPHICS_SETTINGS_MANAGER_REF.Instance.ssr = ssr.isOn;
        GRAPHICS_SETTINGS_MANAGER_REF.Instance.fov = fov.value;
        GRAPHICS_SETTINGS_MANAGER_REF.Instance.matQuality = materialQuality.value;

        GRAPHICS_SETTINGS_MANAGER_REF.Instance.setCamProperties(Camera.main);


    }


    public void OpenLoadingScreen()
    {
        if(loadingScreenChanging)
            return;
        LoadingScreen.SetActive(true);   
        foreach(var i in LoadingScreenElements)
        {
            i.color = Color.white;
        }
    }
    public void CloseLoadingScreen()
    {
        if(loadingScreenChanging)
            return;
        loadingScreenChanging = true;
        float delay = 5;
        StartCoroutine(setActive(false , LoadingScreen,delay));
        foreach(var i in LoadingScreenElements)
        {
            i.DOColor(Color.clear , delay);
        }
    }
    IEnumerator setActive(bool value , GameObject obj , float delay = 3f)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(value);
        loadingScreenChanging = false;
    }
}
public static class Global_Ui_Manager_Ref
{
    public static Global_Ui_Manager Instance;
}
