using System.Collections;
using System.Collections.Generic;
using Card;
using CardNetWork;
using General;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// FIXME:很多功能未测试，很多情况可能没考虑清楚
/// </summary>
public class BlackjackController : SingletonMono<BlackjackController>
{
    public enum PlayerState
    {
        Preparing,  // 准备中
        Playing,    // 游戏中
        Stand       // 已停牌
    }

    public struct PlayerInfo
    {
        public string playerName;
        public int playerScore;
        public int playerBet;
        public int playerTotalCoin;
    }

    [Header("UI组件")]
    public Button hitButton;
    public Button standButton;
    public Button restartButton;
    public Button leaveButton;
    public Button betButton;
    public TMP_InputField betInput;
    public TMP_Text gameStatusText;
    public Transform playerHandArea;
    public Transform opponentHandArea;

    private PlayerState currentState = PlayerState.Preparing;
    public PlayerInfo playerInfo;
    
    private void Start()
    {
        // 注册网络事件
        NetWorkManager.instance.RegisterCmd("start_msg", OnGameStart);
        NetWorkManager.instance.RegisterCmd("update_player_info", OnCardDealt);
        NetWorkManager.instance.RegisterCmd("result", OnGameResult);
        //NetWorkManager.instance.RegisterCmd("game_restart", OnGameRestart);
        
        // 按钮事件
        hitButton.onClick.AddListener(() => NetWorkManager.instance.SendMessageToServer("hit"));
        standButton.onClick.AddListener(() => NetWorkManager.instance.SendMessageToServer("stand"));
        restartButton.onClick.AddListener(() => NetWorkManager.instance.SendMessageToServer("restart_game"));
        leaveButton.onClick.AddListener(() => NetWorkManager.instance.SendMessageToServer("leave"));
        // 按钮事件
        betButton.onClick.AddListener(PlaceBet);
    }

    private void OnGameStart(ServerMessageEventArgs args)
    {
        currentState = PlayerState.Playing;
        UpdateGameUI();
    }

    private void OnCardDealt(ServerMessageEventArgs args)
    {
        string[] parameters = args.Parameters.Split(',');
        string targetPlayerName = parameters[0];
        int suit = int.Parse(parameters[1]);
        int value = int.Parse(parameters[2]);
        
        Transform parent = (targetPlayerName == playerInfo.playerName) ? playerHandArea : opponentHandArea;
        //CreateCard(suit, value, parent);
        CardFactory.instance.InstantiateCard((CardSuit)suit,value,parent);
    }

    private void OnGameResult(ServerMessageEventArgs args)
    {
        string[] parameters = args.Parameters.Split(',');
        // 格式: ["result", 1, winner_id] 或 ["result", 0]（平局）
        if (args.Code == "1")
        {
            string winnerId = parameters[0];
            gameStatusText.text = (winnerId == playerInfo.playerName) ? "你赢了!" : "对手赢了!";
        }
        else
        {
            gameStatusText.text = "平局!";
        }
        
        currentState = PlayerState.Preparing;
        restartButton.gameObject.SetActive(true);
    }

    //FIXME: not test yet
    private void OnGameRestart(ServerMessageEventArgs args)
    {
        // 清空手牌
        foreach (Transform child in playerHandArea) Destroy(child.gameObject);
        foreach (Transform child in opponentHandArea) Destroy(child.gameObject);
        
        gameStatusText.text = "新游戏开始";
        restartButton.gameObject.SetActive(false);
    }

    // private void CreateCard(string suit, string value, Transform parent)
    // {
    //     GameObject cardObj = Instantiate(cardPrefab, parent);
    //     CardDisplay display = cardObj.GetComponent<CardDisplay>();
    //     display.SetCard(suit, value);
    // }

    private void UpdateGameUI()
    {
        hitButton.interactable = (currentState == PlayerState.Playing);
        standButton.interactable = (currentState == PlayerState.Playing);
        restartButton.interactable = (currentState == PlayerState.Preparing);
    }
    
    
    private void PlaceBet()
    {
        if (int.TryParse(betInput.text, out int betAmount))
        {
            if (betAmount > playerInfo.playerTotalCoin)
            {
                Debug.Log("下注金额超过现有金币");
                return;
            }
            
            NetWorkManager.instance.SendMessageToServer("bet", new [] {betAmount.ToString()});
        }
    }

    private void StartGame()
    {
        NetWorkManager.instance.SendMessageToServer("start_sendcard");
    }
}
