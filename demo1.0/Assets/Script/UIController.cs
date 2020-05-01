using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIController : MonoBehaviourPun
{
    public GamePlayController gamePlayController;
    public ChampionShop championShop;
    public Text timerText;
    public Text goldText;
    public Text hpText;
    public Text text;

    //排行榜
    public GameObject[] Top;
    public Image[] imageHealth;
    public Text[] textHealth;
    public Text[] playerName;
    public Text[] Gold;

    public GameObject timer;
    public GameObject[] championsFrameArray;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //更新UI界面 
    public void UpdateUI()
    {
        goldText.text = gamePlayController.currentGold.ToString();
        hpText.text = "HP " + gamePlayController.currentHP.ToString();
    }

    //更新时间
    public void UpdateTimerText()
    {
        timerText.text = gamePlayController.timerDisplay.ToString();
    }
    //Change
    public void ChangeText(string str)
    {
        text.text = str;
    }
    // 隐藏倒计时
    public void SetTimerTextActive(bool b)
    {
        timer.SetActive(b);
    }
    /// <summary>
    /// 商品UI被点击
    /// </summary>
    public void OnChampionClicked()
    {
        //获取当前选择的UI Name
        string name = EventSystem.current.currentSelectedGameObject.transform.parent.name;

        //截取出数字
        string defaultName = "champion container ";
        int championFrameIndex = int.Parse(name.Substring(defaultName.Length, 1));

        //传数字给商店
        championShop.OnChampionFrameClicked(championFrameIndex);
    }

    /// <summary>
    /// 加载商店UI
    /// </summary>
    /// <param name="champion"></param>
    /// <param name="index"></param>
    public void LoadShopItem(Champion champion, int index)
    {
        //get unit frames
        Transform championUI = championsFrameArray[index].transform.Find("champion");
        Transform top = championUI.Find("top");
        Transform bottom = championUI.Find("bottom");
        Transform name = bottom.Find("Name");
        Transform cost = bottom.Find("Cost");


        //assign texts from champion info to unit frames
        name.GetComponent<Text>().text = champion.uiname;
        cost.GetComponent<Text>().text = champion.cost.ToString();

    }

    /// <summary>
    /// 隐藏商店UI
    /// </summary>
    public void HideChampionFrame(int index)
    {
        championsFrameArray[index].transform.Find("champion").gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示商店UI
    /// </summary>
    public void ShowShopItems()
    {
        //unhide all champion frames
        for (int i = 0; i < championsFrameArray.Length; i++)
        {
            championsFrameArray[i].transform.Find("champion").gameObject.SetActive(true);
        }
    }
    /// <summary>
    /// 显示排行榜
    /// </summary>
    public void showTop()
    {
        for(int i=0;i < gamePlayController.currentRoomPlayers;i++)
        {
            Top[i].gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 刷新排行榜
    /// </summary>
    public void updateTop(Player[] player)
    {
        sortPlayer(player);
        for (int i = 0; i < player.Length; i++)
        {
            playerName[i].text = player[i].playerName;
            Gold[i].text = player[i].Gold.ToString();
            textHealth[i].text = player[i].Health.ToString()+"%";
        }
    }
    //选择排序，根据血量排行
    public void sortPlayer(Player[] player)
    {
        int index = 0;
        Player temp;
        
        for(int i = 0; (i + 1) < player.Length; i++)
        {
            for(int j = i+1;j < player.Length; j++)
            {
                if (player[j].Health > player[i].Health)
                    index = j;  
            }
            temp = (Player)player[index].Clone();
            player[index] = (Player)player[i].Clone(); ;
            player[i] = (Player)temp.Clone();
        }
    }
}
