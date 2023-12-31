using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using ExitGames.Client.Photon;

public class PointObj : MonoBehaviourPunCallbacks, IPunObservable
{
    PointSpwan pointspwan;

    public float max_health;
    public float current_health;

    private Vector3 initialPosition;
    private bool moveForward = true;

    public float moveDistance = 10f;
    public float moveSpeed = 200f;

    public float coolTime = 1f;
    private float lastDamageTime = 0f; // 마지막 데미지 처리 시간 저장용 변수

    public string targetType;

    private void Awake()
    {
        pointspwan = FindObjectOfType<PointSpwan>();
    }

    void Start()
    {
        gameObject.layer = 11;
        current_health = max_health;
        // 초기 위치 저장
        initialPosition = transform.position;
    }

    void Update()
    {
        if (moveForward)
        {
            // 앞으로 이동
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

            // 이동 거리 확인하여 처음 위치로 돌아감
            if (transform.position.z >= initialPosition.z + moveDistance)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, initialPosition.z + moveDistance);
                moveForward = false; // 뒤로 이동하기 위해 상태 변경
            }
        }
        else
        {
            // 뒤로 이동
            transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);

            // 이동 거리 확인하여 처음 위치로 돌아감
            if (transform.position.z <= initialPosition.z - moveDistance)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, initialPosition.z - moveDistance);
                moveForward = true; // 앞으로 이동하기 위해 상태 변경
            }
        }

        // X와 Y 축의 움직임 제한 (수정된 부분)
        var newPosition = new Vector3(initialPosition.x, initialPosition.y, transform.position.z);
        transform.SetPositionAndRotation(newPosition, Quaternion.identity);
    }

    [PunRPC]
    public void PointUp(int p_damage)
    {
        current_health -= p_damage;

        if (current_health <= 0 && photonView != null && photonView.IsMine)
        {
            bool isSpecialTarget = gameObject.name == "PointTarget2(Clone)";

            PhotonNetwork.Destroy(gameObject);

            pointspwan.isTargetDestroyed = true;

            int playerId = photonView.Owner.ActorNumber;

            int scoreIncrease = isSpecialTarget ? 2 : 1;

            object[] data = new object[] { playerId, scoreIncrease };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(1, data, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    [PunRPC]
    public void PointUpWithFloat(float p_damage)
    {
        current_health -= (int)p_damage;

        if (current_health <= 0 && photonView != null && photonView.IsMine)
        {
            bool isSpecialTarget = gameObject.name == "PointTarget2(Clone)";

            PhotonNetwork.Destroy(gameObject);

            pointspwan.isTargetDestroyed = true;

            int playerId = photonView.Owner.ActorNumber;

            int scoreIncrease = isSpecialTarget ? 2 : 1;

            object[] data = new object[] { playerId, scoreIncrease };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(1, data, raiseEventOptions, SendOptions.SendReliable);
        }
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 로컬 플레이어의 데이터 전송
            stream.SendNext((int)current_health); // int로 형변환하여 전송
        }
        else
        {
            // 원격 플레이어의 데이터 수신
            current_health = (int)stream.ReceiveNext(); // int로 형변환하여 받아옴
        }
    }
}
