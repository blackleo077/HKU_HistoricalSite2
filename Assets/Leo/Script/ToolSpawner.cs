using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolSpawner : MonoBehaviour
{

    public OVRInputController inputcontroller;

    public Transform handR;
    public Tools[] ToolsPrefab;

    Tools currentTools;

    private void Start()
    {
        SpawnTool(ToolsPrefab[0]);
    }

    public void SpawnTool(Tools targetTool)
    {
        RemoveExistTool();
        Tools tool = GameObject.Instantiate(targetTool.gameObject, Vector3.zero, Quaternion.identity, handR).GetComponent<Tools>();
        currentTools = tool;
        tool.SetInputController(inputcontroller);
    }


    void RemoveExistTool()
    {
        if (currentTools)
        {
            Destroy(currentTools.gameObject);
        }
        else
        {
            return;
        }
    }

}
