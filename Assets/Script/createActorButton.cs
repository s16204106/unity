using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class createActorButton : MonoBehaviour
{
    public UIManager uIManager;
    public Button ActorType;
    private GameObject res = null;

    // Start is called before the first frame update
    void Start()
    {
        ActorType.onClick.AddListener(CreateActor);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void CreateActor()
    {
        //工厂列表
        switch (ActorType.name)
        {
            case "法师": res = Resources.Load<GameObject>("human_cleric_Rig"); break;
            case "弓箭手": res = Resources.Load<GameObject>("human_archer_Rig"); break;
        }
        int x=0,z=0;
        for(int i=0;i<1;i++)
        {
            //网络实例化
            GameObject go = PhotonNetwork.Instantiate(res.name, uIManager.getPosition() + new Vector3(x, 5f, z), Quaternion.identity);
            go.SetActive(true);
        }
    }
}
