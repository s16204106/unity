using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChampionShop : MonoBehaviourPun
{
    public UIController uIController;
    public GamePlayController gamePlayController;
    public GameData gameData;

    ///商店列表
    [HideInInspector]
    public Champion[] availableChampionArray;

    // Start is called before the first frame update
    void Start()
    {
        RefreshShop(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RefreshShop(bool isFree)
    {
        //如果金币不够无法刷新
        if (gamePlayController.currentGold < 2 && isFree == false)
            return;

        availableChampionArray = new Champion[5];

        //刷新商品
        for (int i = 0; i < availableChampionArray.Length; i++)
        {
            //随机获取
            Champion champion = GetRandomChampionInfo();
            //存入商品列表
            availableChampionArray[i] = champion;
            //加载
            uIController.LoadShopItem(champion, i);
            //显示
            uIController.ShowShopItems();
        }

        //扣除刷新商品费用
        if (isFree == false)
            gamePlayController.currentGold -= 2;
        //更新余额
        uIController.UpdateUI();
        photonView.RPC("UpdatePlayer", RpcTarget.All, gamePlayController.playerID, gamePlayController.currentHP, gamePlayController.currentGold, gamePlayController.currentRank);
    }

    /// <summary>
    /// 随机生成战士
    /// </summary>
    public Champion GetRandomChampionInfo()
    {
        //randomise a number
        int rand = Random.Range(0, gameData.championsArray.Length);

        //return from array
        return gameData.championsArray[rand];
    }
    /// <summary>
    /// 当商品UI被点击，判断是否购买成功
    /// </summary>
    /// <param name="index"></param>
    public void OnChampionFrameClicked(int index)
    {
        bool isSucces = gamePlayController.BuyChampionFromShop(availableChampionArray[index]);

        if (isSucces)
            uIController.HideChampionFrame(index);
    }
    /// <summary>
    /// 升级
    /// </summary>
    public void BuyLvl()
    {
        gamePlayController.Buylvl();
    }
}
