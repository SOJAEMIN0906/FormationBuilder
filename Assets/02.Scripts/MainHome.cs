using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainHome : MonoBehaviour
{
    public static MainHome instance;

    MemberList MemberList;
    List<Member> InableMembers = new List<Member>();
    List<Member> inMembers = new List<Member>();
    InMemberList InMemberList;

    public Sprite DefaultUniformImage;
    public Sprite DefaultKeeperUniformImage;
    public Sprite CustomUniformSprite;
    public Sprite CustomKeeperUniformSprite;

    FormationInfos FormationInfo;

    [SerializeField]
    GameObject TeamMemberSettingObj;
    [SerializeField]
    GameObject MemberSelectingObj;
    [SerializeField]
    GameObject AddMemberObj;
    public GameObject MemberSelectPrefab;
    public GameObject MemberPrefab;
    public GameObject InMemberInfoPrefab;
    public GameObject OutMemberInfoPrefab;

    [SerializeField]
    RawImage Background;
    RectTransform BackgroundRectTrans;

    [SerializeField]
    Transform MemberSelectingContents;
    [SerializeField]
    Transform MemberSettingOutMemberContents;
    [SerializeField]
    Transform MemberSettingInMemberContents;
    [SerializeField]
    Transform MembersTrans;
    Transform[] MemberShowersTrans;

    [SerializeField]
    TMP_InputField AddMemberNumberInput;
    [SerializeField]
    TMP_InputField AddMemberNameInput;

    [SerializeField]
    TMP_Dropdown formationDrop;

    MemberShower currentMemberShower;
    MemberInfo currentMemberInfo;

    int currentMemberShowerIndex;

    Vector2 pointerDownPosition;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            Screen.SetResolution(1080, 1920, true);

            MemberList = LoadMemberData();

            FormationInfo = LoadFormationData();

            InMemberList = LoadInMemberList();

            foreach (Member m in MemberList.memberList)
            {
                if (InMemberList.inMembers.Contains(m.serialNum))
                {
                    InableMembers.Add(m);
                }
            }

            BackgroundRectTrans = Background.GetComponent<RectTransform>();

            MemberShowersTrans = new Transform[MembersTrans.childCount];
            for (int i = 0; i < MembersTrans.childCount; i++)
            {
                MemberShowersTrans[i] = MembersTrans.GetChild(i);
            }

            for (int i = 0; i < MembersTrans.childCount; i++)
            {
                MemberShowersTrans[i].gameObject.SetActive(true);

                MemberShowersTrans[i].position = FormationInfo.formations[0].positions[i];
            }

            currentMemberShowerIndex = MembersTrans.childCount;

            CustomKeeperUniformSprite = LoadCustomUniform(EUniform.Keeper);
            if (CustomKeeperUniformSprite != null)
            {
                MemberShowersTrans[0].GetComponent<MemberShower>().SetImg(CustomKeeperUniformSprite);
            }
            else
            {
                MemberShowersTrans[0].GetComponent<MemberShower>().SetImg(DefaultKeeperUniformImage);
            }

            CustomUniformSprite = LoadCustomUniform(EUniform.Field);
            if (CustomUniformSprite != null)
            {
                for (int i = 1; i < MemberShowersTrans.Length; i++)
                {
                    MemberShowersTrans[i].GetComponent<MemberShower>().SetImg(CustomUniformSprite);
                }
            }
            else
            {
                for (int i = 1; i < MemberShowersTrans.Length; i++)
                {
                    MemberShowersTrans[i].GetComponent<MemberShower>().SetImg(DefaultUniformImage);
                }
            }

            EventTrigger trigger = Background.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entryPointerDown = new EventTrigger.Entry();
            entryPointerDown.eventID = EventTriggerType.PointerDown;
            entryPointerDown.callback.AddListener((data) => { OnPointerDown((PointerEventData)data); });
            trigger.triggers.Add(entryPointerDown);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnPointerDown(PointerEventData eventData)
    {
        pointerDownPosition = eventData.position;

        Vector3 localPoint;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(BackgroundRectTrans, eventData.position, eventData.pressEventCamera, out localPoint);

        if (currentMemberShowerIndex < MembersTrans.childCount)
        {
            MemberShowersTrans[currentMemberShowerIndex].position = localPoint;
            MemberShowersTrans[currentMemberShowerIndex].gameObject.SetActive(true);
            FormationInfo.formations[0].positions[currentMemberShowerIndex] = MemberShowersTrans[currentMemberShowerIndex].position;
            SaveFormationData(FormationInfo);
            currentMemberShowerIndex++;
        }
    }

    public void FormationDropdownChange(int value)
    {
        if (value != 0)
        {
            for (int i = 0; i < MembersTrans.childCount; i++)
            {
                MemberShowersTrans[i].gameObject.SetActive(true);

                MemberShowersTrans[i].position = FormationInfo.formations[value].positions[i];
            }
            
            currentMemberShowerIndex = MembersTrans.childCount;
        }
    }

    
    public void OpenGallery(int uniformType)
    {
        EUniform eUniform = (EUniform)uniformType;
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize: 1024);
                if (texture == null)
                {
                    Debug.LogError("Failed to load texture from " + path);
                    return;
                }

                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

                switch (eUniform)
                {
                    case EUniform.Field:
                        CustomUniformSprite = sprite;
                        for (int i = 1; i < MemberShowersTrans.Length; i++)
                        {
                            MemberShowersTrans[i].GetComponent<MemberShower>().SetImg(CustomUniformSprite);
                        }
                        break;

                    case EUniform.Keeper:
                        CustomKeeperUniformSprite = sprite;
                        MemberShowersTrans[0].GetComponent<MemberShower>().SetImg(CustomKeeperUniformSprite);
                        break;
                }

                string destinationPath = Path.Combine(Application.persistentDataPath, eUniform.ToString() + ".png");
                File.Copy(path, destinationPath, true);
                Debug.Log("Image copied to: " + destinationPath);
            }
        }, "Select an Image", "image/*");
        

        if (permission == NativeGallery.Permission.Denied)
        {
            Debug.LogWarning("갤러리에 접근 권한이 거부되었습니다.");
        }
        else if (permission == NativeGallery.Permission.Granted)
        {
            Debug.Log("갤러리 접근 권한이 허용되었습니다.");
        }
    }

    public void SaveMemberData(MemberList memberList)
    {
        string json = JsonUtility.ToJson(memberList);
        string path = Application.persistentDataPath + "/MemberList.json";
        File.WriteAllText(path, json);
        Debug.Log("Data saved to " + path);
    }

    public MemberList LoadMemberData()
    {
        string path = Application.persistentDataPath + "/MemberList.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            MemberList data = JsonUtility.FromJson<MemberList>(json);
            Debug.Log("Data loaded from " + path);
            return data;
        }
        else
        {
            MemberList data = new MemberList();
            data.memberList = new List<Member>();

            SaveMemberData(data);
            Debug.Log("Save file not found at " + path);
            return data;
        }
    }

    public Member ModifyMemberInfo(Member member)
    {
        currentMemberShower = null;
        currentMemberInfo = null;
        for (int i = 0; i < MemberList.memberList.Count; i++)
        {
            if (MemberList.memberList[i].serialNum == member.serialNum)
            {
                MemberList.memberList.RemoveAt(i);
                MemberList.memberList.Add(member);
                break;
            }
        }

        Member rslMember = OptimizeData(member.serialNum);
        SaveMemberData(MemberList);

        return rslMember;
    }

    public void SaveFormationData(FormationInfos formationData)
    {
        string path = Application.persistentDataPath + "/FormaitionInfo.json";
        string json = JsonUtility.ToJson(formationData);
        File.WriteAllText(path, json);
    }

    public FormationInfos LoadFormationData()
    {
        FormationInfos formationData;
        string path = Application.persistentDataPath + "/FormaitionInfo.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            formationData = JsonUtility.FromJson<FormationInfos>(json);
        }
        else
        {
            formationData = new FormationInfos();
            formationData.formations = new List<FormationInfo>();

            FormationInfo formationInfo = new FormationInfo();
            formationInfo.positions = new List<Vector3>()
            {
                new Vector3(0.00f, 0.00f, -9.00f),
                new Vector3(0.00f, 0.00f, -9.00f),
                new Vector3(0.00f, 0.00f, -9.00f),
                new Vector3(0.00f, 0.00f, -9.00f),
                new Vector3(0.00f, 0.00f, -9.00f),
                new Vector3(0.00f, 0.00f, -9.00f),
                new Vector3(0.00f, 0.00f, -9.00f),
                new Vector3(0.00f, 0.00f, -9.00f),
                new Vector3(0.00f, 0.00f, -9.00f),
                new Vector3(0.00f, 0.00f, -9.00f),
                new Vector3(0.00f, 0.00f, -9.00f)
            };
            formationData.formations.Add(formationInfo);
            
            
            formationInfo = new FormationInfo();
            formationInfo.positions = new List<Vector3>()
            {
                new Vector3(0.00f, -4.40f, -9.00f),
                new Vector3(-1.90f, -2.10f, -9.00f),
                new Vector3(-0.70f, -2.60f, -9.00f),
                new Vector3(0.70f, -2.60f, -9.00f),
                new Vector3(1.90f, -2.10f, -9.00f),
                new Vector3(-1.90f, 0.00f, -9.00f),
                new Vector3(-0.70f, -0.20f, -9.00f),
                new Vector3(0.70f, -0.20f, -9.00f),
                new Vector3(1.90f, 0.00f, -9.00f),
                new Vector3(-1.00f, 2.00f, -9.00f),
                new Vector3(1.00f, 2.00f, -9.00f)
            };
            formationData.formations.Add(formationInfo);


            formationInfo = new FormationInfo();
            formationInfo.positions = new List<Vector3>()
            {
                new Vector3(0.00f, -4.40f, -9.00f),
                new Vector3(-1.90f, -2.10f, -9.00f),
                new Vector3(-0.70f, -2.60f, -9.00f),
                new Vector3(0.70f, -2.60f, -9.00f),
                new Vector3(1.90f, -2.10f, -9.00f),
                new Vector3(-1.30f, -0.20f, -9.00f),
                new Vector3(0.00f, -0.20f, -9.00f),
                new Vector3(1.30f, -0.20f, -9.00f),
                new Vector3(-1.30f, 2.10f, -9.00f),
                new Vector3(0.00f, 2.10f, -9.00f),
                new Vector3(1.30f, 2.10f, -9.00f)
            };
            formationData.formations.Add(formationInfo);


            formationInfo = new FormationInfo();
            formationInfo.positions = new List<Vector3>()
            {
                new Vector3(0.00f, -4.40f, -9.00f),
                new Vector3(-2.20f, -2.10f, -9.00f),
                new Vector3(-1.30f, -2.60f, -9.00f),
                new Vector3(0.00f, -2.10f, -9.00f),
                new Vector3(1.30f, -2.60f, -9.00f),
                new Vector3(2.20f, -2.10f, -9.00f),
                new Vector3(-1.30f, -0.20f, -9.00f),
                new Vector3(0.00f, -0.20f, -9.00f),
                new Vector3(1.30f, -0.20f, -9.00f),
                new Vector3(-1.00f, 2.10f, -9.00f),
                new Vector3(1.00f, 2.10f, -9.00f)
            };
            formationData.formations.Add(formationInfo);

            SaveFormationData(formationData);
        }

        return formationData;
    }

    public void SaveInMemberList(InMemberList inMemberList)
    {
        string path = Application.persistentDataPath + "/InMemberList.json";
        string json = JsonUtility.ToJson(inMemberList);
        File.WriteAllText(path, json);
    }

    public InMemberList LoadInMemberList()
    {
        InMemberList inMemberList;
        string path = Application.persistentDataPath + "/InMemberList.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            inMemberList = JsonUtility.FromJson<InMemberList>(json);
        }
        else
        {
            inMemberList = new InMemberList();
            inMemberList.inMembers = new List<int>();

            SaveInMemberList(inMemberList);
        }

        return inMemberList;
    }

    public Sprite LoadCustomUniform(EUniform eUniform)
    {
        string imagePath = Path.Combine(Application.persistentDataPath, eUniform.ToString() + ".png");

        if (File.Exists(imagePath))
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);

            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(imageBytes))
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                Debug.Log("Image loaded from: " + imagePath);
                
                return sprite;
            }
            else
            {
                Debug.LogError("Failed to convert texture from " + imagePath);
            }
        }
        else
        {
            Debug.LogWarning("Image not found at: " + imagePath);
        }

        return null;
    }

    /// <summary>
    /// 선수 명단 창 열기
    /// </summary>
    public void OpenTeamMemberSetting()
    {
        currentMemberShower = null;
        currentMemberInfo = null;
        TeamMemberSettingObj.SetActive(true);
        for (int i = MemberSettingInMemberContents.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(MemberSettingInMemberContents.GetChild(i).gameObject);
        }
        for (int i = MemberSettingOutMemberContents.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(MemberSettingOutMemberContents.GetChild(i).gameObject);
        }

        foreach (var m in MemberList.memberList)
        {
            if (InableMembers.Contains(m))
            {
                Instantiate(InMemberInfoPrefab, MemberSettingInMemberContents).GetComponent<MemberInfo>().Set(m, MemberSettingInMemberContents.childCount);
            }
            else
            {
                Instantiate(OutMemberInfoPrefab, MemberSettingOutMemberContents).GetComponent<MemberInfo>().Set(m, MemberSettingOutMemberContents.childCount);
            }
        }

        RectTransform contents = MemberSettingInMemberContents.GetComponent<RectTransform>();
        contents.offsetMin = new Vector2(contents.offsetMin.x, -MemberSettingInMemberContents.childCount * 150);

        contents = MemberSettingOutMemberContents.GetComponent<RectTransform>();
        contents.offsetMin = new Vector2(contents.offsetMin.x, -MemberSettingOutMemberContents.childCount * 150);
    }

    /// <summary>
    /// 선수 명단 창 닫기
    /// </summary>
    public void CloseTeamMemberSetting()
    {
        TeamMemberSettingObj.SetActive(false);
        currentMemberShower = null;
        currentMemberInfo = null;
    }

    public void AddInableTeam(Member member, int index)
    {
        InableMembers.Add(member);
        InMemberList.inMembers.Add(member.serialNum);
        SaveInMemberList(InMemberList);

        DestroyImmediate(MemberSettingOutMemberContents.GetChild(index - 1).gameObject);
        Instantiate(InMemberInfoPrefab, MemberSettingInMemberContents).GetComponent<MemberInfo>().Set(member, MemberSettingInMemberContents.childCount);
        
        for (int i = 0; i < MemberSettingOutMemberContents.childCount; i++)
        {
            MemberSettingOutMemberContents.GetChild(i).GetComponent<MemberInfo>().ChangeIndex(i + 1);
        }
        currentMemberShower = null;
        currentMemberInfo = null;
    }

    public void OutInableTeam(Member member, int index)
    {
        InableMembers.Remove(member);
        InMemberList.inMembers.Remove(member.serialNum);
        SaveInMemberList(InMemberList);

        DestroyImmediate(MemberSettingInMemberContents.GetChild(index - 1).gameObject);
        Instantiate(OutMemberInfoPrefab, MemberSettingOutMemberContents).GetComponent<MemberInfo>().Set(member, MemberSettingOutMemberContents.childCount);

        for (int i = 0; i < MemberSettingInMemberContents.childCount; i++)
        {
            MemberSettingInMemberContents.GetChild(i).GetComponent<MemberInfo>().ChangeIndex(i + 1);
        }
        currentMemberShower = null;
        currentMemberInfo = null;
    }

    public void OpenAddMember()
    {
        AddMemberNameInput.text = "Name";
        AddMemberNumberInput.text = "Number";
        AddMemberObj.SetActive(true);
        currentMemberShower = null;
        currentMemberInfo = null;
    }

    public void CloseAddMember()
    {
        AddMemberNameInput.text = "Name";
        AddMemberNumberInput.text = "Number";
        AddMemberObj.SetActive(false);
        currentMemberShower = null;
        currentMemberInfo = null;
    }

    public void MemberInfoSelected(MemberInfo memberInfo)
    {
        currentMemberInfo = memberInfo;
    }

    public void AddMember()
    {
        currentMemberShower = null;
        currentMemberInfo = null;
        Member member = new Member(MemberList.memberList.Count, AddMemberNameInput.text, AddMemberNumberInput.text);
        MemberList.memberList.Add(member);
        
        OptimizeData();
        SaveMemberData(MemberList);

        CloseAddMember();
        OpenTeamMemberSetting();
    }

    public void ClearInableMember()
    {
        currentMemberShower = null;
        currentMemberInfo = null;

        InableMembers.Clear();
        InMemberList.inMembers.Clear();
        SaveInMemberList(InMemberList);

        for (int i = MemberSettingInMemberContents.childCount - 1; i >= 0; i--)
        {
            Destroy(MemberSettingInMemberContents.GetChild(i).gameObject);
        }
        for (int i = MemberSettingOutMemberContents.childCount - 1; i >= 0; i--)
        {
            Destroy(MemberSettingOutMemberContents.GetChild(i).gameObject);
        }

        foreach (var m in MemberList.memberList)
        {
            Instantiate(OutMemberInfoPrefab, MemberSettingOutMemberContents).GetComponent<MemberInfo>().Set(m, MemberSettingOutMemberContents.childCount);
        }
    }

    public void RemoveMember()
    {
        if (currentMemberInfo != null)
        {
            for (int i = 0; i < MemberList.memberList.Count; i++)
            {
                if (MemberList.memberList[i].serialNum == currentMemberInfo.GetMember().serialNum)
                {
                    MemberList.memberList.RemoveAt(i);
                    break;
                }
            }

            OptimizeData();
            SaveMemberData(MemberList);
        }
        currentMemberShower = null;
        currentMemberInfo = null;
    }

    /// <summary>
    /// 선수를 선택하는 창을 호출할 때
    /// </summary>
    /// <param name="memberShower"></param>
    public void OpenMemberSelecter(MemberShower memberShower)
    {
        currentMemberShower = memberShower;
        MemberSelectingObj.SetActive(true);
        for (int i = MemberSelectingContents.childCount - 1; i >= 0; i--)
        {
            Destroy(MemberSelectingContents.GetChild(i).gameObject);
        }

        //선수를 이미 배치했을 때 해당 선수들을 제외한 나머지 선수들 명단을 출력
        foreach(var m in InableMembers)
        {
            if (!inMembers.Contains(m))
            {
                Instantiate(MemberSelectPrefab, MemberSelectingContents).GetComponent<MemberSelectButton>().Set(m);
            }
        }

        RectTransform contents = MemberSelectingContents.GetComponent<RectTransform>();
        contents.offsetMin = new Vector2(contents.offsetMin.x, -MemberSelectingContents.childCount * 150);
    }

    public void CloseMemberSelecter()
    {
        currentMemberShower = null;
        currentMemberInfo = null;
        MemberSelectingObj.SetActive(false);
    }

    public void ClearSeletedMember()
    {
        inMembers.Remove(currentMemberShower.GetMember());
        currentMemberShower.ClearMember();

        MemberSelectingObj.SetActive(false);
        currentMemberShower = null;
        currentMemberInfo = null;
    }

    public void ClearAllMembers()
    {
        formationDrop.value = 0;
        for (int i = 0; i < MemberShowersTrans.Length; i++)
        {
            MemberShowersTrans[i].GetComponent<MemberShower>().ClearMember();
            MemberShowersTrans[i].position = Vector3.zero;
            FormationInfo.formations[0].positions[i] = new Vector3(0, 0, -14);
            MemberShowersTrans[i].gameObject.SetActive(false);
        }
        SaveFormationData(FormationInfo);

        currentMemberShowerIndex = 0;

        inMembers.Clear();
        MemberSelectingObj.SetActive(false);
        currentMemberShower = null;
        currentMemberInfo = null;
    }

    public int StartMemberMoving(MemberShower memberShower)
    {
        for (int i = 0; i < MemberShowersTrans.Length; i++)
        {
            FormationInfo.formations[0].positions[i] = MemberShowersTrans[i].position;

            if (MemberShowersTrans[i].GetComponent<MemberShower>() == memberShower)
            {
                return i;
            }
        }

        return -1;
    }

    public void MemberObjMoving(int serialIndex)
    {
        formationDrop.value = 0;

        FormationInfo.formations[0].positions[serialIndex] = MemberShowersTrans[serialIndex].position;

        SaveFormationData(FormationInfo);

        currentMemberShower = null;
        currentMemberInfo = null;
    }

    public void MemberSelected(Member member)
    {
        if (currentMemberShower != null)
            currentMemberShower.SetMember(member);

        MemberSelectingObj.SetActive(false);
        currentMemberShower = null;
        currentMemberInfo = null;

        inMembers.Add(member);
    }

    public void OptimizeData()
    {
        MemberList list = new MemberList();
        list.memberList = new List<Member>();
        InMemberList inList = new InMemberList();
        inList.inMembers = new List<int>();
        for (int i = 0; i < MemberList.memberList.Count; i++)
        {
            Member member = MemberList.memberList[i];
            member.serialNum = i;
            list.memberList.Add(member);
            if (InMemberList.inMembers.Contains(MemberList.memberList[i].serialNum))
            {
                inList.inMembers.Add(i);
            }
        }
        MemberList = list;
        InMemberList = inList;
        SaveInMemberList(InMemberList);
        ClearAllMembers();
    }

    public Member OptimizeData(int serialNum)
    {
        MemberList list = new MemberList();
        list.memberList = new List<Member>();
        Member rsltMember = new Member();
        InMemberList inList = new InMemberList();
        inList.inMembers = new List<int>();
        for (int i = 0; i < MemberList.memberList.Count; i++)
        {
            Member member = MemberList.memberList[i];
            member.serialNum = i;
            list.memberList.Add(member);
            if (MemberList.memberList[i].serialNum == serialNum)
            {
                rsltMember = member;
            }
            if (InMemberList.inMembers.Contains(MemberList.memberList[i].serialNum))
            {
                inList.inMembers.Add(i);
            }
        }
        MemberList = list;
        InMemberList = inList;
        SaveInMemberList(InMemberList);
        ClearAllMembers();
        return rsltMember;
    }
}
