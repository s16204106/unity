using UnityEngine;
using System.Collections;
using Photon.Pun;

public class projectile : MonoBehaviourPun
{

    public GameObject hitEffect;

    private int damage;

    public void Triggershoot(GameObject magicMissile, int damage, Vector3 origin, Vector3 victim)
    {
        this.damage = damage;
        StartCoroutine(lerpyLoop(magicMissile, origin, victim));
    }

    public IEnumerator lerpyLoop(GameObject projectileInstance, Vector3 begin, Vector3 end)
    {
        Vector3 origin = new Vector3(0, 0, 0);
        Vector3 victim = new Vector3(0, 0, 0);
        if (begin != null && end != null)
        {
            origin = begin;
            victim = end;
        }
        float distance = Vector3.Distance(victim, this.transform.position);
        float progress = 0;
        float timeScale = 13f / distance;

        // lerp ze missiles!
        while (progress < 0.9)
        {
            if (end == null)
            {
                Destroy(this.gameObject);
                yield return null;
            }
            else if (projectileInstance)
            {
                progress += timeScale * Time.deltaTime;
                float ypos = (progress - Mathf.Pow(progress, 2)) * 12;
                float ypos_b = ((progress + 0.1f) - Mathf.Pow((progress + 0.1f), 2)) * 12;
                projectileInstance.transform.position = Vector3.Lerp(origin, victim, progress) + new Vector3(0, ypos, 0);
                /*if (progress < 0.9f)
                {
                    projectileInstance.transform.LookAt(Vector3.Lerp(origin, victim, progress + 0.1f) + new Vector3(0, ypos_b, 0));
                }*/
                yield return null;
            }
        }
        Destroy(projectileInstance);
        if (hitEffect)
            Instantiate(hitEffect, victim, transform.rotation);

        yield return null;
    }

    public void clearProjectiles(GameObject magicMissile)
    {

        if (magicMissile)
            Destroy(magicMissile);

    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PhotonView>().IsMine)
        {
            return;
        }
        else
            other.GetComponent<TargetState>().photonView.RPC("TakeDamage", RpcTarget.All, damage);
    }
}
