using Photon.Pun;
using UnityEngine;

public enum Targetstate
{
    idle,
    run,
    attack
}
public class TargetState : MonoBehaviourPun
{
    //预制体
    
    public GameObject prefab;

    //生命值
    public int Health;
    [HideInInspector]
    public int currentHealth;


    //数量
    public int number;

    public AI ai;
    [HideInInspector]
    public GamePlayController gamePlayController;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = Health;
        gamePlayController = GameObject.Find("Scripts").GetComponent<GamePlayController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            if (currentHealth <= 0)
            {
                if (gameObject)
                {
                    gamePlayController.currentFighter--;
                    PhotonNetwork.Destroy(gameObject);
                }

            }
        }
    }
    [PunRPC]
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }

    public float GetHealth()
    {
        return (float)currentHealth / (float)Health;
    }
    /*
    private void OnTriggerEnter(Collider other)
    {
        projectile projectile;
        if (other.GetComponent<PhotonView>().IsMine)
        {
             return;
        }
        else if ((projectile = other.GetComponent<projectile>()) != null)
        {
            TakeDamage(projectile.damage);
        }
    }*/

    #region IPunObservable implementation
    /*
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(this.currentHealth);
        }
        else
        {
             //Network player, receive data
            this.currentHealth = (int)stream.ReceiveNext();
        }
    }*/

    #endregion

    [PunRPC]
    public void setTag()
    {
        this.gameObject.tag = "champion";
    }

}
