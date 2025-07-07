using System;
using System.Collections.Generic;
using General;
using UnityEngine;

namespace Card
{
    /// <summary>
    /// 工厂模式，创建卡牌用的。把资源创建和游戏管理逻辑分开
    /// </summary>
    public class CardFactory : SingletonMono<CardFactory>
    {
        public GameObject cardPrefab;
        public List<CardData> cardDataSet = new List<CardData>();
        
        private Dictionary<int,CardData> heartsCardContainer = new Dictionary<int,CardData>();
        private Dictionary<int,CardData> diamondsCardContainer = new Dictionary<int,CardData>();
        private Dictionary<int,CardData> clubsCardContainer = new Dictionary<int,CardData>();
        private Dictionary<int,CardData> spadesCardContainer = new Dictionary<int,CardData>();
        
        private void Start()
        {
            //初始化卡牌
            foreach (var c in cardDataSet)
            {
                SetupCardData(c);
            }
        }

        public GameObject InstantiateCard(CardSuit suit,int cardNumber,Transform parent = null)
        {
            var go = Instantiate(cardPrefab,parent);
            
            var card = go.GetComponent<Card>();
            if (card != null)
            {
                var data = GetCardData(suit, cardNumber);
                card.LoadCardData(data);
                return go;
            }
            
            Destroy(go);
            return null;
        }

        private void SetupCardData(CardData cardData)
        {
            switch (cardData.suit)
            {
                case CardSuit.Hearts:
                    heartsCardContainer.Add(cardData.value,cardData);
                    break;
                case CardSuit.Diamonds:
                    diamondsCardContainer.Add(cardData.value,cardData);
                    break;
                case CardSuit.Clubs:
                    clubsCardContainer.Add(cardData.value,cardData);
                    break;
                case CardSuit.Spades:
                    spadesCardContainer.Add(cardData.value,cardData);
                    break;
                default:
                    return;
            }
        }
        
        private CardData GetCardData(CardSuit suit, int cardNumber)
        {
            Dictionary<int,CardData> data = null;
            switch (suit)
            {
                case CardSuit.Hearts:
                    data =  heartsCardContainer;
                    break;
                case CardSuit.Diamonds:
                    data =  diamondsCardContainer;
                    break;
                case CardSuit.Clubs:
                    data =  clubsCardContainer;
                    break;
                case CardSuit.Spades:
                    data =  spadesCardContainer;
                    break;
                default:
                    return null;
            }

            return data[cardNumber];
        }
    }
}