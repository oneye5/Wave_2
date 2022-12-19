using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadMovement : MonoBehaviour
{
    [SerializeField] float sense;
    float actualPitch = 0.0f;
    public void Tick(PlayerInput inputIn)
    {
        var input = inputIn;
        input.mDelta *= sense;

        Vector2 pitchConstraints = new Vector2Int(-89 , 89);

        Vector3 rot = transform.localRotation.eulerAngles;
        rot = new Vector3(rot.x , rot.y , 0);
        float pitchChange = -input.mDelta.y;
        //constrain pitch
        rot = new Vector3(pitchChange + rot.x , input.mDelta.x + rot.y , 0);
        actualPitch += pitchChange;
        if(actualPitch < pitchConstraints.x)
        {
            float correction = pitchConstraints.x - actualPitch;
            rot = new Vector3(MathF.Round(rot.x + correction) , rot.y , 0);
            actualPitch += correction;
        }
        if(actualPitch > pitchConstraints.y)
        {
            float correction = pitchConstraints.y - actualPitch;
            rot = new Vector3(MathF.Round(rot.x + correction) , rot.y , 0);
            actualPitch += correction;
        }

        transform.localRotation = Quaternion.Euler(rot);
    }
}
