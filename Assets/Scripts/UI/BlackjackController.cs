using System;
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
        public List<GameObject> playerHandCards;

        public PlayerInfo(string playerName)
        {
            this.playerName = playerName;
            this.playerScore = 0;
            this.playerBet = 0;
            this.playerTotalCoin = 0;
            playerHandCards = new List<GameObject>();
        }
        
        public void UpdatePlayerInfo(int score, int bet, int coin)
        {
            playerScore = score;
            playerBet = bet;
            playerTotalCoin = coin;
        }
        
        public void ClearHandCards()
        {
            foreach (GameObject card in playerHandCards)
            {
                Destroy(card);
            }
            playerHandCards.Clear();
        }
    }

    #region 玩家状态ui组件

    public Text playerNameText;
    public Text playerTotalCoinText;
    public Text playerScoreText;
    //
    public Text opponentNameText;
    public Text opponentTotalCoinText;
    public Text opponentScoreText;
    //
    public Text resultText;
    
    #endregion

    // [Header("UI组件")]
    public Button hitButton;
    public Button standButton;
    public Button startButton;
    // public Button leaveButton;
    public Button betButton;
    // public TMP_InputField betInput;
    // public TMP_Text gameStatusText;
    public Action OnGameStartSuccessEvent;
    public Action OnGameStartFailEvent;
    public Action<int> OnBetSuccessEvent;
    public Action OnBetFailEvent;
    public Action OnHitSuccessEvent;
    public Action OnHitFailEvent;
    public Action<string> OnPlayerStandSuccessEvent;
    public Action OnPlayerStandFailEvent;
    public Action<string> OnGameHasWinnerEvent;
    public Action OnBetTooMuchEvent;
    public Action OnGameHasNoWinnerEvent;
    public Action OnGameResultedEvent;
    public Action OnGameRestartSuccessEvent;
    public Action OnGameRestartFailEvent;
    public PlayerInfo GetPlayerInfo => playerInfo;
    public PlayerInfo GetOpponentInfo => opponentInfo;
    
    //public Action<string> OnGameHasWinner;
    
    public Transform playerHandArea;
    public Transform opponentHandArea;


    
    private PlayerInfo playerInfo;
    private PlayerInfo opponentInfo;
   
    //private PlayerState currentState = PlayerState.Preparing;
    
    private void Start()
    {
        playerInfo = new PlayerInfo(NetWorkManager.instance.PlayerName);
        opponentInfo = new PlayerInfo("");
        // 注册网络事件
        NetWorkManager.instance.RegisterCmd("start_msg", OnGameStart);
        NetWorkManager.instance.RegisterCmd("bet", OnPlayerBet);
        NetWorkManager.instance.RegisterCmd("hit", OnPlayerHit);
        NetWorkManager.instance.RegisterCmd("send_card_to_player", OnReceiveCard);
        NetWorkManager.instance.RegisterCmd("update_player_info", OnUpdatePlayerInfo);
        NetWorkManager.instance.RegisterCmd("player_stand", OnPlayerStand);
        NetWorkManager.instance.RegisterCmd("result", OnGameResult);
        NetWorkManager.instance.RegisterCmd("game_restart", OnGameRestart);
        NetWorkManager.instance.RegisterCmd("result", OnGameResult);
        
        NetWorkManager.instance.SendMessageToServer("get_player_info");
        
        //// 按钮事件
        hitButton.onClick.AddListener(Hit);
        standButton.onClick.AddListener(Stand);
        betButton.onClick.AddListener(()=>PlaceBet(20));
        startButton.onClick.AddListener(StartGame);
        //leaveButton.onClick.AddListener(() => NetWorkManager.instance.SendMessageToServer("leave"));
        //// 按钮事件
    }

    private void OnPlayerHit(ServerMessageEventArgs args)
    {
        if (args.RunResult == ServerRunResult.Success)
        {
            OnHitSuccessEvent?.Invoke();
            Debug.Log($"Player {playerInfo.playerName} hit success");
        }
        else
        {
            OnHitFailEvent?.Invoke();
            Debug.Log($"Player {playerInfo.playerName} hit failed");
        }
    }

    private void OnPlayerBet(ServerMessageEventArgs args)
    {
        if (args.RunResult == ServerRunResult.Success)
        {
            string[] parameters = args.Parameters.Split(',');
            int coin = int.Parse(parameters[0]);
            
            OnBetSuccessEvent?.Invoke(coin);
            Debug.Log($"Player {playerInfo.playerName} bet {coin}");
        }
        else
        {
            OnBetFailEvent?.Invoke();
            Debug.Log($"Player {playerInfo.playerName} bet failed");
        }
    }

    private void OnGameStart(ServerMessageEventArgs args)
    {
        if (args.RunResult == ServerRunResult.Success)
        {
            OnGameStartSuccessEvent?.Invoke();
            Debug.Log($"start game success");
        }
        else
        {
            OnGameStartFailEvent?.Invoke();
            Debug.Log($"start game failed");
        }
    }

    private void OnReceiveCard(ServerMessageEventArgs args)
    {
        string[] parameters = args.Parameters.Split(',');
        string targetPlayerName = parameters[0];
        int suit = int.Parse(parameters[1]);
        int value = int.Parse(parameters[2]);

        if (targetPlayerName != playerInfo.playerName)
        {
            Debug.Log($"received card: {args.Raw}");
            var card = CardFactory.instance.InstantiateCard((CardSuit)suit,value,opponentHandArea,true);
            opponentInfo.playerHandCards.Add(card);
        }
        else
        {
            var card = CardFactory.instance.InstantiateCard((CardSuit)suit,value,playerHandArea);
            playerInfo.playerHandCards.Add(card);
        }
    }

    private void OnGameResult(ServerMessageEventArgs args)
    {
        string[] parameters = args.Parameters.Split(',');
        // 格式: ["result", 0, winner_id] 或 ["result", 1]（平局）
        if (args.RunResult == ServerRunResult.Success)
        {
            string winnerId = parameters[0];
            resultText.text = (winnerId == playerInfo.playerName) ? "你赢了!" : "对手赢了!";
            OnGameHasWinnerEvent?.Invoke(winnerId);
        }
        else
        {
            resultText.text = "平局!";
            OnGameHasNoWinnerEvent?.Invoke();
        }
        
        OnGameResultedEvent?.Invoke();
        
        // 先放这了
        TurnOverCards();
        StartCoroutine(ShowResult());

        //TODO: do something else
        //currentState = PlayerState.Preparing;
        //restartButton.gameObject.SetActive(true);
    }

    private void OnPlayerStand(ServerMessageEventArgs args)
    {
        if (args.RunResult == ServerRunResult.Success)
        {
            string[] parameters = args.Parameters.Split(',');
            string targetPlayerName = parameters[0];
            OnPlayerStandSuccessEvent?.Invoke(targetPlayerName);
        }
        else
        {
            OnPlayerStandFailEvent?.Invoke();
        }
    }

    private void OnUpdatePlayerInfo(ServerMessageEventArgs args)
    {
        Debug.Log($"OnUpdatePlayerInfo: {args.RunResult}");
        
        if (args.RunResult == ServerRunResult.Success)
        {
            string[] parameters = args.Parameters.Split(',');
            string targetPlayerName = parameters[0];
            int coin = int.Parse(parameters[1]);
            int score = int.Parse(parameters[2]);
            string handCards = parameters[3];
            
            Debug.Log($"OnUpdatePlayerInfo: {targetPlayerName}, {coin}, {score}, {handCards}");
            
            if (handCards == "empty")
            {
                if (targetPlayerName != playerInfo.playerName)
                {
                    opponentInfo.playerName = targetPlayerName;
                    opponentInfo.UpdatePlayerInfo(score,0,coin);
                    opponentInfo.ClearHandCards();
                }
                else
                {
                    playerInfo.UpdatePlayerInfo(score,0,coin);
                    playerInfo.ClearHandCards();
                }
            }
            
            //// 状态ui
            if (targetPlayerName != playerInfo.playerName)
            {
                opponentNameText.text = "昵称：" + opponentInfo.playerName;
                opponentTotalCoinText.text = "筹码：" + coin;
                opponentScoreText.text = "得分：" + score;
            }
            else
            {
                playerNameText.text = "昵称：" + playerInfo.playerName;
                opponentTotalCoinText.text = "筹码：" + coin;
                opponentScoreText.text = "得分：" + score;
            }
            ////

        }
        else
        {
            Debug.LogError(args.Cmd + args.RunResult);
        }
    }
    
    private void OnGameRestart(ServerMessageEventArgs args)
    {
        if (args.RunResult == ServerRunResult.Success)
        {
            OnGameRestartSuccessEvent?.Invoke();   
        }
        else
        {
            OnGameRestartFailEvent?.Invoke();
        }
        //gameStatusText.text = "新游戏开始";
        //restartButton.gameObject.SetActive(false);
    }
    
    public void PlaceBet(int betValue)
    {
        if (betValue > playerInfo.playerTotalCoin)
        {
            Debug.LogError("下注金额超过现有金币");
            OnBetTooMuchEvent?.Invoke();
            return;
        }
            
        NetWorkManager.instance.SendMessageToServer("bet", new [] {betValue.ToString()});
    }

    public void Hit()
    {
        NetWorkManager.instance.SendMessageToServer("hit");
    }

    public void StartGame()
    {
        NetWorkManager.instance.SendMessageToServer("start_sendcard");
    }
    public void Stand()
    {
        NetWorkManager.instance.SendMessageToServer("stand");
    }
    
    // private void CreateCard(string suit, string value, Transform parent)
    // {
    //     GameObject cardObj = Instantiate(cardPrefab, parent);
    //     CardDisplay display = cardObj.GetComponent<CardDisplay>();
    //     display.SetCard(suit, value);
    // }

    // private void UpdateGameUI()
    // {
    //     //hitButton.interactable = (currentState == PlayerState.Playing);
    //     //standButton.interactable = (currentState == PlayerState.Playing);
    //     //restartButton.interactable = (currentState == PlayerState.Preparing);
    // }

    private void TurnOverCards()
    {
        foreach (var card in opponentInfo.playerHandCards)
        {
            card.GetComponent<Card.Card>().FrontSprite.enabled = true;
        }
    }

    private IEnumerator ShowResult()
    {
        resultText.enabled = true;
        yield return new WaitForSeconds(1f);
        resultText.enabled = false;
    }
}
