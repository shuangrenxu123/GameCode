using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BagPanel : WindowRoot
{

    public override void Start()
    {
        WindowsManager.Instance.DisableWindow<BagPanel>();
        GameObject slot = GetUIGameObject("slot");
        int childCount = slot.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var go = GetUIEvnetListener(slot.transform.GetChild(i).name);
            go.PointerEnter += OnPointEnter;
            go.PointerExit += OnPointExit;
            go.PointerClick += OnPointClick;
        }
    }

    private void OnPointClick(PointerEventData eventData)
    {
        Debug.Log(eventData.pointerEnter.name);
    }

    private void OnPointExit(PointerEventData eventData)
    {
        eventData.pointerEnter.GetComponent<Image>().color = Color.white;
    }

    private void OnPointEnter(PointerEventData eventData)
    {
        eventData.pointerEnter.GetComponent<Image>().color = Color.red;
    }
    public override void Update()
    {

    }

}
