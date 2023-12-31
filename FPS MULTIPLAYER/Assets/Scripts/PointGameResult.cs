using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class PointGameResult : MonoBehaviourPunCallbacks
{
    private PlayerMovement playerMove;
    private Manager manager;
    private Weapon weapon;
    private PointGameStart pointGameStart;

    public GameObject resultBord;

    public TMP_Text gwinnerName;
    public TMP_Text gwinnerKill;
    public TMP_Text swinnerName;
    public TMP_Text swinnerKill;
    public TMP_Text bwinnerName;
    public TMP_Text bwinnerKill;

    public bool isResult = false;

    void Start()
    {
        playerMove = FindObjectOfType<PlayerMovement>();
        manager = FindObjectOfType<Manager>();
        weapon = FindObjectOfType<Weapon>();
        pointGameStart = FindObjectOfType<PointGameStart>();

        var data = PlayerPrefs.GetString("PlayerData").Split(',');
    }

    void Update()
    {
        if (!isResult)
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player.GetScore() > 3)
                {
                    PlayerPrefs.SetString("PlayerData", GetPlayerData());
                    isResult = true;
                    StartCoroutine(go());
                    break;
                }
            }
        }

        UpdateScores();
    }

    void UpdateScores()
    {
        var scores = new List<(string name, int score)>();

        foreach (var player in PhotonNetwork.PlayerList)
        {
            scores.Add((player.NickName, player.GetScore()));
        }

        scores.Sort((x, y) => y.score.CompareTo(x.score));

        if (scores.Count >= 1)
        {
            gwinnerName.text = scores[0].name;
            gwinnerKill.text = scores[0].score.ToString();
        }
        if (scores.Count >= 2)
        {
            swinnerName.text = scores[1].name;
            swinnerKill.text = scores[1].score.ToString();
        }
        if (scores.Count >= 3)
        {
            bwinnerName.text = scores[2].name;
            bwinnerKill.text = scores[2].score.ToString();
        }
    }

    string GetPlayerData()
    {
        var data = new List<string>();
        foreach (var player in PhotonNetwork.PlayerList)
        {
            data.Add(player.NickName + ":" + player.GetScore());
        }

        return string.Join(",", data);
    }

    IEnumerator go()
    {
        manager.RespawnPlayer();
        playerMove.isPointGame = false;
        weapon.isWeapon = false;
        pointGameStart.pointGameUI.gameObject.SetActive(false);
        pointGameStart.pointGamemanager.gameObject.SetActive(false);
        resultBord.SetActive(true);
        pointGameStart.moveMap.SetActive(true);

        yield return new WaitForSeconds(2f);

        resultBord.SetActive(false);

        yield return new WaitForSeconds(2f);

        deleteDate();
    }

    public void deleteDate()
    {
        PlayerPrefs.DeleteKey("PlayerData");

        foreach (var player in PhotonNetwork.PlayerList)
        {
            player.SetScore(0);
        }

        isResult = false;
    }
}

