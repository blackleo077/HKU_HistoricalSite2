using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tools : MonoBehaviour
{
    protected string toolName;
    protected Tools mytype;
    protected string description;
    protected float power;
    protected float effectiveDistance;

    protected delegate void OnToolTriggerDelegate();
    protected OnToolTriggerDelegate ToolTriggerDelegate;

    protected delegate void OnToolActivatingDelegate();
    protected OnToolActivatingDelegate ToolActivitingDelegate;

    protected delegate void OnToolReleaseDelegate();
    protected OnToolReleaseDelegate ToolReleaseDelegate;

    protected OVRInputController inputController;
    protected virtual void Start()
    {

    }

    protected virtual void OnToolTrigger() { }

    protected virtual void OnToolActiviting() { }

    protected virtual void OnToolRelease() { }

    public void TriggerTool(){ OnToolTrigger(); }

    public void ActivatingTool(){ OnToolActiviting(); }

    public void ReleaseTool(){ OnToolRelease(); }

    public void SetInputController(OVRInputController controller)
    {
        inputController = controller;
        inputController.attachTool(this);
    }

    protected virtual void OnDestroy()
    {
        inputController.detachTool(this);

    }






}
