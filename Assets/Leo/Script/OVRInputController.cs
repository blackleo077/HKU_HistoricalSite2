using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Photon.Pun;

[RequireComponent(typeof(ToolSpawner))]
public class OVRInputController : MonoBehaviourPun
{
    PhotonOVRPlayer player;

    public OVRInput.Button LaserButton;
    public OVRInput.Axis1D LaserAxis;

    public OVRInput.Button RecallTeleportButton;

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
            }

            if (OVRInput.Get(LaserAxis) > 0.5f)
            {
                targettool.ActivatingTool();
            }

            if (OVRInput.GetUp(LaserButton))
            {
                targettool.ReleaseTool();
            }
        }
        else
        {
            GetComponent<ToolSpawner>().SpawnTool(ToolSpawner.ToolsType.laser);
        }
    }

    void RecallPlayersButton()
    {
        if (OVRInput.GetDown(RecallTeleportButton) || Input.GetKeyDown(KeyCode.R) )
        {
            RecallPlayers();
        }
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
}
