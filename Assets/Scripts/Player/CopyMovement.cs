using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyMovement : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    private void Update()
    {
       transform.position = rb.position;
    }
}
