using UnityEngine;

public struct PlayerInput
{
    public Vector2 mDelta;
    public Vector3 keyState;
    public bool fire;
    public bool reload;
    public uint? weaponSwitch;
    public void Tick()
    {
        mDelta.x = Input.GetAxisRaw("MouseX");
        mDelta.y = Input.GetAxisRaw("MouseY");

        keyState.x = Input.GetAxisRaw("MoveX");
        keyState.y = Input.GetAxisRaw("MoveY");
        keyState.z = Input.GetAxisRaw("MoveZ");




        if(Input.GetAxisRaw("Fire1") != 0)
            fire = true;
        else fire = false;



        if(Input.GetAxisRaw("Reload") != 0)
            reload = true;
        else reload = false;


        if(Input.GetKeyDown("1"))
            weaponSwitch = 0;
        else if(Input.GetKeyDown("2"))
            weaponSwitch = 1;
        else if(Input.GetKeyDown("3"))
            weaponSwitch = 2;
        else
            weaponSwitch = null;
    }
    public void Clear()
    {
        mDelta = Vector2.zero;
        keyState = Vector3.zero;
        fire = false;
        reload = false;
        weaponSwitch = null;
    }
}
