using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifacts : MonoBehaviour
{

    private string m_Name ;
    private string m_Description;
    private string m_Owner;

    public enum discoverStatus {
        hidden,
        visible,
        discoverd,
    }

    private discoverStatus m_Status;

    private InfoBoard infoBoard;

    public void Init()
    {
        m_Name = "Name";
        m_Description = "Des";
        m_Status = discoverStatus.hidden;
    }

    public void ShowInfoBoard(bool active)
    {
        if(active)
        {
            if (infoBoard != null)
                return;

            infoBoard = GameObject.Instantiate((GameObject)Resources.Load("InfoBoard") as GameObject, transform.position , Quaternion.identity).GetComponent<InfoBoard>();
            infoBoard.Init(m_Name, m_Description, transform.position,true,true);
        }
        else
        {
            if (infoBoard == null)
                return;

            Debug.Log("kill board2");
            Destroy(infoBoard.gameObject);
        }

    }

}
