using DG.Tweening;
using System.Collections;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.XR;
using Cursor = UnityEngine.Cursor;

public class PlayerManager : NetworkBehaviour
{
    PlayerInput playerInput;
    HeadMovement headMovement;
    public BodyMovement bodyMovement;
    SmoothHead smoothHead;
    public Rigidbody rb; // main rb of body
    public WeaponManager weaponManager;
    Camera cam;
    public HealthHandle healthHandle;
    public PlayerUiManager uiManager;

    private bool resetting = false;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        headMovement = GetComponentInChildren<HeadMovement>();
        bodyMovement = GetComponentInChildren<BodyMovement>();
        smoothHead = GetComponentInChildren<SmoothHead>();
        rb = bodyMovement.gameObject.GetComponent<Rigidbody>();
        weaponManager = GetComponentInChildren<WeaponManager>();
        healthHandle = GetComponentInChildren<HealthHandle>();
        cam = GetComponentInChildren<Camera>();
        uiManager = GetComponentInChildren<PlayerUiManager>();
        healthHandle.init();
        if(!IsOwner)
        {
            Debug.Log("player is not owner");
            cam.enabled = false;
            GetComponentInChildren<AudioListener>().enabled = false;
            var visuals = GetComponentInChildren<BulletVisuals>();
            var parent = visuals.ModelParrent;
            visuals.ModelParrent = headMovement.gameObject;
            Destroy(uiManager.gameObject);
        }
        else
        {
            if(!(IsHost || IsServer))
                ServerGameManagerRef.Instance.PlayerJoin_ServerRpc(
                    AuthenticationService.Instance.PlayerId ,
                    GetComponent<NetworkObject>().NetworkObjectId);

            ServerGameManagerRef.Instance.GameStart();
            ResetPlayer();
            GRAPHICS_SETTINGS_MANAGER_REF.Instance.setCamProperties(cam);
            MainPlayer.Instance = this;
            
            if(WeaponStatsCache.weaponAttributes == null)
            WeaponStatsCache.CreateCache();
        }


        
    }
    void Update()
    {
        if(!IsOwner)
            return;
        
        uiManager.Tick();

        if(!Global_Ui_Manager_Ref.Instance.menuActive)
            playerInput.Tick();
        else
            playerInput.Clear();

        headMovement.Tick(playerInput);
        weaponManager.Tick(playerInput , headMovement.gameObject.transform);

        if(!resetting)
        bodyMovement.Tick(playerInput , headMovement.gameObject.transform);

        mouseLockStateTick();
    }
    private void LateUpdate()
    {
        if(!IsOwner)
            return;
        smoothHead.Tick(rb , headMovement.gameObject.transform);
    }
    public void ResetPlayer()
    {
        if(!IsOwner)
            return;


        weaponManager.weapons.Clear();
        var localOffset = headMovement.transform.localPosition;
        rb.position = new Vector3(0 , -1000 , 0);
        headMovement.transform.localPosition = new Vector3(0,-rb.position.y + 25f,0);
        weaponManager.visuals.ChangeWeapon(-1);

        rb.useGravity = false;
        rb.velocity = new Vector3(0 , 0 , 0);
        bodyMovement.collider.enabled = false;
         resetting = true;
        Debug.Log("spawn delay started");
      
        StartCoroutine( resetWithDelay(4f,localOffset));
        
    }
    IEnumerator resetWithDelay(float delay,Vector3 localHeadPos)
    {
        yield return new WaitForSeconds(delay);

         weaponManager.AddWeapon(WeaponTypes.Sniper);
        weaponManager.AddWeapon(WeaponTypes.RocketLauncher);
        smoothHead.transform.rotation = new Quaternion();
        var spawnPos = ServerGameManagerRef.Instance.getSpawnPosition();
        weaponManager.visuals.ChangeWeapon(0);
        healthHandle.publicHealth = healthHandle.defaultHealth;
        Debug.Log("resetting player to "  + spawnPos);
        rb.position =(spawnPos);
        headMovement.transform.localPosition = localHeadPos;
   
        resetting = false;
        rb.useGravity = true;
        bodyMovement.collider.enabled = true;
        rb.velocity = new Vector3 (0 , 0 , 0);
    }

    private bool winFocus;
    private void mouseLockStateTick()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        if(Input.GetKeyDown(KeyCode.Mouse0) && winFocus && !Global_Ui_Manager_Ref.Instance.menuActive)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }


    }
    private void OnApplicationFocus(bool focus)
    {
        if(focus)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;

        winFocus = focus;
    }

    void OnDestroy()
    {
        //
        if(IsOwner && HighLevelNetcodeRef.Instance.currentLobby != null)
        {
            MainPlayer.Instance = null;
            ServerGameManagerRef.Instance.hostLeave();
        }
        
    }
}

public static class MainPlayer
{
    public static PlayerManager Instance;
}
