using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonPlayerData 
{
    int m_PhotonActionNumber;
    string m_OculusAvatarID;

    public PhotonPlayerData(int actionnumber, string oculusavatarid)
    {
        PhotonActionNumber = actionnumber;
        OculusAvatarID = oculusavatarid;
    }

    public int PhotonActionNumber
    {
        get { return m_PhotonActionNumber; }
        set { m_PhotonActionNumber = value; }
    }
    public string OculusAvatarID
    {
        get { return m_OculusAvatarID; }
        set { m_OculusAvatarID = value; }
    }
}
