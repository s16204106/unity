using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviourPun
{
    //状态
    public Targetstate CurrentState = Targetstate.idle;
    //玩家
    private NavMeshAgent agent;
    //距离
    private float distance;
    //当前目标
    private GameObject enemy;
    //目标群
    private GameObject[] enemys;
    
    //攻击范围
    public float attackRange;

    //发现范围
    public float findRange;
    //攻击力
    public int damage;
    //射击点
    public GameObject shootPoint;
    //子弹
    public GameObject projectile;

    public TargetState targetState;

    private Animator animator;
    private GameObject magicMissile;
    [HideInInspector]
    public GamePlayController gamePlayController;
    [HideInInspector]
    public bool isAttack= false;

    // Start is called before the first frame update
    void Start()
    {
        //shootPoint = this.transform.Find("shootpoint").gameObject;
        enemys = new GameObject[100];
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        gamePlayController = GameObject.Find("Scripts").GetComponent<GamePlayController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine)
        {
            //进入结算阶段，销毁处于战场的champion
            if (gamePlayController.currentGameStage == GameStage.End)
                PhotonNetwork.Destroy(this.gameObject);

            enemys = GameObject.FindGameObjectsWithTag("champion");
            enemy = findMin(enemys);

            if (enemy != null)
            {
                distance = Vector3.Distance(enemy.transform.position, transform.position);
                switch (CurrentState)
                {
                    case Targetstate.idle:
                        if (distance >attackRange && distance <= findRange)
                        {
                            CurrentState = Targetstate.run;
                        }
                        else if (distance < attackRange)
                        {
                            agent.isStopped = false;
                            CurrentState = Targetstate.attack;
                        }
                        animator.SetInteger("Run", -1);
                        agent.isStopped = true;
                        break;
                    case Targetstate.run:
                        if (distance > findRange)
                        {
                            CurrentState = Targetstate.idle;
                        }
                        else if (distance <= attackRange)
                        {
                            agent.isStopped = false;
                            CurrentState = Targetstate.attack;
                        }
                        animator.SetInteger("Run", 1);
                        agent.isStopped = false;
                        agent.transform.LookAt(enemy.transform.position);
                        agent.SetDestination(enemy.transform.position);
                        break;
                    case Targetstate.attack:
                        if (distance > attackRange && distance <= findRange)
                        {
                            CurrentState = Targetstate.run;
                        }
                        this.transform.LookAt(enemy.transform.position);
                        if (this != null && enemy != null && distance < attackRange)
                        {
                            animator.SetInteger("Run", -1);
                            animator.SetTrigger("Attack");
                            agent.isStopped = true;
                        }
                        break;
                    default:
                        animator.SetFloat("Speed", -1);
                        agent.isStopped = true;
                        break;
                }
            }
            else
            {
                animator.SetInteger("Run", -1);
                agent.isStopped = true;
            }

        }
    }
    //寻找最近目标
    public GameObject GetTarget(GameObject[] enemys)
    {

        if (enemys == null)
        {
            return null;
        }
        quickSort(enemys, 0, enemys.Length - 1);
        int i = 0;
        while(true)
        {
            bool flag = enemys[i].GetComponent<PhotonView>().IsMine;
            if (flag && i+1 < enemys.Length)
                i++;
            else if (!flag)
                return enemys[i];
            else
                return null;
        }
       
    }
    
    public GameObject GetCurrentTarget()
    {
        return enemy;
    }
    public void attack()
    {
        
        if (enemy != null)
        {
            shoot(enemy.transform.position,damage);
            //避免其他客户端子弹造成伤害
            photonView.RPC("shoot", RpcTarget.Others, enemy.transform.position, 0);
        }
    }


    [PunRPC]
    public void shoot(Vector3 enemy ,int damage)
    {
        magicMissile = Instantiate(this.projectile, shootPoint.transform.position,this.transform.rotation);
        magicMissile.GetComponent<projectile>().Triggershoot(magicMissile, damage, shootPoint.transform.position, enemy);

    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        throw new System.NotImplementedException();
    }
    /// <summary>
    /// 查找
    /// </summary>
    /// <param name="arr"></param>
    /// <param name="low"></param>
    /// <param name="high"></param>
    public GameObject findMin(GameObject[] arr)
    {
        int index = -1 ;
        for(int i=0;i<arr.Length;i++)
        {
            if (!arr[i].GetComponent<PhotonView>().IsMine)
            {
                if (index == -1)
                    index = i;
                if (Vector3.Distance(arr[i].transform.position, this.transform.position) < Vector3.Distance(arr[index].transform.position, this.transform.position))
                    index = i;
            }
        }
        if (index != -1)
            return arr[index];
        else
            return null;
    }
    /// <summary>
    /// 快速排序
    /// </summary>
    /// <param name="arr"></param>
    /// <param name="low"></param>
    /// <param name="high"></param>
    public void quickSort(GameObject[] arr, int low, int high)
    {

        if (low < high)
        {
            // 找寻基准数据的正确索引
            int index = getIndex(arr, low, high);

            // 进行迭代对index之前和之后的数组进行相同的操作使整个数组变成有序
            quickSort(arr, 0, index - 1);
            quickSort(arr, index + 1, high);
        }
        
    }

    public int getIndex(GameObject[] arr, int low, int high)
    {
        // 基准数据
        GameObject tmp = arr[low];
        while (low < high)
        {
            // 当队尾的元素大于等于基准数据时,向前挪动high指针
            while (low < high && Vector3.Distance(arr[high].transform.position , transform.position) >= Vector3.Distance(tmp.transform.position, transform.position))
            {
                high--;
            }
            // 如果队尾元素小于tmp了,需要将其赋值给low
            arr[low] = arr[high];
            // 当队首元素小于等于tmp时,向前挪动low指针
            while (low < high && Vector3.Distance(arr[low].transform.position, transform.position) <= Vector3.Distance(tmp.transform.position, transform.position))
            {
                low++;
            }
            // 当队首元素大于tmp时,需要将其赋值给high
            arr[high] = arr[low];

        }
        // 跳出循环时low和high相等,此时的low或high就是tmp的正确索引位置
        // 由原理部分可以很清楚的知道low位置的值并不是tmp,所以需要将tmp赋值给arr[low]
        arr[low] = tmp;
        return low; // 返回tmp的正确位置
    }

}
