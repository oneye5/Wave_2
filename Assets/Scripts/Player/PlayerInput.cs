using UnityEngine;

public struct PlayerInput
{
    public Vector2 mDelta;
    public Vector3 keyState;
    public bool fire;
    public bool reload;
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
    }
}
