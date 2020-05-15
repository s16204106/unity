using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum GameStage { Preparation, Preparation2, Combat,End, Loss };

public class GamePlayController : MonoBehaviourPun, IPunObservable
{
    [HideInInspector]
    public GameObject temp;
    [HideInInspector]
    public GameObject []myChampionArray = new GameObject[40];

    public GameData gameData;
    public GameStage currentGameStage;
    public ChampionShop championShop;
    public UIController uIController;
    public InputController inputController;
    public Map map;
    public CameraBehavior cameraBehavior;
    public Player []playerList; 
    
    [HideInInspector]
    public int playerID;

    [HideInInspector]
    public bool isDeath = false;

    [HideInInspector]
    public door[] doors;
    [HideInInspector]
    public int currentRank = 1;
    [HideInInspector]
    public int currentFighter = 0;
    [HideInInspector]
    public int currentGold = 5;
    [HideInInspector]
    public int currentHP = 100;
    [HideInInspector]
    public int timerDisplay = 0;
    [HideInInspector]
    public GameObject camp;

    [HideInInspector]
    public GameObject[] ownChampionInventoryArray;
    [HideInInspector]
    public GameObject[,] gridChampionsArray;

    ///准备时间
    public int preparationStageDuration;
    public int preparationStageDuration2;

    ///最长战斗时间
    public int combatStageDuration;

    //延迟时间
    public int Delay;
    private float timer = 0;

    //控制开启协程
    private bool coroutineFlag;

    //结束回合的玩家数
    private int allEndNum = 0;

    //结束该局的玩家数
    private int deathPlayerNum = 0;
    [HideInInspector]
    public int currentRoomPlayers = 0;

    // Start is called before the first frame update
    void Start()
    {
        currentRoomPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        initPlayer();
        //uIController.updateTop(playerList);
        uIController.showTop();

        playerID = PhotonNetwork.LocalPlayer.ActorNumber;

        doors = gameData.doors;
        camp = gameData.camp[playerID - 1];
        ownChampionInventoryArray = new GameObject[Map.inventorySize];
        gridChampionsArray = new GameObject[Map.hexMapSizeX, Map.hexMapSizeZ / 2];
        uIController.UpdateUI();
    }

    private void initPlayer()
    {
        playerList = new Player[currentRoomPlayers];
        for (int i = 0; i < playerList.Length; i++)
        {
            playerList[i] = new Player();
            playerList[i].ID = i + 1;
            playerList[i].playerName = PhotonNetwork.CurrentRoom.GetPlayer(i+1).NickName;
            playerList[i].Health = 100;
            playerList[i].Gold = 5;
            playerList[i].Rank = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        uIController.updateTop(playerList);
        //Debug.Log("当前状态：" + currentGameStage + "---进入End状态的玩家数:" + allEndNum + "########我方战士数：" + currentFighter + "--当前对手数：" + (currentRoomPlayers - 1 - deathPlayerNum - allEndNum));
        if (currentGameStage == GameStage.Preparation)
        {
            currentFighter = 0;
            if(allEndNum!=0)
                photonView.RPC("ResetAllEndNum", RpcTarget.All);

            timer += Time.deltaTime;
            timerDisplay = (int)(preparationStageDuration - timer);

            uIController.UpdateTimerText();
            //准备超时，重置时间并转换阶段
            if (timer > preparationStageDuration)
            {
                timer = 0;
                OnGameStageComplete();
            }
        }
        else if(currentGameStage == GameStage.Preparation2)
        {
            timer += Time.deltaTime;
            timerDisplay = (int)(preparationStageDuration2 - timer);
            uIController.UpdateTimerText();
            if (timer > preparationStageDuration2)
            {
                timer = 0;
                OnGameStageComplete();
            }
        }
        else if (currentGameStage == GameStage.Combat)
        {
            timer += Time.deltaTime;
            timerDisplay = (int)timer;
            //战斗超时或我方战士死光，重置时间并转换阶段
            if (timer > combatStageDuration || currentFighter == 0)
            {
                currentHP -= 6;
                uIController.UpdateUI();
                timer = 0;
                OnGameStageComplete();
            }
            //战斗胜利
            else if((currentRoomPlayers - 1 - deathPlayerNum - allEndNum) == 0)
            {
                if (coroutineFlag == false)
                {
                    currentGold += 2;
                    uIController.showWinnerIcon();
                    uIController.UpdateUI();
                    StartCoroutine(DelayChangeState(3f, currentGameStage));
                    coroutineFlag = true;
                }
            }
        }
        else if(currentGameStage == GameStage.End)
        {
            OnGameStageComplete();
        }
    }
    private TargetState targetState;
    /// <summary>
    /// 阶段转换
    /// </summary>
    private void OnGameStageComplete()
    {
        if (currentGameStage == GameStage.Preparation)
        {
            //设置新游戏状态
            currentGameStage = GameStage.Preparation2;
            uIController.ChangeText("即将战斗");

            //调整摄像头为起始位置
            //cameraBehavior.SetCamera(cameraBehavior.GetCameraPoint(playerID,PointType.camp));

            // 放下还在拖动物体
            if (draggedChampion != null)
            {
                //stop dragging    
                draggedChampion.GetComponent<ChampionController>().IsDragged = false;
                draggedChampion = null;
            }
            //归位
            for (int i = 0; i < ownChampionInventoryArray.Length; i++)
            {
                
                if (ownChampionInventoryArray[i] != null)
                {
                    
                    ChampionController championController = ownChampionInventoryArray[i].GetComponent<ChampionController>();
                    championController.IsDragged = false;
                    championController.transform.position = championController.gridTargetPosition;

                }
            }
            
            //批量生成军队
            for (int x = 0; x < Map.hexMapSizeX; x++)
            {
                for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
                {
                    if (gridChampionsArray[x, z] != null)
                    {
                        //生成champion里对应的number值的战士
                        targetState = gridChampionsArray[x, z].GetComponent<TargetState>();
                        for (int n = 0; n < targetState.number; n++)
                        {
                            GameObject temp = PhotonNetwork.Instantiate(targetState.prefab.name, targetState.GetComponent<Transform>().position, targetState.GetComponent<Transform>().rotation);
                            temp.GetComponent<ChampionController>().enabled = false;
                            
                            //设置标签
                            temp.GetComponent<TargetState>().photonView.RPC("setTag", RpcTarget.All);
                            myChampionArray[currentFighter] = temp;
                            currentFighter++;
                        }
                    }
                }
            }

        }
        else if(currentGameStage == GameStage.Preparation2)
        {
            //设置新游戏状态
            currentGameStage = GameStage.Combat;
            //隐藏倒计时
            uIController.SetTimerTextActive(false);
            uIController.ChangeText("准备时间");
            //激活战士
            photonView.RPC("setAble", RpcTarget.All);
            //开门
            for (int i = 0; i < doors.Length; i++)
            {
                doors[i].openTheDoor();
            }

        }
        else if (currentGameStage == GameStage.Combat)
        {
            //设置新游戏状态
            currentGameStage = GameStage.End;
            photonView.RPC("SetAllEndNum", RpcTarget.All);

        }
        else if(currentGameStage == GameStage.End)
        {
            //当所有玩家进入End状态时
            if(allEndNum == currentRoomPlayers - deathPlayerNum)
            {
                //避免开启多个携程
                if (coroutineFlag == false)
                {
                    StartCoroutine(DelayChangeState(3f, currentGameStage));
                    coroutineFlag = true;
                }
            }
        }
    }
    IEnumerator DelayChangeState(float t,GameStage gs)
    {
        if(gs == GameStage.Combat)
        {
            yield return new WaitForSeconds(t);
            OnGameStageComplete();
            coroutineFlag = false;
            uIController.hideWinnerIcon();
        }
        else if (gs == GameStage.End)
        {
            currentGold += 6;
            photonView.RPC("UpdatePlayer", RpcTarget.All, playerID, currentHP, currentGold , currentRank);
            //延时t秒
            yield return new WaitForSeconds(t);
            
            //设置新游戏状态
            this.currentGameStage = GameStage.Preparation;
            
            //显示倒计时
            uIController.SetTimerTextActive(true);

            //更新
            coroutineFlag = false;
            timer = 0;
            cameraBehavior.SetCamera(cameraBehavior.GetCameraPoint(playerID, PointType.start));
            for (int i = 0; i < doors.Length; i++)
            {
                doors[i].resetTheDoor();
            }

            uIController.UpdateUI();

            //刷新商品
            for (int i = 0; i < championShop.availableChampionArray.Length; i++)
            {
                //随机获取
                Champion champion = championShop.GetRandomChampionInfo();
                //存入商品列表
                championShop.availableChampionArray[i] = champion;
                //加载
                uIController.LoadShopItem(champion, i);
                //显示
                uIController.ShowShopItems();
            }
        }
    }
    [PunRPC]
    private void SetAllEndNum()
    {
        allEndNum++;
    }
    [PunRPC]
    private void ResetAllEndNum()
    {
        allEndNum = 0;
    }
    [PunRPC]
    private void UpdatePlayer(int id, int health ,int gold ,int rank)
    {
        for(int i=0;i<playerList.Length;i++)
        {
            if(playerList[i].ID == id)
            {
                playerList[i].Health = health;
                playerList[i].Gold = gold;
                playerList[i].Rank = rank;
            }
        }
    }

    private TriggerInfo dragStartTrigger = null;
    private GameObject draggedChampion = null;

    /// <summary>
    /// 开始拖动
    /// </summary>
    public void StartDrag()
    {
        if (currentGameStage != GameStage.Preparation)
            return;
        
        //获取当前鼠标指向
        TriggerInfo triggerinfo = inputController.triggerInfo;
        //当前鼠标指向一个触发器
        if (triggerinfo != null)
        {
            dragStartTrigger = triggerinfo;
            //获取指示灯上的物体
            GameObject championGO = GetChampionFromTriggerInfo(triggerinfo);

            if (championGO != null)
            {
                //show indicators
                map.ShowIndicators();

                draggedChampion = championGO;

                championGO.GetComponent<ChampionController>().IsDragged = true;
            }

        }
    }
    /// <summary>
    /// 结束拖动
    /// </summary>
    public void StopDrag()
    {
        map.HideIndicators();
        if (draggedChampion != null)
        {
            draggedChampion.GetComponent<ChampionController>().IsDragged = false;

            //get trigger info
            TriggerInfo triggerinfo = inputController.triggerInfo;
            //if mouse cursor on trigger
            if (triggerinfo != null)
            {
                //get current champion over mouse cursor
                GameObject currentTriggerChampion = GetChampionFromTriggerInfo(triggerinfo);

                //互换角色位置
                if (currentTriggerChampion != null)
                {
                    //起始位置存入当前角色
                    StoreChampionInArray(dragStartTrigger.gridType, dragStartTrigger.gridX, dragStartTrigger.gridZ, currentTriggerChampion);

                    //当前位置存入拖拽角色
                    StoreChampionInArray(triggerinfo.gridType, triggerinfo.gridX, triggerinfo.gridZ, draggedChampion);
                }
                else
                {
                    //添加入战斗场地
                    if (triggerinfo.gridType == Map.GRIDTYPE_HEXA_MAP)
                    {
                        //删除起始位
                        RemoveChampionFromArray(dragStartTrigger.gridType, dragStartTrigger.gridX, dragStartTrigger.gridZ);

                        //添加终止位
                        StoreChampionInArray(triggerinfo.gridType, triggerinfo.gridX, triggerinfo.gridZ, draggedChampion);

                    }
                    //添加入待命场地
                    else if (triggerinfo.gridType == Map.GRIDTYPE_OWN_INVENTORY)
                    {
                        //删除起始位
                        RemoveChampionFromArray(dragStartTrigger.gridType, dragStartTrigger.gridX, dragStartTrigger.gridZ);

                        //添加终止位
                        StoreChampionInArray(triggerinfo.gridType, triggerinfo.gridX, triggerinfo.gridZ, draggedChampion);
                    }
                }
            }
        }

        draggedChampion = null;
    }

    /// <summary>
    /// 从triggerinfo获取champion gameobject
    /// </summary>
    /// <param name="triggerinfo"></param>
    /// <returns></returns>
    public GameObject GetChampionFromTriggerInfo(TriggerInfo triggerinfo)
    {
        GameObject championGO = null;

        if (triggerinfo.gridType == Map.GRIDTYPE_OWN_INVENTORY)
        {
            championGO = ownChampionInventoryArray[triggerinfo.gridX];
        }
        else if (triggerinfo.gridType == Map.GRIDTYPE_HEXA_MAP)
        {
            championGO = gridChampionsArray[triggerinfo.gridX, triggerinfo.gridZ];
        }

        return championGO;
    }
    /// <summary>
    /// 角色存入数组
    /// </summary>
    /// <param name="triggerinfo"></param>
    /// <param name="champion"></param>
    private void StoreChampionInArray(int gridType, int gridX, int gridZ, GameObject champion)
    {
        //修改战士位置属性
        ChampionController championController = champion.GetComponent<ChampionController>();
        championController.SetGridPosition(gridType, gridX, gridZ);
        //放入数组
        if (gridType == Map.GRIDTYPE_OWN_INVENTORY)
        {
            ownChampionInventoryArray[gridX] = champion;
        }
        else if (gridType == Map.GRIDTYPE_HEXA_MAP)
        {
            gridChampionsArray[gridX, gridZ] = champion;
        }
    }
    /// <summary>
    /// 从数组移除角色
    /// </summary>
    private void RemoveChampionFromArray(int type, int gridX, int gridZ)
    {
        if (type == Map.GRIDTYPE_OWN_INVENTORY)
        {
            ownChampionInventoryArray[gridX] = null;
        }
        else if (type == Map.GRIDTYPE_HEXA_MAP)
        {
            gridChampionsArray[gridX, gridZ] = null;
        }
    }

    /// <summary>
    /// 商店购买后地图上生成战士
    /// </summary>
    public bool BuyChampionFromShop(Champion champion)
    {
        //获取位置
        int emptyIndex = -1;
        for (int i = 0; i < ownChampionInventoryArray.Length; i++)
        {
            if (ownChampionInventoryArray[i] == null)
            {
                emptyIndex = i;
                break;
            }
        }

        //空间不足
        if (emptyIndex == -1)
            return false;

        //金币不足
        if (currentGold < champion.cost)
            return false;

        //实例化
        GameObject championPrefab = PhotonNetwork.Instantiate(champion.prefab.name, map.ownInventoryGridPositions[emptyIndex], Quaternion.Euler(0, cameraBehavior.GetCameraPoint(playerID,PointType.start).eulerAngles.y, 0));

        ChampionController championController = championPrefab.GetComponent<ChampionController>();

        championController.Init(champion);
        championController.photonView.RPC("setDisable", RpcTarget.All);


        //设置物体的网格属性
        championController.SetGridPosition(Map.GRIDTYPE_OWN_INVENTORY, emptyIndex, -1);

        //设置世界坐标
        championController.SetWorldPosition();

        //存入数组
        StoreChampionInArray(Map.GRIDTYPE_OWN_INVENTORY, map.ownTriggerArray[emptyIndex].gridX, -1, championPrefab);

        //扣除商品费用
        currentGold -= champion.cost;

        //更新
        photonView.RPC("UpdatePlayer", RpcTarget.All, playerID, currentHP, currentGold, currentRank);
        uIController.UpdateUI();

        return true;
    }
    /// <summary>
    /// 升级
    /// </summary>
    public void Buylvl()
    {
        //金币不足
        if (currentGold < 4)
            return;

        //扣除费用
        currentGold -= 4;

        //更新
        currentRank += 1;
        uIController.UpdateUI();
        photonView.RPC("UpdatePlayer", RpcTarget.All, playerID, currentHP, currentGold, currentRank);
    }

    /// <summary>
    /// 激活战士
    /// </summary>
    [PunRPC]
    public void setAble()
    {
        for(int i=0;i<currentFighter;i++)
        {
            myChampionArray[i].GetComponent<NavMeshAgent>().enabled = true;
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data

            stream.SendNext(this.allEndNum);
            stream.SendNext(this.deathPlayerNum);
        }
        else
        {
            // Network player, receive data

            this.allEndNum = (int)stream.ReceiveNext();
            this.deathPlayerNum = (int)stream.ReceiveNext();
        }
    }

}
