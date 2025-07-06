using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlackjackController : MonoBehaviour
{
    public enum PlayerState
    {
        Preparing,  // 准备中
        Playing,    // 游戏中
        Stand       // 已停牌
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
    
    [Header("卡牌预设")]
    public GameObject cardPrefab;

    private PlayerState currentState = PlayerState.Preparing;
    private int playerId;
    private int opponentId;
    private int coins = 1000;
    private int currentBet;
    private List<Card> handCards = new();
    
    private void Start()
    {
        // 注册网络事件
        NetWorkManager.instance.RegisterGameHandler("start_msg", OnGameStart);
        NetWorkManager.instance.RegisterGameHandler("update_player_info", OnCardDealt);
        NetWorkManager.instance.RegisterGameHandler("result", OnGameResult);
        NetWorkManager.instance.RegisterGameHandler("game_restart", OnGameRestart);
        
        // 按钮事件
        hitButton.onClick.AddListener(() => NetWorkManager.instance.SendGameCommand("hit", playerId));
        standButton.onClick.AddListener(() => NetWorkManager.instance.SendGameCommand("stand", playerId));
        restartButton.onClick.AddListener(() => NetWorkManager.instance.SendGameCommand("restart_game", playerId));
        leaveButton.onClick.AddListener(() => NetWorkManager.instance.SendGameCommand("leave", playerId));
        // 按钮事件
        betButton.onClick.AddListener(PlaceBet);
    }

    private void OnGameStart(string[] data)
    {
        currentState = PlayerState.Playing;
        UpdateGameUI();
    }

    private void OnCardDealt(string[] data)
    {
        int targetPlayerId = int.Parse(data[1]);
        string suit = data[2];
        string value = data[3];
        
        Transform parent = (targetPlayerId == playerId) ? playerHandArea : opponentHandArea;
        CreateCard(suit, value, parent);
    }

    private void OnGameResult(string[] data)
    {
        // 格式: ["result", 1, winner_id] 或 ["result", 0]（平局）
        if (data[1] == "1")
        {
            int winnerId = int.Parse(data[2]);
            gameStatusText.text = (winnerId == playerId) ? "你赢了!" : "对手赢了!";
        }
        else
        {
            gameStatusText.text = "平局!";
        }
        
        currentState = PlayerState.Preparing;
        restartButton.gameObject.SetActive(true);
    }

    private void OnGameRestart(string[] data)
    {
        // 清空手牌
        foreach (Transform child in playerHandArea) Destroy(child.gameObject);
        foreach (Transform child in opponentHandArea) Destroy(child.gameObject);
        
        gameStatusText.text = "新游戏开始";
        restartButton.gameObject.SetActive(false);
    }

    private void CreateCard(string suit, string value, Transform parent)
    {
        GameObject cardObj = Instantiate(cardPrefab, parent);
        CardDisplay display = cardObj.GetComponent<CardDisplay>();
        display.SetCard(suit, value);
    }

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
            if (betAmount > coins)
            {
                Debug.Log("下注金额超过现有金币");
                return;
            }
            
            NetWorkManager.instance.SendGameCommand("bet", playerId, betAmount);
        }
    }

    private void StartGame()
    {
        NetWorkManager.instance.SendGameCommand("start_sendcard");
    }
}
