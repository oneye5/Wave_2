using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothHead : MonoBehaviour
{
    public float angleSmoothing;
    public float positionMulti;
    public float positionSmoothing;
    public float maxMoveMagnitude;
    public float yPosOffset;
    public void Tick(Rigidbody rb,Transform head)
    {
        transform.rotation = Quaternion.Slerp(head.rotation , transform.rotation , angleSmoothing);

        Vector3 offset = new Vector3(rb.velocity.x , rb.velocity.y , rb.velocity.z);
        offset = offset * positionMulti;

        if(offset.magnitude > maxMoveMagnitude)
        {
            offset = offset.normalized * maxMoveMagnitude;

        }
        offset = new Vector3(offset.x , offset.y + yPosOffset , offset.z);
        offset = Vector3.Slerp(transform.localPosition , offset , positionSmoothing);
        transform.localPosition = offset;

    }
}
