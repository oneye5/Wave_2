using System.Collections;
using Unity.Netcode;
using UnityEngine;

class NetworkDestroyer : NetworkBehaviour
{
    [SerializeField] float Timer;
    private void Awake()
    {

        StartCoroutine(kill(Timer));
    }
    IEnumerator kill(float delay)
    {
        yield return new WaitForSeconds(delay);
        if(!IsClient)
            GetComponent<NetworkObject>().Despawn();
        Destroy(this.gameObject);

    }
}
