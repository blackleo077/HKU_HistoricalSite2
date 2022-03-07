using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolSpawner : MonoBehaviour
{

    public OVRInputController inputcontroller;

    public Transform spawnToolTransform;
    public Tools[] ToolsPrefab;

    public enum ToolsType
    {
        laser,
    }

    Tools currentTools;

    private void Start()
    {
    }

    public void SpawnTool(ToolsType toolsType)
    {
        Debug.Log("SpawnTool");
        RemoveExistTool();
        Tools tool = GameObject.Instantiate(ToolsPrefab[(int)toolsType].gameObject, Vector3.zero, Quaternion.identity, spawnToolTransform).GetComponent<Tools>();
        tool.transform.localPosition = Vector3.zero;
        tool.transform.localRotation = Quaternion.identity;
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
