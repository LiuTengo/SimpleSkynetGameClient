using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    [ExecuteInEditMode] // 允许在编辑模式下预览
    public class HorizontalObjectLayout : MonoBehaviour
    {
        [Header("布局设置")]
        public float spacing = 1f;        // 对象间距
        public Vector2 padding = Vector2.zero; // 边距 (左右, 上下)
        public bool centerChildren = true; // 是否居中
    
        [Header("尺寸控制")]
        public bool uniformWidth;         // 统一宽度
        public float targetWidth = 1f;    // 目标宽度
    
        // 缓存子对象尺寸（优化性能）
        private Dictionary<Transform, Vector2> sizeCache = new Dictionary<Transform, Vector2>();
    
        void Start() => UpdateLayout();
        void OnEnable() => UpdateLayout();
        
        // void OnTransformChildrenChanged() => UpdateLayout();
        // void OnValidate() => UpdateLayout();

        [ContextMenu("更新布局")]
        public void UpdateLayout()
        {
            List<Transform> activeChildren = GetActiveChildren();
            if (activeChildren.Count == 0) return;

            // 1. 计算总宽度
            float totalWidth = CalculateTotalWidth(activeChildren);

            // 2. 设置起始位置
            float startX = centerChildren ? 
                -totalWidth * 0.5f : 
                padding.x;
        
            float currentX = startX;

            // 3. 定位子对象
            foreach (Transform child in activeChildren)
            {
                // 获取实际尺寸（考虑统一宽度选项）
                Vector2 childSize = GetChildSize(child);

                // 设置位置（保持Y轴不变）
                Vector3 newPos = transform.position + 
                                 transform.right * currentX + 
                                 transform.up * padding.y;
            
                child.position = newPos;

                // 更新X位置
                currentX += childSize.x + spacing;
            }
        }

        // 获取指定索引的位置
        public Vector3 GetPositionAtIndex(int index)
        {
            List<Transform> children = GetActiveChildren();
            int totalCount = children.Count;
        
            if (totalCount == 0) 
                return transform.position;
        
            // 计算总宽度
            float totalWidth = (totalCount - 1) * spacing;
            float startX = centerChildren ? -totalWidth / 2f : padding.x;
        
            // 计算目标位置
            float xPos = startX + index * spacing;
            return transform.position + new Vector3(xPos, padding.y, -index * 0.01f);
        }

        public List<Transform> GetActiveChildren()
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeInHierarchy)
                    children.Add(child);
            }
            return children;
        }

        private float CalculateTotalWidth(List<Transform> children)
        {
            float width = -spacing; // 初始补偿最后一个间距
        
            foreach (Transform child in children)
            {
                width += GetChildSize(child).x + spacing;
            }
        
            return width + padding.x * 2;
        }

        private Vector2 GetChildSize(Transform child)
        {
            // 优先使用缓存
            if (sizeCache.TryGetValue(child, out Vector2 size))
                return size;
        
            // 自动检测尺寸（支持Collider2D/Renderer）
            Vector2 detectedSize = Vector2.one;
        
            if (TryGetComponent<Collider2D>(out var col2D))
                detectedSize = col2D.bounds.size;
            else if (TryGetComponent<Renderer>(out var renderer))
                detectedSize = renderer.bounds.size;
        
            // 应用宽度覆盖
            if (uniformWidth)
                detectedSize.x = targetWidth;
        
            sizeCache[child] = detectedSize;
            return detectedSize;
        }

        // 清除尺寸缓存（当对象缩放改变时调用）
        public void ClearSizeCache() => sizeCache.Clear();
    }
}