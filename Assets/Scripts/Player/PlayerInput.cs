using UnityEngine;

public struct PlayerInput
{
    public Vector2 mouseDelta;
    public Vector3 wasd;
    public bool fire;
    public bool reload;
    public uint? weaponSwitch;
    public void Tick()
    {
        mouseDelta.x = Input.GetAxisRaw("MouseX");
        mouseDelta.y = Input.GetAxisRaw("MouseY");

        wasd.x = Input.GetAxisRaw("MoveX");
        wasd.y = Input.GetAxisRaw("MoveY");
        wasd.z = Input.GetAxisRaw("MoveZ");




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
        mouseDelta = Vector2.zero;
        wasd = Vector3.zero;
        fire = false;
        reload = false;
        weaponSwitch = null;
    }
}
