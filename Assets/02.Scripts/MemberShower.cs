using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MemberShower : MonoBehaviour
{
    [SerializeField]
    TMP_Text memberNameTxt;
    [SerializeField]
    TMP_Text memberNumberTxt;

    [SerializeField]
    Image UniformImg;

    RectTransform rectTrans;

    Member member;

    Vector3 pointerDownPosition;

    bool isDragging = false;

    int serialIndex;

    private void Awake()
    {
        rectTrans = GetComponent<RectTransform>();

        memberNameTxt.text = "";
        memberNumberTxt.text = "";

        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entryPointerDown = new EventTrigger.Entry();
        entryPointerDown.eventID = EventTriggerType.PointerDown;
        entryPointerDown.callback.AddListener((data) => { OnPointerDown((PointerEventData)data); });
        trigger.triggers.Add(entryPointerDown);

        EventTrigger.Entry entryPointerBeginDrag = new EventTrigger.Entry();
        entryPointerBeginDrag.eventID = EventTriggerType.BeginDrag;
        entryPointerBeginDrag.callback.AddListener((data) => { BeginDrag((PointerEventData)data); });
        trigger.triggers.Add(entryPointerBeginDrag);

        EventTrigger.Entry entryPointerDrag = new EventTrigger.Entry();
        entryPointerDrag.eventID = EventTriggerType.Drag;
        entryPointerDrag.callback.AddListener((data) => { OnDrag((PointerEventData)data); });
        trigger.triggers.Add(entryPointerDrag);

        EventTrigger.Entry entryPointerEndDrag = new EventTrigger.Entry();
        entryPointerEndDrag.eventID = EventTriggerType.EndDrag;
        entryPointerEndDrag.callback.AddListener((data) => { EndDrag((PointerEventData)data); });
        trigger.triggers.Add(entryPointerEndDrag);
    }

    private void OnPointerDown(PointerEventData eventData)
    {
        pointerDownPosition = eventData.position;

        Vector3 localPoint;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTrans, eventData.position, eventData.pressEventCamera, out localPoint);
    }

    public void BeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        serialIndex = MainHome.instance.StartMemberMoving(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        pointerDownPosition = eventData.position;

        Vector3 localPoint;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTrans, eventData.position, eventData.pressEventCamera, out localPoint);

        rectTrans.position = localPoint;

        MainHome.instance.MemberObjMoving(serialIndex);
    }

    public void EndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void SetImg(Sprite sprite)
    {
        UniformImg.sprite = sprite;
    }

    public Member GetMember()
    {
        return member;
    }

    public void OpenMemebers()
    {
        if (!isDragging)
            MainHome.instance.OpenMemberSelecter(this);
    }

    public void SetMember(Member member)
    {
        this.member = member;
        memberNameTxt.text = this.member.name;
        memberNumberTxt.text = this.member.number.ToString();
    }

    public void ClearMember()
    {
        member = new Member();
        memberNameTxt.text = "";
        memberNumberTxt.text = "";
    }
}
