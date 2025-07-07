using System;
using UnityEngine;

namespace Card
{
    public enum CardSuit
    {
        Clubs = 1,
        Diamonds = 2,
        Hearts = 3,
        Spades = 4,
    }
    
    [Serializable]
    [CreateAssetMenu(menuName = "CardAsset/CardSO")]
    public class CardData : ScriptableObject
    {
        public CardSuit suit;
        public int value;
        public Sprite cardFrontFace;
        public Sprite cardBackFace;
    }
}
