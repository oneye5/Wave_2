using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        headMovement = GetComponentInChildren<HeadMovement>();
        bodyMovement = GetComponentInChildren<BodyMovement>();
        smoothHead = GetComponentInChildren<SmoothHead>();
        rb = bodyMovement.gameObject.GetComponent<Rigidbody>();
        weaponManager = GetComponentInChildren<WeaponManager>();
        weaponManager.AddWeapon(WeaponTypes.Sniper);

        cam = GetComponentInChildren<Camera>();
        if(!IsOwner)
        {
            cam.enabled = false;
            GetComponentInChildren< AudioListener>().enabled = false;
            var visuals = GetComponentInChildren<BulletVisuals>();
            var parent = visuals.ModelParrent;
            visuals.ModelParrent = headMovement.gameObject;
        }
    }
    void Update()
    {
      if(!IsOwner)
            return;
        playerInput.Tick();
        headMovement.Tick(playerInput);
        bodyMovement.Tick(playerInput ,headMovement.gameObject.transform);
        weaponManager.Tick(playerInput,headMovement.gameObject.transform);

    }
    private void LateUpdate()
    {
        if(!IsOwner)
            return;
        smoothHead.Tick(rb , headMovement.gameObject.transform);
    }
}
