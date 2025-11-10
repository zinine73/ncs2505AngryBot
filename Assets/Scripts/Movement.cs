using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class Movement : MonoBehaviourPunCallbacks, IPunObservable
{
    // 컴포넌트 캐시 처리를 위한 변수
    CharacterController controller;
    new Transform transform;
    Animator animator;
    new Camera camera;
    // 가상의 plane에 raycasting하기 위한 변수
    Plane plane;
    Ray ray;
    Vector3 hitPoint;
    PhotonView pv;
    CinemachineVirtualCamera virtualCamera;
    // 이동 속도
    public float moveSpeed = 10.0f;
    Vector3 receivePos;
    Quaternion receiveRot;
    public float damping = 10.0f;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        camera = Camera.main;
        pv = GetComponent<PhotonView>();
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        // PhotonView가 자신일 경우 가상카메라 연결
        if (pv.IsMine)
        {
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;
        }
        // 가상의 바닥을 주인공의 위치를 기준으로 생성
        plane = new Plane(transform.up, transform.position);
    }

    void Update()
    {
        if (pv.IsMine)
        {
            Move();
            Turn();
        }
        else
        {
            // 수신된 좌표, 회전값로 보간한 이동, 회전 처리
            transform.position = Vector3.Lerp(
                transform.position,
                receivePos,
                Time.deltaTime * damping
            );
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                receiveRot,
                Time.deltaTime * damping
            );
        }
    }

    // 키보드 입력값 연결
    float h => Input.GetAxis("Horizontal");
    float v => Input.GetAxis("Vertical");

    // 이동 처리하는 함수
    void Move()
    {
        Vector3 cameraForward = camera.transform.forward;
        Vector3 cameraRight = camera.transform.right;
        cameraForward.y = 0.0f;
        cameraRight.y = 0.0f;
        // 이동할 방향 벡터 계산
        Vector3 moveDir = (cameraForward * v) + (cameraRight * h);
        moveDir.Set(moveDir.x, 0.0f, moveDir.z);
        // 주인공 캐릭터 이동 처리
        controller.SimpleMove(moveDir * moveSpeed);
        // 주인공 캐릭터 애니메이션 처리
        float forward = Vector3.Dot(moveDir, transform.forward);
        float strafe = Vector3.Dot(moveDir, transform.right);
        animator.SetFloat("Forward", forward);
        animator.SetFloat("Strafe", strafe);
    }

    // 회전 처리하는 함수
    void Turn()
    {
        // 마우스의 2차원 좌표값을 이용해 3차원 광선(레이)을 생성
        ray = camera.ScreenPointToRay(Input.mousePosition);
        float enter = 0.0f;
        // 가상의 바닥에 레이를 발사해 충돌지점 거리를 enter변수로 반환
        plane.Raycast(ray, out enter);
        // 가상의 바닥에 레이가 충돌한 좌표값 추출
        hitPoint = ray.GetPoint(enter);
        // 회전해야할 방향의 벡터를 계산
        Vector3 lookDir = hitPoint - transform.position;
        lookDir.y = 0;
        // 주인공 캐릭터의 회전값 지정
        transform.localRotation = Quaternion.LookRotation(lookDir);
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 자신의 로컬 캐릭터인 경우 자신의 데디터를 다른 네트워크 유저에게 송신
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            receivePos = (Vector3)stream.ReceiveNext();
            receiveRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
