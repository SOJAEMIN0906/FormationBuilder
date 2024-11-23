[System.Serializable]
public class MemberList
{
    public System.Collections.Generic.List<Member> memberList;
}

[System.Serializable]
public struct Member
{
    public Member(int serialNum, string name, string number)
    {
        this.serialNum = serialNum;
        this.name = name;
        this.number = number;
    }
    public int serialNum;
    public string name;
    public string number;
}

[System.Serializable]
public class InMemberList
{
    public System.Collections.Generic.List<int> inMembers;
}

public enum EFormation
{
    F_442,
    F_433,
    F_532
}

public enum EUniform
{
    Field,
    Keeper
}

[System.Serializable]
public class FormationInfos
{
    public System.Collections.Generic.List<FormationInfo> formations;
}

[System.Serializable]
public class FormationInfo
{
    public System.Collections.Generic.List<UnityEngine.Vector3> positions;
}