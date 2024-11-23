using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MemberSelectButton : MonoBehaviour
{
    Member member;
    [SerializeField]
    TMP_Text nameTxt;
    [SerializeField]
    TMP_Text numberTxt;

    public void Set(Member member)
    {
        this.member = member;
        nameTxt.text = member.name;
        numberTxt.text = member.number.ToString();
    }

    public void Selected()
    {
        MainHome.instance.MemberSelected(member);
    }
}
