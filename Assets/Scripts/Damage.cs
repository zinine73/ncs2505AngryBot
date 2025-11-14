using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Player = Photon.Realtime.Player;

public class Damage : MonoBehaviourPunCallbacks
{
    Renderer[] renderers;
    int initHp = 100;
    public int currHp = 100;
    Animator anim;
    CharacterController cc;
    readonly int hashDie = Animator.StringToHash("Die");
    readonly int hashRespawn = Animator.StringToHash("Respawn");
    NewGameManager gameManager;
    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        anim = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        currHp = initHp;
        gameManager = GameObject.Find("NewGameManager").GetComponent<NewGameManager>();
    }
    void OnCollisionEnter(Collision collision)
    {
        if (currHp > 0 && collision.collider.CompareTag("BULLET"))
        {
            currHp -= 20;
            if (currHp <= 0)
            {
                if (photonView.IsMine)
                {
                    var actorNo = collision.collider.GetComponent<RPCBullet>().actorNumber;
                    Player lastShootPlayer = PhotonNetwork.CurrentRoom.GetPlayer(actorNo);
                    string msg = string.Format("\n<color=#00ff00>{0}</color> is killed by <color=#ff0000>{1}</color>",
                        photonView.Owner.NickName, lastShootPlayer);
                    photonView.RPC("KillMessage", RpcTarget.AllBufferedViaServer, msg);
                }
                StartCoroutine(PlayerDie());
            }
        }
    }
    [PunRPC]
    void KillMessage(string msg)
    {
        gameManager.msgList.text += msg;
    }
    IEnumerator PlayerDie()
    {
        cc.enabled = false;
        anim.SetBool(hashRespawn, false);
        anim.SetTrigger(hashDie);
        yield return new WaitForSeconds(3.0f);
        
        anim.SetBool(hashRespawn, true);
        SetPlayerVisible(false);
        yield return new WaitForSeconds(1.5f);

        Transform[] points = GameObject.Find("SpawnPointGroup")
            .GetComponentsInChildren<Transform>();
        int idx = Random.Range(1, points.Length);
        transform.position = points[idx].position;
        currHp = initHp;
        SetPlayerVisible(true);
        cc.enabled = true;
    }
    void SetPlayerVisible(bool isVisible)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = isVisible;
        }
    }
}
