using System;

[Serializable]
public class Card
{
    public enum Suit
    {
        Hearts = 1,   // 红心
        Diamonds, // 方块
        Clubs,    // 梅花
        Spades    // 黑桃
    }

    public enum Value
    {
        Ace = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }

    public Suit CardSuit { get; }
    public Value CardValue { get; }
    
    // 用于网络传输的字符串表示
    public string SuitString => CardSuit.ToString().ToLower();
    public string ValueString => CardValue.ToString().ToLower();

    public Card(string suit, string value)
    {
        if (!Enum.TryParse(suit, true, out Suit parsedSuit))
        {
            throw new ArgumentException($"Invalid suit: {suit}");
        }
        
        if (!Enum.TryParse(value, true, out Value parsedValue))
        {
            throw new ArgumentException($"Invalid value: {value}");
        }
        
        CardSuit = parsedSuit;
        CardValue = parsedValue;
    }

    public Card(Suit suit, Value value)
    {
        CardSuit = suit;
        CardValue = value;
    }

    // 获取卡牌点数（A可计为1或11）
    public int GetPointValue(bool aceAsEleven = true)
    {
        switch (CardValue)
        {
            case Value.Ace:
                return aceAsEleven ? 11 : 1;
            case Value.Jack:
            case Value.Queen:
            case Value.King:
                return 10;
            default:
                return (int)CardValue;
        }
    }

    public override string ToString()
    {
        return $"{CardValue} of {CardSuit}";
    }
}