using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Photon.Pun;


using Oculus.Avatar;
using Oculus.Platform;
using Oculus.Platform.Models;

[RequireComponent(typeof(ToolSpawner))]
public class OVRInputController : MonoBehaviourPun
{
    PhotonOVRPlayer player;

    public OVRInput.Button LaserButton;
    public OVRInput.Axis1D LaserAxis;

    public OVRInput.Button RecallTeleportButton;

    public OVRInput.Button DebugPrintTexyBT;

    public Tools targettool;

    private void Start()
    {
        GetComponent<ToolSpawner>().inputcontroller = this;
        player = GetComponentInParent<PhotonOVRPlayer>();
    }

    private void Update()
    {
        if (GetComponent<PhotonView>().IsMine)
        {
             UpdateLaser();
            if (PhotonNetwork.IsMasterClient)
            {
                RecallPlayersButton();
            }
        }
    }
    void UpdateLaser()
    {
        if (targettool != null)
        {
            if (OVRInput.GetDown(LaserButton))
            {
                targettool.TriggerTool();
                if(PhotonNetwork.IsConnected)
                    photonView.RPC("TriggerToolRPC", RpcTarget.Others);
            }

            if (OVRInput.Get(LaserAxis) > 0.5f)
            {
                targettool.ActivatingTool();
                //photonView.RPC("ActivatingToolRPC", RpcTarget.Others);
            }

            if (OVRInput.GetUp(LaserButton))
            {
                targettool.ReleaseTool();
                if (PhotonNetwork.IsConnected)
                    photonView.RPC("ReleaseToolRPC", RpcTarget.Others);
            }
        }
        else
        {
            GetComponent<ToolSpawner>().SpawnTool(ToolSpawner.ToolsType.laser);
            if (PhotonNetwork.IsConnected)
                photonView.RPC("SpawnToolRPC", RpcTarget.OthersBuffered);
        }
    }

    void RecallPlayersButton()
    {
        if (OVRInput.GetDown(RecallTeleportButton) || Input.GetKeyDown(KeyCode.R) )
        {
            RecallPlayers();
        }
    }

    void DebugPrintText()
    {
    }

    void RecallPlayers()
    {
        Debug.LogError(photonView.ViewID +"RecallPlayer");
        player.RecallPlayers();
    }

    public void attachTool(Tools t)
    { 
        targettool = t;
    }
    public void detachTool(Tools t) 
    {
        targettool = null ;
    }


    #region
    [PunRPC]
    void TriggerToolRPC()
    {
        targettool.TriggerTool();
    }

    [PunRPC]
    void ActivatingToolRPC()
    {
        targettool.ActivatingTool();
    }

    [PunRPC]
    void ReleaseToolRPC()
    {
        targettool.ReleaseTool();
    }

    [PunRPC]
    void SpawnToolRPC()
    {
        GetComponent<ToolSpawner>().SpawnTool(ToolSpawner.ToolsType.laser);
    }


    #endregion
}
