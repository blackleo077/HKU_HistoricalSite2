using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PhotonGameManager : MonoBehaviourPunCallbacks
{

    public static PhotonGameManager instance;
    [SerializeField]
    PhotonSpawnPlayer spawnPlayer;
    

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public PhotonSpawnPlayer GetSpawnPlayer()
    {
        return spawnPlayer;
    }

    public void MoveToMaster(Vector3 pos)
    {
        Debug.LogError("GM MoveToMaster"+pos);
        photonView.RPC("MovePlayers", RpcTarget.All, pos);
    }

    [PunRPC]
    void MovePlayers(Vector3 pos)
    {
       foreach( PhotonOVRPlayer player in spawnPlayer.GetPhotonOVRPlayer())
        {
            player.TeleportTo(pos);
        }
    }

    private void Start()
    {
        Debug.Log("PhotonGameManager Init");
        
        if (PhotonNetwork.IsMasterClient)
        {

            Debug.LogError("Master Client Enter Room");
            spawnPlayer.SpawnPlayer();

        }
        else
        {
            Debug.LogError("Normal Player Enter Room");

            spawnPlayer.SpawnPlayer();
        }
        
    }

    #region Photon Callbacks

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("LoginPage");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.LogError("Join room");
       

    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

    }


    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects


        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

        }
    }


    #endregion


    #region Public Methods

    public string GetAvatarID()
    {
        return "10150030458762178";
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    #endregion
}
