using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.ProBuilder;
using UnityEngine.Windows;

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
    [SerializeField] float A_strafeVelAdd;

    [SerializeField] float ChangeDir_Multi;
    [SerializeField] float ChangeDir_Margin;

    [HideInInspector] public CapsuleCollider collider;
    bool grounded;
    bool justLanded;
    [HideInInspector] public Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
    }

    public void Tick(PlayerInput input , Transform head)
    {
        checkGround();
        move(input , head);
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
    void move(PlayerInput input , Transform head)
    {
        var z = head.forward * input.wasd.z;
        var x = head.right * input.wasd.x;
        var normForce = z + x;
        normForce = new Vector3(normForce.x , 0 , normForce.z);
        normForce = normForce.normalized;
        Vector2 xzVelocity = new Vector2(rb.velocity.x , rb.velocity.z);
        Vector3 force = Vector3.zero;
        float vel = new Vector2(rb.velocity.x , rb.velocity.z).magnitude;


        if(grounded)
        {
            var nonLinearForce = calculateNonLinear(vel , G_maxVel , new Vector2(normForce.x , normForce.z));
            force = (normForce * G_accel)* nonLinearForce;
        }
        else
        { //there are two air movement modes, one is air strafing with mouse and the other is basic wasd movement



            //check that wasd and mouse are going in the same direction
            float changeDir = 0;
            if(input.wasd.x > 0 && input.mouseDelta.x > 0
                ||
                input.wasd.x < 0 && input.mouseDelta.x < 0)
            {
                changeDir = input.mouseDelta.x;
            }
            if(changeDir == 0)
                goto basicWasd;

            //check the angle change is within margin
            
            float velocityAngle = MathF.Atan2(xzVelocity.x , xzVelocity.y); // is in radians
            float velHeadDeltaAngle = Mathf.DeltaAngle(head.rotation.eulerAngles.y, velocityAngle * Mathf.Rad2Deg);
            if(Mathf.Abs(velHeadDeltaAngle) > A_wishDirMargin)
                goto basicWasd;

            //checks complete, now calculate new velocity

            float newVelocityMagnitude = xzVelocity.magnitude + A_strafeVelAdd;
            float newAngle = Mathf.Deg2Rad * head.rotation.eulerAngles.y;
            Vector3 newVel = new Vector3(Mathf.Sin(newAngle) * newVelocityMagnitude, rb.velocity.y , Mathf.Cos(newAngle) * newVelocityMagnitude);
            rb.velocity = newVel;




            //DEFAULT AIR MOVE ===========================


            goto continueWithMovement;
        basicWasd:
          
           var nonLinearForce =  calculateNonLinear(vel,A_maxVel,new Vector2(normForce.x,normForce.z));
            force = (normForce * A_accel) * nonLinearForce;
        }
        continueWithMovement:


        //friction
        if(grounded)
        {
            if(vel > G_maxVel)
            {
                rb.velocity = new Vector3(rb.velocity.x * G_maxVelFriction , rb.velocity.y , rb.velocity.z * G_maxVelFriction);
            }
            else
            {
                rb.velocity = new Vector3(rb.velocity.x * G_friction , rb.velocity.y , rb.velocity.z * G_friction);
            }
        }
        else
        {
            rb.velocity = new Vector3(rb.velocity.x * A_friction , rb.velocity.y , rb.velocity.z * A_friction);
        }

        //jumping

        if(input.wasd.y == 1 && grounded == true)
        {
            rb.velocity = new Vector3(rb.velocity.x , G_jumpForce , rb.velocity.z);
        }
        //apply force
        if(force == Vector3.zero)
            return;

        rb.AddForce(force,ForceMode.Acceleration);
    }

    float calculateNonLinear(float vel,float maxVel,Vector2 wishDir)
    {
        var v1 = new Vector3(rb.velocity.x , 0 , rb.velocity.z);
        var v2 = new Vector3(wishDir.x , 0 , wishDir.y);
        v1 = v1.normalized;
        v2 = v2.normalized;

        float dif = Vector3.Dot(v1 , v2);
        if(dif > ChangeDir_Margin)
            dif = 0;
        
        dif *= ChangeDir_Multi;
        dif = Mathf.Abs(dif);

        float outM = maxVel / (1 + MathF.Abs(vel));
        outM +=dif;
        return outM;
    }
}
