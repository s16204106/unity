using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button mBtn;
    public GameObject pane;
    public Camera cam;
    private Vector3 Pos;
    private float X,Y;

    // Start is called before the first frame update
    void Start()
    {
        mBtn.gameObject.SetActive(false);
        pane.gameObject.SetActive(false);
        mBtn.onClick.AddListener(CreateActorButton);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {           
            if (mBtn.gameObject.activeSelf)
            {
                //检测是否点击了UI上的物件，返会bool值
                if (EventSystem.current.IsPointerOverGameObject() == false)
                {
                    //关闭按钮
                    mBtn.gameObject.SetActive(false);
                    pane.gameObject.SetActive(false);
                }
            }
            else
            {
                //显示建造按钮，位置为鼠标点击位置
                mBtn.gameObject.SetActive(true);
                Pos = Input.mousePosition;
                mBtn.transform.localPosition = new Vector3(Pos.x - Screen.width / 2f, Pos.y - Screen.height / 2f,Pos.z);
            }
        }
    }
    //获取鼠标点击的对应地面坐标
    public Vector3 getPosition()
    {
       
        Pos = mBtn.transform.localPosition;
        RectTransform rect = GetComponent<RectTransform>();
        Pos.x /= rect.rect.width;
        Pos.y /= rect.rect.height;
        Pos.x += 0.5f;
        Pos.y += 0.5f;
        Ray ray = cam.ViewportPointToRay(Pos);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (hits[0].collider.name == "Cube")
        {
            return hits[0].point;
        }
        else
        {
            Debug.LogError("position is zero");
            return new Vector3(0, 0, 0);
        }
 
    }
    private void CreateActorButton()
    {
        //显示创建怪物按钮，并设置坐标。
        pane.gameObject.SetActive(true);
        pane.transform.localPosition = new Vector3(Pos.x - Screen.width / 2f + 130, Pos.y - Screen.height / 2f, Pos.z);
    }
}
