using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviourPun
{
    //定义网格类型
    public static int GRIDTYPE_OWN_INVENTORY = 0;
    public static int GRIDTYPE_OPONENT_INVENTORY = 1;
    public static int GRIDTYPE_HEXA_MAP = 2;

    //定义网格大小
    public static int hexMapSizeX = 4;
    public static int hexMapSizeZ = 10;
    public static int inventorySize = 5;


    //位置列表
    [HideInInspector]
    public Vector3[] ownInventoryGridPositions;
    [HideInInspector]
    public Vector3[,] mapGridPositions;

    //定义指示灯和触发器数组
    [HideInInspector]
    public GameObject[] ownIndicatorArray;
    [HideInInspector]
    public GameObject[,] mapIndicatorArray;
    [HideInInspector]
    public TriggerInfo[] ownTriggerArray;
    [HideInInspector]
    public TriggerInfo[,] mapGridTriggerArray;

    //触发器生成点
    [HideInInspector]
    public Transform ownInventoryStartPosition;
    [HideInInspector]
    public Transform mapStartPosition;

    //指示灯预制体
    public GameObject squareIndicator;
    public GameObject hexaIndicator;

    //指示灯默认颜色和选中颜色
    public Color indicatorDefaultColor;
    public Color indicatorActiveColor;

    private GameObject indicatorContainer;

    // Start is called before the first frame update
    void Start()
    {
        CreateGridPosition();
        CreateIndicators();
        HideIndicators();
        ShowIndicators();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Creates the positions for all the map grids
    /// </summary>
    private void CreateGridPosition()
    {
        //实例化本地网格
        ownInventoryGridPositions = new Vector3[inventorySize];
        mapGridPositions = new Vector3[hexMapSizeX, hexMapSizeZ];

        //根据网络id路由一个位置
        int userid = PhotonNetwork.LocalPlayer.ActorNumber;
        switch (userid)
        {
            case 1: ownInventoryStartPosition = GameObject.Find("square1").GetComponent<Transform>();
                    mapStartPosition = GameObject.Find("hexa1").GetComponent<Transform>(); break;
            case 2: ownInventoryStartPosition = GameObject.Find("square2").GetComponent<Transform>();
                    mapStartPosition = GameObject.Find("hexa2").GetComponent<Transform>(); break;
        }

        //创建待命指示灯位置
        for (int i = 0; i < inventorySize; i++)
        {
            //calculate position x offset for this slot
            float offsetX = i * -2.5f;

            //calculate and store the position
            Vector3 position = GetMapHitPoint(ownInventoryStartPosition.position + new Vector3(offsetX, 0, 0));

            //add position variable to array
            ownInventoryGridPositions[i] = position;
        }

        //创建战斗指示灯位置
        for (int x = 0; x < hexMapSizeX; x++)
        {
            for (int z = 0; z < hexMapSizeZ; z++)
            {
                
                int rowOffset = z % 2;
                //最后一列偶数为空
                if(x!=3||rowOffset%2!=0)
                {
                    //计算指示灯点位置
                    float offsetX = x * -3f + rowOffset * 1.5f;
                    float offsetZ = z * -2.5f;

                    //calculate and store the position
                    Vector3 position = GetMapHitPoint(mapStartPosition.position + new Vector3(offsetX, 0, offsetZ));

                    //存入
                    mapGridPositions[x, z] = position;
                }
            }

        }

    }
    /// <summary>
    /// Creates all the map indicators
    /// </summary>
    private void CreateIndicators()
    {
        //指示灯容器
        indicatorContainer = new GameObject();
        indicatorContainer.name = "IndicatorContainer";

        //触发器容器
        GameObject triggerContainer = new GameObject();
        triggerContainer.name = "TriggerContainer";

        //初始化指示灯数组
        ownIndicatorArray = new GameObject[inventorySize];
        mapIndicatorArray = new GameObject[hexMapSizeX, hexMapSizeZ / 2];

        //初始化触发器数组
        ownTriggerArray = new TriggerInfo[inventorySize];
        mapGridTriggerArray = new TriggerInfo[hexMapSizeX, hexMapSizeZ / 2];


        //iterate own grid position
        for (int i = 0; i < inventorySize; i++)
        {
            //实例化
            GameObject indicatorGO = Instantiate(squareIndicator);

            //设置位置
            indicatorGO.transform.position = ownInventoryGridPositions[i];

            //放入容器
            indicatorGO.transform.parent = indicatorContainer.transform;

            //放入指示灯数组
            ownIndicatorArray[i] = indicatorGO;

            //创建矩形触发器
            GameObject trigger = CreateBoxTrigger(GRIDTYPE_OWN_INVENTORY, i);

            //设置位置
            trigger.transform.parent = triggerContainer.transform;

            //放入容器
            trigger.transform.position = ownInventoryGridPositions[i];

            //存入触发器数组
            ownTriggerArray[i] = trigger.GetComponent<TriggerInfo>();
        }

        //iterate map grid position
        for (int x = 0; x < hexMapSizeX; x++)
        {
            for (int z = 0; z < hexMapSizeZ / 2; z++)
            {
                if (mapGridPositions[x, z] != new Vector3(0,0,0))
                {
                    //create indicator gameobject
                    GameObject indicatorGO = Instantiate(hexaIndicator);

                    //set indicator gameobject position
                    indicatorGO.transform.position = mapGridPositions[x, z];

                    //set indicator parent
                    indicatorGO.transform.parent = indicatorContainer.transform;

                    //store indicator gameobject in array
                    mapIndicatorArray[x, z] = indicatorGO;

                    //create trigger gameobject
                    GameObject trigger = CreateSphereTrigger(GRIDTYPE_HEXA_MAP, x, z);

                    //set trigger parent
                    trigger.transform.parent = triggerContainer.transform;

                    //set trigger gameobject position
                    trigger.transform.position = mapGridPositions[x, z];

                    //store triggerinfo
                    mapGridTriggerArray[x, z] = trigger.GetComponent<TriggerInfo>();
                }

            }
        }

    }

    /// <summary>
    /// Get a point with accurate y axis
    /// </summary>
    /// <returns></returns>
    public Vector3 GetMapHitPoint(Vector3 p)
    {
        Vector3 newPos = p;

        RaycastHit hit;

        if (Physics.Raycast(newPos + new Vector3(0, 10, 0), Vector3.down, out hit, 15))
        {
            newPos = hit.point;
        }

        return newPos;
    }

    /// <summary>
    /// 创建盒型触发器
    /// </summary>
    /// <returns></returns>
    private GameObject CreateBoxTrigger(int type, int x)
    {
        //create primitive gameobject
        GameObject trigger = new GameObject();

        //add collider component
        BoxCollider collider = trigger.AddComponent<BoxCollider>();

        //set collider size
        collider.size = new Vector3(2, 0.5f, 2);

        //set collider to trigger 
        collider.isTrigger = true;

        //add and store trigger info
        TriggerInfo trigerInfo = trigger.AddComponent<TriggerInfo>();
        trigerInfo.gridType = type;
        trigerInfo.gridX = x;

        trigger.layer = LayerMask.NameToLayer("Triggers");

        return trigger;
    }

    // <summary>
    /// 创建球型触发器
    /// </summary>
    /// <returns></returns>
    private GameObject CreateSphereTrigger(int type, int x, int z)
    {
        //create primitive gameobject
        GameObject trigger = new GameObject();

        //add collider component
        SphereCollider collider = trigger.AddComponent<SphereCollider>();

        //set collider size
        collider.radius = 1.4f;

        //set collider to trigger 
        collider.isTrigger = true;

        //add and store trigger info
        TriggerInfo trigerInfo = trigger.AddComponent<TriggerInfo>();
        trigerInfo.gridType = type;
        trigerInfo.gridX = x;
        trigerInfo.gridZ = z;

        trigger.layer = LayerMask.NameToLayer("Triggers");

        return trigger;
    }
    /// <summary>
    /// 返回网格类型
    /// </summary>
    /// <param name="triggerinfo"></param>
    /// <returns></returns>
    public GameObject GetIndicatorFromTriggerInfo(TriggerInfo triggerinfo)
    {
        GameObject triggerGo = null;

        if (triggerinfo.gridType == GRIDTYPE_OWN_INVENTORY)
        {
            triggerGo = ownIndicatorArray[triggerinfo.gridX];
        }
        else if (triggerinfo.gridType == GRIDTYPE_HEXA_MAP)
        {
            triggerGo = mapIndicatorArray[triggerinfo.gridX, triggerinfo.gridZ];
        }


        return triggerGo;
    }
    /// <summary>
    /// 重置指示灯颜色
    /// </summary>
    public void resetIndicators()
    {
        for (int x = 0; x < hexMapSizeX; x++)
        {
            for (int z = 0; z < hexMapSizeZ / 2; z++)
            {
                if(mapIndicatorArray[x, z]!=null)
                    mapIndicatorArray[x, z].GetComponent<MeshRenderer>().material.color = indicatorDefaultColor;
            }
        }

        for (int x = 0; x < inventorySize; x++)
        {
            ownIndicatorArray[x].GetComponent<MeshRenderer>().material.color = indicatorDefaultColor;
        }

    }
    /// <summary>
    /// 显示指示灯
    /// </summary>
    public void ShowIndicators()
    {
        indicatorContainer.SetActive(true);
    }
    /// <summary>
    /// 隐藏指示灯
    /// </summary>
    public void HideIndicators()
    {
        indicatorContainer.SetActive(false);
    }
}
