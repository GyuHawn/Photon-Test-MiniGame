using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PDamage : MonoBehaviourPunCallbacks, IPunObservable
{
    private PlayerMovement player;
    public float coolTime = 1f;
    private float lastDamageTime = 0f; // 마지막 데미지 처리 시간 저장용 변수
    
    private void Awake()
    {
        player = GetComponent<PlayerMovement>();
    }
    

    [PunRPC]
    public void TakeDamage(int p_damage)
    {
        if (photonView.IsMine)
        {
            PhotonView pv = GetComponent<PhotonView>();
            if (!pv.ObservedComponents.Contains(this))
            {
                pv.ObservedComponents.Add(this);
            }

            if (player != null)
            {
                float currentTime = Time.time; // 현재 시간 저장
                float timePassed = currentTime - lastDamageTime; // 마지막 데미지 처리 이후 경과 시간 계산
                if (timePassed >= coolTime) // 데미지 처리 Cool Time 이 지났다면
                {
                    player.current_health -= p_damage;
                    player.RefreshHealthBar();
                    Debug.Log(player.current_health);

                    if (player.current_health <= 0)
                    {
                        player.manager.Spawn();
                        PhotonNetwork.Destroy(gameObject);

                        // Decrease the kill count when player dies
                        FindObjectOfType<PointCount>().photonView.RPC("DecreaseKillTextRPC", RpcTarget.AllBuffered);

                        Debug.Log("YOU DIED");
                    }
                    lastDamageTime = currentTime; // 마지막 데미지 처리 시간 갱신
                }
                else // 아직 데미지 처리 Cool Time 이 지나지 않았다면
                {
                    Debug.Log("Cool Time Not Over!");
                }
            }
        }
    }

    [PunRPC]
    public void TakeDamageWithFloat(float p_damage)
    {
        if (photonView.IsMine)
        {
            PhotonView pv = GetComponent<PhotonView>();
            if (!pv.ObservedComponents.Contains(this))
            {
                pv.ObservedComponents.Add(this);
            }

            if (player != null)
            {
                player.current_health -= (int)p_damage;
                player.RefreshHealthBar();
                Debug.Log(player.current_health);

                if (player.current_health <= 0)
                {
                    player.manager.Spawn();
                    PhotonNetwork.Destroy(gameObject);

                    // Decrease the kill count when player dies
                    FindObjectOfType<PointCount>().photonView.RPC("DecreaseKillTextRPC", RpcTarget.AllBuffered);

                    Debug.Log("YOU DIED");
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {

            stream.SendNext((int)player.current_health); // int로 형변환하여 전송

        }
        else
        {
            // 원격 플레이어의 데이터 수신
            string receivedScene = (string)stream.ReceiveNext(); // 씬 이름 받아옴
            player.current_health = (int)stream.ReceiveNext(); // int로 형변환하여 받아옴
            player.RefreshHealthBar();

        }
    }
}