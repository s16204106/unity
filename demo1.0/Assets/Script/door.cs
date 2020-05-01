using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class door : MonoBehaviour
{
    public Vector3 startPosition;
    public GamePlayController gamePlayController;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
            
    }   
    public void openTheDoor()
    {
        this.transform.position = Vector3.Lerp(this.transform.position, this.transform.position + new Vector3(0,-10,0), 1f);
    }
    public void resetTheDoor()
    {
        this.transform.position = startPosition;
    }
}
