using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using Cursor = UnityEngine.Cursor;

public class PlayerManager : NetworkBehaviour
{
    PlayerInput playerInput;
    HeadMovement headMovement;
    BodyMovement bodyMovement;
    SmoothHead smoothHead;
    Rigidbody rb; // main rb of body
    public WeaponManager weaponManager;
    Camera cam;
    public HealthHandle healthHandle;
    PlayerUiManager uiManager;
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
        }


    }
    void Update()
    {
        if(!IsOwner)
            return;
        playerInput.Tick();
        headMovement.Tick(playerInput);
        weaponManager.Tick(playerInput , headMovement.gameObject.transform);
        bodyMovement.Tick(playerInput , headMovement.gameObject.transform);

        uiManager.Tick();

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
        smoothHead.transform.rotation = new Quaternion();
        var spawnPos = ServerGameManagerRef.Instance.getSpawnPosition();
        bodyMovement.gameObject.GetComponent<ClientNetworkTransform>().Teleport(spawnPos,new Quaternion(),Vector3.one);
        bodyMovement.transform.position = spawnPos;

       

        weaponManager.weapons.Clear();
        weaponManager.AddWeapon(WeaponTypes.Sniper);

        weaponManager.visuals.ChangeWeapon(0);
        healthHandle.publicHealth = healthHandle.defaultHealth;
        Debug.Log("resetting player");
    }
    private void mouseLockStateTick()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

    }
    private void OnApplicationFocus(bool focus)
    {
        if(focus)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }

    void OnDestroy()
    {
        //
        if(IsOwner && HighLevelNetcodeRef.Instance.currentLobby != null)
            ServerGameManagerRef.Instance.hostLeave();
    }
}
