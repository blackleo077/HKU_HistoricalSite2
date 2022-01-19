using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonLogin : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update

    string gameVersion = "1";
    void Start()
    {
        ConnectToServer();
    }



    public void ConnectToServer()
    {

        //  PhotonNetwork.NickName = System.DateTime.Now.Minute +"-"+ System.DateTime.Now.Second;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
    }

    public override void OnConnectedToMaster() // Auto connect to Photon Network
    {
        Debug.Log("Connected to Master");
        // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
        PhotonNetwork.JoinRandomRoom();
    }


    public override void OnDisconnected(DisconnectCause cause) // Auto disconnect to Photon Network
    {
        Debug.Log("Disconnected to Master");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Create Room");
        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Connect to Room");
        Debug.Log("Load Game Scene");
        PhotonNetwork.LoadLevel("HistoricalSite");
    }



}
