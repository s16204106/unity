using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviourPunCallbacks
{
    public GamePlayController gamePlayController;
    public CameraBehavior cameraBehavior;
    public ShowFPS showFps;

    //商店、金币和血量
    public ChampionShop championShop;
    public Text timerText;
    public Text goldText;
    public Text hpText;
    public Text text;

    //排行榜
    public GameObject[] Top;
    public Text[] textHealth;
    public Text[] playerName;
    public Text[] Gold;
    public Text[] Ranking;

    //聊天框
    public GameObject sendMessage;
    public InputField inputFieldChat;
    public Text[] messageText;
    private string[] messageList = new string[8];
    private int messageNum = 0;

    public GameObject timer;
    public GameObject[] championsFrameArray;
    private int index;

    //网络和帧数
    public Text ping;
    public Text fps;

    //赢家图标
    public GameObject winnerIcon;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        showFPSAndPing();
        //Debug.Log("CurrentSelect:"+EventSystem.current.currentSelectedGameObject);
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
        Transform icon = top.Find("icon");
        Transform name = bottom.Find("Name");
        Transform cost = bottom.Find("Cost");
        Transform number = bottom.Find("Number");


        //assign texts from champion info to unit frames
        icon.GetComponent<Image>().sprite = champion.sprite;
        name.GetComponent<Text>().text = champion.uiname;
        cost.GetComponent<Text>().text = champion.cost.ToString();
        number.GetComponent<Text>().text = champion.number.ToString();
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
            Ranking[i].text = "Lv."+player[i].Rank.ToString();
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
    
    //聊天窗口
    public void showOrHideInputField()
    {
        if(sendMessage.activeSelf)
        {
            //关闭输入框，发送消息
            string name = PhotonNetwork.CurrentRoom.GetPlayer(gamePlayController.playerID).NickName; 
            string message = name+":"+inputFieldChat.text.ToString();
            
            photonView.RPC("SendChatMessage",RpcTarget.All, message);
            sendMessage.SetActive(false);
            inputFieldChat.text = "";
        }
        else
        {
            //激活并选中
            sendMessage.SetActive(true);
            EventSystem.current.SetSelectedGameObject(inputFieldChat.gameObject);
        }
    }
    
    [PunRPC]
    public void SendChatMessage(string newMessage)
    {
        //消息是否为空
        if (string.IsNullOrEmpty(newMessage))
        {
            return;
        }
        //消息槽是否已满
        if (messageNum == 7)
        {
            messageList[messageNum] = null;
            messageNum--;
        }

        //更新消息槽
        for (int i = messageNum; i > 0; i--)
        {
            messageList[i] = messageList[i - 1];
        }
        messageList[0] = newMessage;
        messageNum++;
        updateMessage();
    }

    //更新消息记录
    public void updateMessage()
    {
        for (int i = 0; i < messageList.Length; i++)
        {
            if (!string.IsNullOrEmpty(messageList[i]))
            {
                messageText[i].text = messageList[i].ToString();
                messageText[i].gameObject.SetActive(true);
            }
            else
            {
                messageText[i].gameObject.SetActive(false);
            }
        }
    }
    //显示FPS和Ping
    public void showFPSAndPing()
    {
       
        int pingValue = PhotonNetwork.GetPing();
        string fpsValue = showFps.strFPSValue.Substring(0,2);
        if (0<=pingValue&&pingValue<=200)
        {
            ping.color = Color.green;
        }
        else if(200<pingValue&&pingValue<=520)
        {
            ping.color = Color.yellow;
        }
        else
        {
            ping.color = Color.red;
        }

        //显示
        ping.text = "Ping:" + pingValue.ToString();
        fps.text = "FPS:"+fpsValue;

    }

    //胜利标志
    public void showWinnerIcon()
    {
        winnerIcon.gameObject.SetActive(true);
    }
    public void hideWinnerIcon()
    {
        winnerIcon.gameObject.SetActive(false);
    }

    //离开房间
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("DemoAsteroids-LobbyScene");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

}
