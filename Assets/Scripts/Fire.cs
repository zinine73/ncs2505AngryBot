using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Fire : MonoBehaviour
{
    public Transform firePos;
    public GameObject bulletPrefab;
    ParticleSystem muzzleFlash;
    PhotonView pv;
    bool isMouseClick => Input.GetMouseButtonDown(0);
    void Start()
    {
        pv = GetComponent<PhotonView>();
        muzzleFlash = firePos.Find("MuzzleFlash").GetComponent<ParticleSystem>();
    }
    void Update()
    {
        if (pv.IsMine && isMouseClick)
        {
            FireBullet(pv.Owner.ActorNumber);
            pv.RPC("FireBullet", RpcTarget.Others, pv.Owner.ActorNumber);
        }
    }
    [PunRPC]
    void FireBullet(int actorNo)
    {
        if (!muzzleFlash.isPlaying) muzzleFlash.Play(true);
        GameObject bullet = Instantiate(bulletPrefab,
                                        firePos.position,
                                        firePos.rotation);
        bullet.GetComponent<RPCBullet>().actorNumber = actorNo;
    }
}
