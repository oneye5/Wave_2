using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

class LocalDestroyer:MonoBehaviour
{
    [SerializeField] float Timer;
    private void Start()
    {
        StartCoroutine(kill(Timer));
    }
    IEnumerator kill(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(this.gameObject);

    }
}
