using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitscanTrailMover : MonoBehaviour
{
    [SerializeField] float maxRange;
    [SerializeField] float timeMulti;
    private void Awake()
    {
        Vector3 toPos;
        RaycastHit hit;
        if(Physics.Raycast(transform.position , transform.rotation.eulerAngles , out hit , maxRange))        
        {
            toPos = hit.point;
        }
        else
        {
            toPos = transform.position;
            toPos = toPos + (transform.forward * maxRange);
        }

        float dist = Vector3.Distance(transform.position , toPos);
        float time = dist * timeMulti;
        transform.DOMove(toPos , time);
    }
}
