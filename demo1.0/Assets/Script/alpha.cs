using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class alpha : MonoBehaviour
{
    public float a = 1;
    // Start is called before the first frame update
    void Start()
    {
        GUI.color = new Color(1f,1f,1f, a);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
