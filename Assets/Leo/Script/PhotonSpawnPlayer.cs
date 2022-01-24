using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class PhotonSpawnPlayer : MonoBehaviour
{
    public static PhotonSpawnPlayer instance;
    public List<PhotonOVRPlayer> ovrplayerlist;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public GameObject VRPlayerPrefab;
    public Transform SpawnPoint;

    [SerializeField]
    float SpawnRadius;


    public IEnumerator  SpawnPlayer()
    {

        RemoveExistOVRPlayer();
        yield return new WaitForEndOfFrame();
        SpawnPlayerObject();
    }

    void SpawnPlayerObject()
    {
        Debug.Log("Spawn");
        GameObject player = PhotonNetwork.Instantiate(VRPlayerPrefab.name, GetSpawnPosition(), Quaternion.identity);
        ovrplayerlist.Add(player.GetComponent<PhotonOVRPlayer>());
        Debug.LogErrorFormat("PhotonOVRPlayer list : {0}", ovrplayerlist.Count);


    }

    Vector3 GetSpawnPosition()
    {
        Vector3 spawnpos = SpawnPoint.transform.position;
        float px = Random.Range(spawnpos.x - SpawnRadius, spawnpos.x + SpawnRadius);
        float py = Random.Range(spawnpos.y - SpawnRadius, spawnpos.y + SpawnRadius);
        float pz = spawnpos.z;
        return spawnpos;
    }


    void RemoveExistOVRPlayer()
    {

        PhotonView[] ovrplayer = PhotonView.FindObjectsOfType<PhotonView>();
        //Debug.LogErrorFormat("PhotonView list : {0}",ovrplayer.Length);
        //Debug.LogErrorFormat("PhotonOVRPlayer list : {0}", ovrplayerlist.Count);

        foreach (PhotonView player in ovrplayer)
        {
            if (!player.IsMine)
            {
                Debug.LogError("Not my Player. Remove its OVRManager");
                player.GetComponent<PhotonOVRPlayer>().RemoveUnnecessaryComponents();
            }
        }
    }

    public string GetAvatarID()
    {
        return "10150030458762178";
    }
}
