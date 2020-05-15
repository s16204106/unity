using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetColorGroup : MonoBehaviourPun
{
    // Start is called before the first frame update
    void Start()
    {
        string viewid = photonView.Owner.UserId;
        Renderer render = GetComponent<Renderer>();
        render.material.color = new Color(0, 0, float.Parse(viewid));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
