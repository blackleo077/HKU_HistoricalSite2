using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class PhotonSpawnPlayer : MonoBehaviour
{
    List<PhotonOVRPlayer> ovrplayerlist = new List<PhotonOVRPlayer>();



    public GameObject VRPlayerPrefab;
    public Transform SpawnPoint;

    float SpawnRadius =0.5f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SpawnPlayer();
        }
    }

    #region Private Method

    void SpawnPlayerObject()
    {
        GameObject player = PhotonNetwork.Instantiate(VRPlayerPrefab.name, GetSpawnPosition(), Quaternion.identity);
    }

    Vector3 GetSpawnPosition()
    {
        Vector3 spawnpos = SpawnPoint.transform.position;
        float px = Random.Range(spawnpos.x - SpawnRadius, spawnpos.x + SpawnRadius);
        float py = Random.Range(spawnpos.y - SpawnRadius, spawnpos.y + SpawnRadius);
        float pz = spawnpos.z;
        return spawnpos;
    }

    #endregion

    #region Public Method
    public void SpawnPlayer()
    {
        SpawnPlayerObject();
    }

    public List<PhotonOVRPlayer> GetPhotonOVRPlayer()
    {
        return ovrplayerlist;
    }

    public void AddOVRPlayer(PhotonOVRPlayer player)
    {
        ovrplayerlist.Add(player);
        Debug.LogError("Current Player:"+ovrplayerlist.Count);
    }

    public void RemoveOVRPlayer(PhotonOVRPlayer player)
    {
        ovrplayerlist.Remove(player);
        Debug.LogError("Current Player:" + ovrplayerlist.Count);
    }
    #endregion
}
