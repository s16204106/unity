using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTrigger : MonoBehaviourPun
{
    public int damage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (!(GetComponentInParent<AI>().CurrentState == Targetstate.attack))
            return;
        else if (GetComponentInParent<PhotonView>().IsMine)
        { 
            if (!other.GetComponent<PhotonView>().IsMine)
            {
                other.GetComponent<TargetState>().photonView.RPC("TakeDamage", RpcTarget.All, damage);
            }
        }
    }
}
