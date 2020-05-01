using UnityEngine;
using System.Collections;
using Photon.Pun;

public class CameraBehavior : MonoBehaviourPun
{

    public float maxSpeed = 1;
    public float breakSpeed = 0.1f;
    float dest_speedX;
    float dest_speedZ;
    public bool InvertX = false;
    public bool InvertZ = false;
    float speedX;
    float speedZ;
    [HideInInspector]
    public Transform []point;
    [HideInInspector]
    public int userid;
    public GameData gameData;
    void Start()
    {
        userid = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log("Player.UserId:" + userid);
        if (userid % 2 == 0)
            InvertX = InvertZ = true;
        point = new Transform[5];
        point[0] = gameData.cameraPoint[userid - 1].GetComponent<Transform>();
        point[1] = gameData.cameraPoint2[userid - 1].GetComponent<Transform>();
        ResetCamera();
    }

    void UpdateInput()
    {
        dest_speedX = Input.GetAxis("Horizontal");
        dest_speedZ = Input.GetAxis("Vertical");

        speedX = Mathf.Lerp(speedX, dest_speedX, breakSpeed);
        speedZ = Mathf.Lerp(speedZ, dest_speedZ, breakSpeed);
        Mathf.Clamp(speedX, -maxSpeed, maxSpeed);
        Mathf.Clamp(speedZ, -maxSpeed, maxSpeed);
    }
    public void SetCamera()
    {
        this.transform.position = point[1].transform.position;
        this.transform.rotation = point[1].transform.rotation;
    }
    public void ResetCamera()
    {
        this.transform.position = point[0].transform.position;
        this.transform.rotation = point[0].transform.rotation;
    }
    void UpdatePosition()
    {
        Vector3 tmpPosition;
        tmpPosition = this.transform.position ;
        //X
        if (InvertX)
        {
            tmpPosition.x -= speedX;
        }
        else
        {
            tmpPosition.x += speedX;
        }
        //Z
        if (InvertZ)
        {
            tmpPosition.z -= speedZ;
        }
        else
        {
            tmpPosition.z += speedZ;
        }

        this.transform.position = tmpPosition;

    }

    // Update is called once per frame
    void Update()
    {
        UpdateInput();
        UpdatePosition();
    }
}
