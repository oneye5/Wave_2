using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class BodyMovement : MonoBehaviour
{
    [Header("GROUND VARS")]
    [SerializeField] float G_accel;
    [SerializeField] float G_friction;
    [SerializeField] float G_maxVel;
    [SerializeField] float G_maxVelFriction;
    [SerializeField] float G_jumpForce;
    [SerializeField] Vector3 G_checkBox;
    [SerializeField] float G_checkOffset;
    [SerializeField] float G_brakingFriction;

    [Header("AIR VARS")]
    [SerializeField] float A_accel;
    [SerializeField] float A_friction;
    [SerializeField] float A_maxVel;
    [SerializeField] float A_brakingForce;
    [SerializeField] float A_wishDirMargin;
    [SerializeField] float A_strafeAccelMulti;

    [HideInInspector] public CapsuleCollider collider;
    bool grounded;
    bool justLanded;
    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
    }

    public void Tick(PlayerInput input,Transform head)
    {
        checkGround();
        move(input,head);
    }

    void checkGround()
    {
        if(grounded == false)
            justLanded = true;
        else
            justLanded = false;
        var hit = Physics.BoxCastAll(transform.position + new Vector3(0 , G_checkOffset , 0) , G_checkBox , Vector3.down);
        foreach(var x in hit)
        {
            if(x.collider.tag != "notGround" && x.collider.tag != "Player" && x.distance < 0.1)
            {
              //  Debug.Log("grounded " + x.collider.gameObject.name);

                grounded = true;
                break;
            }
            else
            {
                grounded = false;
            }
        }

        if(hit.Length == 0)
            grounded = false;

        if(grounded != true)
            justLanded = false;
    }
    void move(PlayerInput input ,Transform head)
    {
        var z = head.forward * input.keyState.z;
        var x = head.right * input.keyState.x;
        var normForce = z + x;
        normForce = new Vector3(normForce.x , 0 , normForce.z);
        normForce = normForce.normalized;

        Vector3 force;
        float vel = new Vector2(rb.velocity.x , rb.velocity.z).magnitude;
        if(grounded)
        {
            force = normForce * G_accel;
        }
        else
        {
            force = normForce * A_accel;
        }

        //friction
        if(grounded)
        {
            if(vel > G_maxVel)
            {
                rb.velocity = new Vector3(rb.velocity.x * G_maxVelFriction , rb.velocity.y , rb.velocity.z * G_maxVelFriction);
            }
            else
            {
                rb.velocity = new Vector3(rb.velocity.x * G_friction, rb.velocity.y , rb.velocity.z * G_friction);
            }
        }
        else
        {
            rb.velocity = new Vector3(rb.velocity.x * A_friction, rb.velocity.y , rb.velocity.z * A_friction);
        }

        //jumping

        if(input.keyState.y == 1 && grounded == true)
        {
            rb.velocity = new Vector3(rb.velocity.x , G_jumpForce , rb.velocity.z);
        }
        //apply force
        rb.AddForce(force);
    }
}
