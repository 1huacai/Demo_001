using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FrameWork.GUI
{
    /// <summary>
    /// 滑动面板
    /// 同时支持滑动 缩放 点击
    /// </summary>
    public class ScrollView : ScrollRect, IPointerClickHandler
    {
        enum TouchType
        {
            Click,
            Move,
            Scale,
        }
        /// <summary>
        /// 当前触摸类型
        /// </summary>
        private TouchType touchType = TouchType.Click;
        /// <summary>
        /// 保存触碰点 做双点缩放和单点移动判定
        /// </summary>
        private List<PointerEventData> points = new List<PointerEventData>(2);
        /// <summary>
        /// 缩放中心点
        /// </summary>
        public Vector2 screenPoint;
        /// <summary>
        /// 最大缩放值
        /// </summary>
        [SerializeField]
        public float maxScale = 1.3f;
        /// <summary>
        /// 当前缩放值
        /// </summary>
        public float scaleZoom = 1;
        /// <summary>
        /// 当前缩放因子
        /// </summary>
        [SerializeField]
        public float ScaleFactor = 10000;
        /// <summary>
        /// 点击回调
        /// </summary>
        public UnityAction<Vector2> OnClickCallBack;
        /// <summary>
        /// 缩放回调
        /// </summary>
        public UnityAction<float> OnScaleCallBack;

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (points.Count <= 2)
            {
                points.Add(eventData);
                if (points.Count == 1)
                {
                    base.OnBeginDrag(eventData);
                    touchType = TouchType.Move;
                }
                else
                    touchType = TouchType.Scale;
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (points.Contains(eventData))
            {
                if (touchType == TouchType.Scale)
                {
                    if (OnScaleCallBack == null)
                        return;
                    float distance = GetDistance(points[0].position, points[1].position);
                    float distance1 = GetDistance(points[0].pressPosition, points[1].pressPosition);
                    if (distance1 <= 0)
                        return;

                    screenPoint = Vector2.Lerp(points[0].position, points[1].position, 0.5f);
                    OnScaleCallBack(scaleZoom - ((distance1 - distance) / ScaleFactor));
                }
                else
                    base.OnDrag(eventData);
            }
        }

        private float GetDistance(Vector2 v1, Vector2 v2)
        {
            return Mathf.Sqrt(Mathf.Pow(v1.x - v2.x, 2) + Mathf.Pow(v1.y - v2.y, 2));
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (points.Contains(eventData))
            {
                if (points[0] == eventData)
                {
                    base.OnEndDrag(eventData);
                    if (touchType == TouchType.Scale)
                        base.StopMovement();
                }
                points.Remove(eventData);
                if (points.Count == 1)
                {
                    touchType = TouchType.Move;
                    base.OnBeginDrag(points[0]);
                }
                else
                    touchType = TouchType.Click;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (touchType == TouchType.Click)
            {
                if (OnClickCallBack != null)
                    OnClickCallBack(eventData.position);
            }
        }

        /* private void SetScale(float scale)
         {
             AorUIManager uiManager = Main.Instance.GetManager<AorUIManager>();
             float minScale = Screen.height / uiManager.StageScale.y / base.content.rect.height;
             scale = Mathf.Clamp(scale, minScale, maxScale);

             Vector3 mousePosWorld = uiManager.UICamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, 0));
             base.content.localScale = new Vector3(scale, scale);
             scaleZoom = scale;
             Vector3 mousePosWorld2 = uiManager.UICamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, 0));
             Vector3 offset = mousePosWorld2 - mousePosWorld;
             offset = base.content.localPosition - offset;

             //fix position
             float maxX = base.content.rect.width * scale / 2 - Screen.width / uiManager.StageScale.x / 2;
             float maxY = base.content.rect.height * scale / 2 - Screen.height / uiManager.StageScale.y / 2;

             offset.x = Mathf.Clamp(offset.x, -maxX, maxX);
             offset.y = Mathf.Clamp(offset.y, -maxY, maxY);

             base.content.localPosition = offset;
         }*/



    }
}
