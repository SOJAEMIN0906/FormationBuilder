using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MemberInfo : MonoBehaviour
{
    Member Member;

    [SerializeField]
    int serialIndex;

    [SerializeField]
    GameObject memberSelectingPanel;

    Image memberSelectingPanelImg;

    Color color;

    [SerializeField]
    TMP_Text modifyTxt;
    [SerializeField]
    TMP_InputField nameInput;
    [SerializeField]
    TMP_InputField numberInput;

    private void Awake()
    {
        memberSelectingPanelImg = memberSelectingPanel.GetComponent<Image>();
        color = memberSelectingPanelImg.color;
    }

    public void Set(Member member, int serialIndex)
    {
        Member = member;
        numberInput.text = member.number;
        nameInput.text = member.name;
        this.serialIndex = serialIndex;
    }

    public void ChangeIndex(int serialIndex)
    {
        this.serialIndex = serialIndex;
    }

    public void Modify()
    {
        if (memberSelectingPanel.activeSelf) //수정 시작
        {
            modifyTxt.text = "완료";
            memberSelectingPanel.SetActive(false);
        }
        else //수정 완료
        {
            modifyTxt.text = "수정";
            Member.name = nameInput.text;
            Member.number = numberInput.text;

            Member = MainHome.instance.ModifyMemberInfo(Member);
            nameInput.text = Member.name;
            numberInput.text = Member.number;
            memberSelectingPanel.SetActive(true);
        }
    }

    public void Selected()
    {
        MainHome.instance.MemberInfoSelected(this);
    }

    public void Add()
    {
        MainHome.instance.AddInableTeam(Member, serialIndex);
    }

    public void Out()
    {
        MainHome.instance.OutInableTeam(Member, serialIndex);
    }

    public Member GetMember()
    {
        return Member;
    }
}
