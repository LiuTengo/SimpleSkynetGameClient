using System;
using System.Collections;
using System.Collections.Generic;
using General;
using UI;
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
    
        public Transform deckPosition; // 发牌起始位置
        public float dealDuration = 0.5f; // 发牌动画时长
        public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 移动曲线
        
        private void Start()
        {
            //初始化卡牌
            foreach (var c in cardDataSet)
            {
                SetupCardData(c);
            }
        }

        public GameObject InstantiateCard(CardSuit suit,int cardNumber,Transform parent = null,bool isOpponentHandArea = false)
        {
            var go = Instantiate(cardPrefab, deckPosition.position, Quaternion.identity, parent);
            
            var card = go.GetComponent<Card>();
            if (card != null)
            {
                var data = GetCardData(suit, cardNumber);
                card.LoadCardData(data, isOpponentHandArea);
                
                // 获取布局组件（如果存在）
                if (parent != null)
                {
                    HorizontalObjectLayout layout = parent.GetComponent<HorizontalObjectLayout>();
            
                    // 播放发牌动画
                    StartCoroutine(AnimateCardDeal(card.transform, parent, layout));
                }

                return go;
            }
            
            Destroy(go);
            return null;
        }
        
        // 发牌动画协程
        private IEnumerator AnimateCardDeal(Transform card, Transform parent, HorizontalObjectLayout layout)
        {
            // 计算目标位置（使用布局或默认位置）
            Vector3 targetPosition = CalculateTargetPosition(card, parent, layout);
        
            // 保存初始位置
            Vector3 startPosition = card.position;
        
            // 动画参数
            float elapsed = 0f;
        
            // 播放动画
            while (elapsed < dealDuration && card != null)
            {
                elapsed += Time.deltaTime;
                float t = moveCurve.Evaluate(elapsed / dealDuration);
            
                // 平滑移动
                card.position = Vector3.Lerp(startPosition, targetPosition, t);
            
                yield return null;
            }
        
            // 确保位置准确
            card.position = targetPosition;
        
            // 重新启用布局组件（如果存在）
            if (layout != null)
            {
                layout.UpdateLayout(); // 更新布局
            }
        }
    
        // 计算目标位置
        private Vector3 CalculateTargetPosition(Transform card, Transform parent, HorizontalObjectLayout layout)
        {
            if (layout != null)
            {
                // 获取当前所有活跃子对象
                List<Transform> children = layout.GetActiveChildren();
            
                // 计算新卡牌在布局中的索引
                int newIndex = children.Count - 1; // 新卡牌在最后
            
                // 使用布局组件计算位置
                return layout.GetPositionAtIndex(newIndex);
            }
            else
            {
                // 默认位置：在父对象中心
                return parent.position;
            }
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