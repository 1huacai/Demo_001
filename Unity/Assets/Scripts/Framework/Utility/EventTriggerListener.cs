using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UGUI事件系统整理
/// 使用UGUI事件系统，需要当前的类继承事件接口，就可以在该类中加入它的事件方法，但是蛮繁琐的
/// </summary>

//使用方法
//EventTriggerListener.Get(btn.gameObject).onClick = OnClickBtn;     
//EventTriggerListener.Get(btn.gameObject).parameter = index;         
//private void OnClickBtn(GameObject go)     
//{     
//    int index = (int)EventTriggerListener.Get(go).parameter;     
//    Debug.Log("您单击的索引为：" + index);     
//}

public class EventTriggerListener : MonoBehaviour,
                                    IPointerClickHandler,       //指针点击
                                    IPointerDownHandler,        //指针按下
                                    IPointerEnterHandler,       //指针进入
                                    IPointerExitHandler,        //指针退出
                                    IPointerUpHandler,          //指针抬起
                                    ISelectHandler,             //选择事件
                                    IUpdateSelectedHandler,     //选中物体每帧触发
                                    IDeselectHandler,           //取消事件
                                    IDropHandler,               //在一个拖动过程中，释放鼠标键时触发此事件
                                    IScrollHandler,             //滚动事件
                                    IMoveHandler,               //移动事件(上下左右)
                                    IDragHandler,               //拖拽
                                    IBeginDragHandler,          //开始拖拽
                                    IEndDragHandler             //结束拖拽
{
    public delegate void VoidDelegate(GameObject go);
    public delegate void PointerEventDataDelegate(GameObject go,  float posX,float posY,float detalX,float detalY);
    public delegate void BaseEventDataDelegate(GameObject go, BaseEventData eventData);
    public delegate void AxisEventDataDelegate(GameObject go, float posX,float posY,int dir);

    /// <summary>
    /// 指针点击
    /// </summary>
    public PointerEventDataDelegate onClick;
    /// <summary>
    /// 指针按下
    /// </summary>
    public PointerEventDataDelegate onDown;
    /// <summary>
    /// 指针进入
    /// </summary>
    public PointerEventDataDelegate onEnter;
    /// <summary>
    /// 指针退出
    /// </summary>
    public PointerEventDataDelegate onExit;
    /// <summary>
    /// 指针抬起
    /// </summary>
    public PointerEventDataDelegate onUp;
    /// <summary>
    /// 选择事件
    /// </summary>
    public BaseEventDataDelegate onSelect;
    /// <summary>
    /// 选中物体每帧触发
    /// </summary>
    public BaseEventDataDelegate onUpdateSelect;
    /// <summary>
    /// 取消事件
    /// </summary>
    public BaseEventDataDelegate onDeSelect;
    /// <summary>
    /// 开始拖动
    /// </summary>
    public PointerEventDataDelegate onDragBegin;
    /// <summary>
    /// 拖动中事件
    /// </summary>
    public PointerEventDataDelegate onDrag;
    /// <summary>
    /// 拖动结束事件
    /// </summary>
    public PointerEventDataDelegate onDragEnd;
    /// <summary>
    /// 在一个拖动过程中，释放鼠标键时触发此事件
    /// </summary>
    public PointerEventDataDelegate onDrop;
    /// <summary>
    /// 滚动事件
    /// </summary>
    public PointerEventDataDelegate onScroll;
    /// <summary>
    /// 移动事件(上下左右)
    /// </summary>
    public AxisEventDataDelegate onMove;

    /// <summary>
    /// 参数
    /// </summary>
    public object parameter;

    /// <summary>
    /// 长按事件
    /// </summary>
    public VoidDelegate onLongPress;
    /// <summary>
    /// 长按1次事件
    /// </summary>
    public VoidDelegate onLongPressOnlyOne;

    public static System.Action<EventTriggerListener> destroyingEvent;

    private bool bLongPress = false;

    public int _downValue = 0;

    /// <summary>
    /// 按下到弹起的时间限制,超过这个限制后则不触发Click事件(毫秒)
    /// </summary>
    public const int ClickDelay = 500;
    /// <summary>
    /// 按下到弹起的距离阀值,超过这个值后则不认为是Click事件,这个值同样影响长按事件的逻辑判断;(屏幕像素)
    /// </summary>
    public const int PosThreshold = 50;
    /// <summary>
    /// 长按触发事件
    /// </summary>
    public const int longPressTime = 1000;
    public const float longPressOnlyOneTime = 1.0f;
    /// <summary>
    /// 是否向下穿透事件
    /// </summary>
    public bool penetrate = false;

    


    public static EventTriggerListener Get(GameObject go)
    {
        EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
        if (listener == null) listener = go.AddComponent<EventTriggerListener>();
        return listener;
    }

    public void OnDestroy()
    {
        destroyingEvent?.Invoke(this);
    }

    private void Update()
    {
        if (bLongPress)
        {
            _downValue += (int)(Time.deltaTime * 1000);
            if (_downValue > longPressTime)
            {
                onLongPress?.Invoke(gameObject);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        this.eventData = eventData;
        if (Input.touchCount >= 2) return;
        if (_downValue < ClickDelay && Vector2.Distance(eventData.position, eventData.pressPosition) < PosThreshold)
        {
            onClick?.Invoke(gameObject, eventData.position.x, eventData.position.y, eventData.delta.x, eventData.delta.y);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        this.eventData = eventData;
        bLongPress = true;
        _downValue = 0;
        onDown?.Invoke(gameObject, eventData.position.x, eventData.position.y, eventData.delta.x, eventData.delta.y);
        Invoke("OnLongPressOnlyOne", longPressOnlyOneTime);
        //穿透事件
        if (penetrate)
        {
            List<RaycastResult> result = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, result);
            GameObject cur = eventData.pointerCurrentRaycast.gameObject;
            for (int i = 0; i < result.Count; i++)
            {
                if (cur != result[i].gameObject)
                {
                    ExecuteEvents.Execute(result[i].gameObject, eventData, ExecuteEvents.dragHandler);
                }
                ExecuteEvents.Execute(result[i].gameObject, eventData, ExecuteEvents.pointerDownHandler);
            }
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        this.eventData = eventData;
        onEnter?.Invoke(gameObject, eventData.position.x, eventData.position.y, eventData.delta.x, eventData.delta.y);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        this.eventData = eventData;
        onExit?.Invoke(gameObject, eventData.position.x, eventData.position.y, eventData.delta.x, eventData.delta.y);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        bLongPress = false;
        CancelInvoke("OnLongPressOnlyOne");
        _downValue = 0;
        this.eventData = eventData;
        onUp?.Invoke(gameObject, eventData.position.x, eventData.position.y, eventData.delta.x, eventData.delta.y);
    }
    public void OnSelect(BaseEventData eventData)
    {
        this.eventData = eventData;
        onSelect?.Invoke(gameObject, eventData);
    }
    public void OnUpdateSelected(BaseEventData eventData)
    {
        this.eventData = eventData;
        onUpdateSelect?.Invoke(gameObject, eventData);
    }
    public void OnDeselect(BaseEventData eventData)
    {
        this.eventData = eventData;
        onDeSelect?.Invoke(gameObject, eventData);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        this.eventData = eventData;
        onDragBegin?.Invoke(gameObject, eventData.position.x, eventData.position.y, eventData.delta.x, eventData.delta.y);
    }
    public void OnDrag(PointerEventData eventData)
    {
        this.eventData = eventData;
        onDrag?.Invoke(gameObject, eventData.position.x, eventData.position.y, eventData.delta.x, eventData.delta.y);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        this.eventData = eventData;
        onDragEnd?.Invoke(gameObject, eventData.position.x, eventData.position.y, eventData.delta.x, eventData.delta.y);
    }
    public void OnDrop(PointerEventData eventData)
    {
        this.eventData = eventData;
        onDrop?.Invoke(gameObject, eventData.position.x, eventData.position.y, eventData.delta.x, eventData.delta.y);
    }
    public void OnScroll(PointerEventData eventData)
    {
        this.eventData = eventData;
        onScroll?.Invoke(gameObject, eventData.position.x, eventData.position.y, eventData.delta.x, eventData.delta.y);
    }
    public void OnMove(AxisEventData eventData)
    {
        this.eventData = eventData;
        onMove?.Invoke(gameObject, eventData.moveVector.x, eventData.moveVector.y,(int)eventData.moveDir);
    }

    public void OnLongPressOnlyOne()
    {
        if (Input.touchCount >= 2) return;
        onLongPressOnlyOne?.Invoke(gameObject);
    }

    private BaseEventData eventData;
    public BaseEventData GetEventData()
    {
        return eventData;
    }

}

