using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    [Header("UI组件")]
    public Image cardImage;
    public Image suitImage;
    public TMP_Text valueText;
    public TMP_Text cornerValueText;
    public GameObject backSide;

    [Header("花色图标")]
    public Sprite heartsSprite;
    public Sprite diamondsSprite;
    public Sprite clubsSprite;
    public Sprite spadesSprite;
    
    [Header("颜色设置")]
    public Color redColor = Color.red;
    public Color blackColor = Color.black;

    private Card_Legacy _card;
    private bool _isFaceUp = true;

    public Card_Legacy Card => _card;

    // 设置卡牌数据并更新显示
    public void SetCard(string suit, string value)
    {
        _card = new Card_Legacy(suit, value);
        UpdateDisplay();
    }

    public void SetCard(Card_Legacy card)
    {
        _card = card;
        UpdateDisplay();
    }

    // 翻转卡牌（正面/背面）
    public void Flip(bool faceUp)
    {
        _isFaceUp = faceUp;
        backSide.SetActive(!faceUp);
        
        if (cardImage) cardImage.enabled = faceUp;
        if (suitImage) suitImage.enabled = faceUp;
        if (valueText) valueText.enabled = faceUp;
        if (cornerValueText) cornerValueText.enabled = faceUp;
    }

    private void UpdateDisplay()
    {
        if (_card == null) return;

        // 设置花色图标
        Sprite suitSprite = null;
        Color suitColor = blackColor;
        
        switch (_card.CardSuit)
        {
            case Card_Legacy.Suit.Hearts:
                suitSprite = heartsSprite;
                suitColor = redColor;
                break;
            case Card_Legacy.Suit.Diamonds:
                suitSprite = diamondsSprite;
                suitColor = redColor;
                break;
            case Card_Legacy.Suit.Clubs:
                suitSprite = clubsSprite;
                break;
            case Card_Legacy.Suit.Spades:
                suitSprite = spadesSprite;
                break;
        }

        if (suitImage)
        {
            suitImage.sprite = suitSprite;
            suitImage.color = suitColor;
        }

        // 设置点数文本
        string valueString = GetValueDisplayString();
        if (valueText) valueText.text = valueString;
        if (cornerValueText) cornerValueText.text = valueString;
        
        // 设置文本颜色
        if (valueText) valueText.color = suitColor;
        if (cornerValueText) cornerValueText.color = suitColor;
    }

    private string GetValueDisplayString()
    {
        switch (_card.CardValue)
        {
            case Card_Legacy.Value.Ace: return "A";
            case Card_Legacy.Value.Jack: return "J";
            case Card_Legacy.Value.Queen: return "Q";
            case Card_Legacy.Value.King: return "K";
            default: return ((int)_card.CardValue).ToString();
        }
    }

    // 获取卡牌点数（用于计算手牌总值）
    public int GetValue()
    {
        return _card?.GetPointValue() ?? 0;
    }
}