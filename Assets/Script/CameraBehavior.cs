using UnityEngine;
using System.Collections;
using Photon.Pun;
using UnityEngine.EventSystems;

public enum PointType
{
    start,
    center,
    camp
}
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
        //userid = 2;
         Debug.Log("Player.UserId:" + userid);
        if (userid % 2 == 0)
            InvertX = InvertZ = true;
        point = new Transform[5];
        // 设置玩家起始摄像头位置
        SetCamera(GetCameraPoint(userid,PointType.start));

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
    //获取摄像头位置
    public Transform GetCameraPoint(int id,PointType pointType)
    {
        if (pointType == PointType.start)
            return gameData.cameraPoint[id - 1].GetComponent<Transform>();

        else if (pointType == PointType.camp)
            return gameData.cameraPoint2[id - 1].GetComponent<Transform>();

        else
            return null;
    }
    //设置摄像头位置
    public void SetCamera(Transform newPoint)
    {
        this.transform.position = newPoint.transform.position;
        this.transform.rotation = newPoint.transform.rotation;
    }
    //点击玩家
    public void OnClickTop()
    {
        string name = EventSystem.current.currentSelectedGameObject.name;
        //截取出数字
        string defaultName = "Player";
        int id = int.Parse(name.Substring(defaultName.Length, 1));
        SetCamera(GetCameraPoint(id, PointType.start));
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
