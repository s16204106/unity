using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChampionController : MonoBehaviourPun
{
    private GamePlayController gamePlayController;
    private Map map;
    public TargetState targetState;

    private bool _isDragged = false;
    [HideInInspector]
    public Champion champion;

    [HideInInspector]
    public Vector3 gridTargetPosition;

    [HideInInspector]
    public int gridType = 0;
    [HideInInspector]
    public int gridPositionX = 0;
    [HideInInspector]
    public int gridPositionZ = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            if (_isDragged)
            {
                //Create a ray from the Mouse click position
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;

                //获取鼠标点击位置
                Physics.Raycast(ray, out hit, 100f);
                
                //被选中的角色上提
                Vector3 p = new Vector3(hit.point.x, 2.0f, hit.point.z);

                this.transform.position = Vector3.Lerp(this.transform.position, p, 0.1f);
            }
            else
            {
                if (gamePlayController.currentGameStage == GameStage.Preparation)
                {

                    float distance = Vector3.Distance(gridTargetPosition, this.transform.position);

                    if (distance > 0.25f)
                    {
                        this.transform.position = Vector3.Lerp(this.transform.position, gridTargetPosition, 0.1f);
                    }
                    else
                    {
                        this.transform.position = gridTargetPosition;
                    }
                }
            }
        }
    }

    public bool IsDragged
    {
        get { return _isDragged; }
        set { _isDragged = value; }
    }
    /// <summary>
    /// 初始化Champion
    /// </summary>
    /// <param name="_champion"></param>
    public void Init(Champion _champion)
    {
        champion = _champion;
        map = GameObject.Find("Scripts").GetComponent<Map>();
        gamePlayController = GameObject.Find("Scripts").GetComponent<GamePlayController>();
        targetState = GetComponent<TargetState>(); 

        //set stats
        targetState.prefab = _champion.prefab;
    }

    /// <summary>
    /// 获取网格世界坐标
    /// </summary>
    /// <returns></returns>
    public Vector3 GetWorldPosition()
    {
        //get world position
        Vector3 worldPosition = Vector3.zero;
        if (gridType == Map.GRIDTYPE_OWN_INVENTORY)
        {
            worldPosition = map.ownInventoryGridPositions[gridPositionX];
        }
        else if (gridType == Map.GRIDTYPE_HEXA_MAP)
        {
            worldPosition = map.mapGridPositions[gridPositionX, gridPositionZ];
        }

        return worldPosition;
    }
    /// <summary>
    /// 设置世界坐标
    /// </summary>
    public void SetWorldPosition()
    {
        //获取世界坐标
        Vector3 worldPosition = GetWorldPosition();
        this.transform.position = worldPosition;
        gridTargetPosition = worldPosition;
    }
    /// <summary>
    /// 设置一个新位置
    /// </summary>
    /// <param name="类型"></param>
    /// <param name="X轴"></param>
    /// <param name="Z轴"></param>
    public void SetGridPosition(int _gridType, int _gridPositionX, int _gridPositionZ)
    {
        gridType = _gridType;
        gridPositionX = _gridPositionX;
        gridPositionZ = _gridPositionZ;
        gridTargetPosition = GetWorldPosition();
    }
    [PunRPC]
    public void setDisable()
    {
        //disable agent
        this.GetComponent<AI>().enabled = false;
        this.transform.Find("Health/healthbar").gameObject.GetComponent<HealthBar>().enabled = false;
        this.transform.Find("Health").gameObject.SetActive(false);
    }
}
