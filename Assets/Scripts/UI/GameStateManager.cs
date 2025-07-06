using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 游戏状态管理器
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    
    public int PlayerId { get; private set; }
    public int Coins { get; private set; }
    public int CurrentBet { get; private set; }
    public List<Card> PlayerHand { get; } = new();
    public List<Card> OpponentHand { get; } = new();
    
    public UnityEvent<int> OnCoinsUpdated = new();
    public UnityEvent<int> OnBetUpdated = new();
    public UnityEvent<BlackjackController.PlayerState> OnStateChanged = new();

    private BlackjackController.PlayerState _currentState = BlackjackController.PlayerState.Preparing;
    public BlackjackController.PlayerState CurrentState
    {
        get => _currentState;
        set
        {
            _currentState = value;
            OnStateChanged.Invoke(value);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(int playerId, int initialCoins)
    {
        PlayerId = playerId;
        Coins = initialCoins;
        CurrentBet = 0;
        PlayerHand.Clear();
        OpponentHand.Clear();
        CurrentState = BlackjackController.PlayerState.Preparing;
    }

    public void UpdateFromServer(string[] data)
    {
        switch (data[0])
        {
            case "update_player_info":
                int playerId = int.Parse(data[1]);
                if (playerId == this.PlayerId)
                {
                    Coins = int.Parse(data[2]);
                    CurrentBet = int.Parse(data[3]);
                    OnCoinsUpdated.Invoke(Coins);
                    OnBetUpdated.Invoke(CurrentBet);
                    
                    // 更新手牌
                    PlayerHand.Clear();
                    for (int i = 4; i < data.Length; i += 2)
                    {
                        PlayerHand.Add(new Card(data[i], data[i+1]));
                    }
                }
                break;
                
            case "result":
                if (data[1] == "1") // 有赢家
                {
                    int winnerId = int.Parse(data[2]);
                    if (winnerId == PlayerId)
                    {
                        Coins += CurrentBet * 2; // 赢回双倍下注
                    }
                }
                CurrentState = BlackjackController.PlayerState.Preparing;
                break;
        }
    }
}
