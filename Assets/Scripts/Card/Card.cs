﻿using UnityEngine;

namespace Card
{
    public class Card : MonoBehaviour
    {
        public CardSuit suit;
        public int value;
        public SpriteRenderer BackSprite;
        public SpriteRenderer FrontSprite;
            
        public void LoadCardData(CardData cardData, bool isOpponentHandArea = false)
        {
            suit = cardData.suit;
            value = cardData.value;
            BackSprite.sprite = cardData.cardBackFace;
            FrontSprite.sprite = cardData.cardFrontFace;
            if (isOpponentHandArea)
            {
                FrontSprite.enabled = false;
            }
        }
    }
}