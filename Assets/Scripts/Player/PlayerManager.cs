using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    PlayerInput playerInput;
    HeadMovement headMovement;
    BodyMovement bodyMovement;
    SmoothHead smoothHead;
    Rigidbody rb; // main rb of body
    WeaponManager weaponManager;
    Camera cam;
    HealthHandle healthHandle;
    PlayerUiManager uiManager;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        headMovement = GetComponentInChildren<HeadMovement>();
        bodyMovement = GetComponentInChildren<BodyMovement>();
        smoothHead = GetComponentInChildren<SmoothHead>();
        rb = bodyMovement.gameObject.GetComponent<Rigidbody>();
        weaponManager = GetComponentInChildren<WeaponManager>();
        weaponManager.AddWeapon(WeaponTypes.Sniper);
        healthHandle = GetComponentInChildren<HealthHandle>();
        cam = GetComponentInChildren<Camera>();
        uiManager = GetComponentInChildren<PlayerUiManager>();
        if(!IsOwner)
        {
            cam.enabled = false;
            GetComponentInChildren< AudioListener>().enabled = false;
            var visuals = GetComponentInChildren<BulletVisuals>();
            var parent = visuals.ModelParrent;
            visuals.ModelParrent = headMovement.gameObject;
            Destroy(uiManager.gameObject);
        }
        else
        {
            ServerGameManagerRef.Instance.PlayerJoin_ServerRpc(
                AuthenticationService.Instance.PlayerId,
                cam.gameObject.GetComponent<NetworkObject>().NetworkObjectId);

            ServerGameManagerRef.Instance.GameStart();
        }
    }
    void Update()
    {
      if(!IsOwner)
            return;
        playerInput.Tick();
        headMovement.Tick(playerInput);
        weaponManager.Tick(playerInput , headMovement.gameObject.transform);
        bodyMovement.Tick(playerInput ,headMovement.gameObject.transform);
       
        uiManager.Tick();
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
        bodyMovement.gameObject.transform.position = ServerGameManagerRef.Instance.getSpawnPosition();
        weaponManager.weapons.Clear();
        weaponManager.AddWeapon(WeaponTypes.Sniper);
        weaponManager.visuals.ChangeWeapon(0);
    }
    private void OnDestroy()
    {
        if(IsOwner)
        ServerGameManagerRef.Instance.hostLeave();
    }
}
