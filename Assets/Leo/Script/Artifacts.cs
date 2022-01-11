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

    private GameObject infoBoard;

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
            if(infoBoard != null)
            {
                GameObject board = (GameObject)Resources.Load("InfoBoard") as GameObject;
                InfoBoard info = board.GetComponent<InfoBoard>();
                info.Init(m_Name, m_Description);
                infoBoard = board;

            }
        }
        else
        {
            if (infoBoard == null)
                return;
            Destroy(infoBoard);
        }

    }

}
