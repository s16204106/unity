using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviourPun, IPunObservable
{
    Image healthbar;
    public TargetState ts;
    // Start is called before the first frame update
    void Start()
    {
        healthbar = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
        healthbar.fillAmount = ts.GetHealth(); ;

        if (ts.GetHealth() > 0.3 && ts.GetHealth() <= 0.6)
        {
            healthbar.color = Color.yellow;
        }
        else if (ts.GetHealth() <= 0.3)
        {
            healthbar.color = Color.red;
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(this.healthbar.fillAmount);
        }
        else
        {
            //Network player, receive data
            this.healthbar.fillAmount = (float)stream.ReceiveNext();
        }
    }
}
