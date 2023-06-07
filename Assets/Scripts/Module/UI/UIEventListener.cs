using UnityEngine;
using UnityEngine.EventSystems;


///<summary>
///UI事件监听器：管理所有UI事件，提供所有UI事件参数类
///              附加到需要交互的UI元素上。用于监听用户操作
///</summary>
public class UIEventListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler,
    IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IInitializePotentialDragHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IScrollHandler, IUpdateSelectedHandler,
    ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler, ICancelHandler, IEventSystemHandler
{
    //定义委托数据类型 
    public delegate void PointerEventHandle(PointerEventData eventData);
    public delegate void BaseEventHandle(BaseEventData eventData);
    public delegate void AxisEventHandle(AxisEventData eventData);
    //声明委托
    public event PointerEventHandle PointerClick;
    public event PointerEventHandle PointerDown;
    public event PointerEventHandle PointerUp;
    public event PointerEventHandle PointerEnter;
    public event PointerEventHandle PointerExit;
    public event PointerEventHandle InitializePotentialDrag;
    public event PointerEventHandle BeginDrag;
    public event PointerEventHandle Drag;
    public event PointerEventHandle EndDrag;
    public event PointerEventHandle Drop;
    public event PointerEventHandle Scroll;
    public event BaseEventHandle UpdateSelected;
    public event BaseEventHandle Select;
    public event BaseEventHandle Deselect;
    public event BaseEventHandle Submit;
    public event BaseEventHandle Cancel;
    public event AxisEventHandle Move;

    /// <summary>
    /// 通过变换组件获取事件监听器
    /// </summary>
    /// <param name="transform">变换组件</param>
    /// <returns>返回事件监听器</returns>
    public static UIEventListener GetListener(Transform transform)
    {
        UIEventListener uiEventListener = transform.GetComponent<UIEventListener>();
        if (uiEventListener == null)
            uiEventListener = transform.gameObject.AddComponent<UIEventListener>();
        return uiEventListener;
    }

    //实现接口
    public void OnPointerClick(PointerEventData eventData)
    {
        PointerClick?.Invoke(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PointerDown?.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PointerUp?.Invoke(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnter?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExit?.Invoke(eventData);
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        InitializePotentialDrag?.Invoke(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDrag?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Drag?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDrag?.Invoke(eventData);
    }

    public void OnDrop(PointerEventData eventData)
    {
        Drop?.Invoke(eventData);
    }

    public void OnScroll(PointerEventData eventData)
    {
        Scroll?.Invoke(eventData);
    }

    public void OnUpdateSelected(BaseEventData eventData)
    {
        UpdateSelected?.Invoke(eventData);
    }

    public void OnSelect(BaseEventData eventData)
    {
        Select?.Invoke(eventData);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        Deselect?.Invoke(eventData);
    }

    public void OnMove(AxisEventData eventData)
    {
        Move?.Invoke(eventData);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        Submit?.Invoke(eventData);
    }

    public void OnCancel(BaseEventData eventData)
    {
        Cancel?.Invoke(eventData);
    }
}
