using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputController : MonoBehaviourPun
{
    public GamePlayController gamePlayController;
    public UIController uIController;
    public Map map;

    [HideInInspector]
    public TriggerInfo triggerInfo = null;

    public LayerMask triggerLayer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        triggerInfo = null;
        map.resetIndicators();

        RaycastHit hit;

        //鼠标点发出射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //检测碰撞
        if (Physics.Raycast(ray, out hit, 100f, triggerLayer, QueryTriggerInteraction.Collide))
        {
            //获取碰撞物体
            triggerInfo = hit.collider.gameObject.GetComponent<TriggerInfo>();

            //如果是一个触发器
            if (triggerInfo != null)
            {
                //获取指示灯类型
                GameObject indicator = map.GetIndicatorFromTriggerInfo(triggerInfo);

                //激活指示灯为绿色
                indicator.GetComponent<MeshRenderer>().material.color = map.indicatorActiveColor;
            }
            //否则重置指示灯颜色
            else
                map.resetIndicators(); 
        }

        //拿起角色
        if (Input.GetMouseButtonDown(0))
        {
            gamePlayController.StartDrag();
        }
            
        //放下
        if (Input.GetMouseButtonUp(0))
        {
            gamePlayController.StopDrag();
        }

        //显示隐藏聊天框
        if(Input.GetKeyDown(KeyCode.Return))
        {
            uIController.showOrHideInputField();
        }
    }
}