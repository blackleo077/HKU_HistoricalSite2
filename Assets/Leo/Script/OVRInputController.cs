using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Photon.Pun;

[RequireComponent(typeof(ToolSpawner))]
public class OVRInputController : MonoBehaviour
{
    OVRInput.Controller TeleportThumbstick;
    public OVRInput.Button LaserButton;
    public OVRInput.Axis1D LaserAxis;
    OVRInput.Controller ResetButton;

    public Tools targettool;

    private void Start()
    {
        GetComponent<ToolSpawner>().inputcontroller = this;
    }

    private void Update()
    {
        if(GetComponent<PhotonView>().IsMine)
            UpdateLaser();
    }
    void UpdateLaser()
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

    public void attachTool(Tools t)
    { 
        targettool = t;
    }
    public void detachTool(Tools t) 
    {
        targettool = null ;
    }
}
