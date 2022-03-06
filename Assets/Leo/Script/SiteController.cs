using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class SiteController : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform ArtifactRoot;
    [SerializeField] Transform[] PreSettleLocation;
    [SerializeField] Sprite[] PreSettleLocationSprite;
    [SerializeField] Artifacts[] ArtifactsPrefab;
    [SerializeField] Artifacts InfoMarkPrefab;
    // Start is called before the first frame update

    List<Vector3> Waitinglocations = new List<Vector3>();
    List<Sprite> WaitingSprites = new List<Sprite>();
    List<Vector3> OccupiedLocations = new List<Vector3>();

    List<Artifacts> WaitingArtifacts = new List<Artifacts>();
    List<Artifacts> SiteArtifacts = new List<Artifacts>();
    List<Artifacts> DiscoveredArtifacts = new List<Artifacts>();


    //no used
    enum SpawnSequence
    {
        InOrder,
        Random
    }

    [SerializeField] private SpawnSequence SpawnOrder;

    enum ObjectType
    {
        Artifacts,
        InfoMark
    }

    [SerializeField] private ObjectType SpawnObjectType;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            InitSitesArtifacts();
            StartCoroutine(SpawnArtifact(true));
        }
        else
        {
            //photonView.RPC("RequestNetworkSpawnArtifacts", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    #region Test
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogError("PlayerEnter Room"+newPlayer.ActorNumber);
        /*
        Player targetplayer = null;
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.ActorNumber == newPlayer.ActorNumber)
                {
                    targetplayer = p;
                }
            }

            if (targetplayer != null)
            {
                for (int i = 0; i < SiteArtifacts.Count; i++)
                {
                    photonView.RPC("SyncNetworkArtifacts", targetplayer, SiteArtifacts[i].GetComponent<PhotonView>().ViewID, SiteArtifacts[i].Location);
                }

                Debug.Log("SyncNetworkArtifacts to " + targetplayer);
            }
            else
            {
                Debug.LogError("Cant FindPlayer ");
            }
        }
        */
        base.OnPlayerEnteredRoom(newPlayer);
    }

    [PunRPC]
    void RequestNetworkSpawnArtifacts(int pid)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Debug.LogError("Server RPC RequestNetworkSpawnArtifacts from "+pid);
        Player targetplayer = null;
        foreach(Player p in PhotonNetwork.PlayerList)
        {
            if(p.ActorNumber == pid)
            {
                targetplayer = p;
                Debug.LogError("FindPlayer " + targetplayer);
            }
        }

        if (targetplayer!=null)
        {
            for (int i = 0; i < SiteArtifacts.Count; i++)
            {
                photonView.RPC("NetworkSpawnArtifact", targetplayer, SiteArtifacts[i].GetComponent<PhotonView>().ViewID, SiteArtifacts[i].Location);
            }
        }
    }

    [PunRPC]
    void SyncNetworkArtifacts(int artviewid, Vector3 location )
    {
        if (PhotonNetwork.IsMasterClient)
            return;
        Artifacts art = PhotonView.Find(artviewid).GetComponent<Artifacts>();
        Debug.LogFormat("Spawn Artifact {0} at {1}",artviewid,location);
        SpawnArtifactsAt(art, location);
    }

    #endregion

    void InitSitesArtifacts()
    {
        if (SpawnObjectType == ObjectType.Artifacts) //spawn artifacts from array 
        {
            for (int i = 0; i < ArtifactsPrefab.Length; i++)
            {
                Artifacts art = PhotonNetwork.Instantiate(ArtifactsPrefab[i].gameObject.name, Vector3.zero, Quaternion.identity).GetComponent<Artifacts>();
                WaitingArtifacts.Add(art);
                art.gameObject.SetActive(false);
            }
        }
        else if (SpawnObjectType == ObjectType.InfoMark) // spawn infomark to same number of location length
        {
            for (int i = 0; i < PreSettleLocationSprite.Length; i++)
            {
                Artifacts infomark = PhotonNetwork.Instantiate(InfoMarkPrefab.gameObject.name, Vector3.zero, Quaternion.identity).GetComponent<Artifacts>();
                infomark.SetBoardStyle(Artifacts.BoardStyle.Image);
                infomark.SetInfoImage(PreSettleLocationSprite[i]);
                WaitingArtifacts.Add(infomark);
                infomark.gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < PreSettleLocation.Length; i++)
        {
            Waitinglocations.Add(PreSettleLocation[i].position);
        }
    }
    public IEnumerator SpawnArtifact(bool repeatSpawn)
    {
        Artifacts art = DrawSpawnArtifact();
        Vector3 location = DrawSpawnLocation();

        if (art == null)
        {
            Debug.LogError("No more artifact exists");
            yield return false;
        }
        else if (location == null)
        {
            Debug.LogError("No more emptylocation exists");
            yield return false;
        }
        else
        {
            Debug.Log("SpawnArtifact at location");
            SpawnArtifactsAt(art, location);
            if (repeatSpawn)
            {
                StartCoroutine(SpawnArtifact(true));
            }
        }
    }


    void SpawnArtifactsAt(Artifacts artifacts,Vector3 targetlocation)
    {
        SiteArtifacts.Add(artifacts);
        WaitingArtifacts.Remove(artifacts);

        OccupiedLocations.Add(targetlocation);
        Waitinglocations.Remove(targetlocation);

        artifacts.transform.SetParent(ArtifactRoot);
        artifacts.transform.position = targetlocation;
        artifacts.transform.rotation = Quaternion.identity;
        artifacts.Location = targetlocation;
        artifacts.gameObject.SetActive(true);
    }


    Vector3 DrawSpawnLocation()
    {
        Vector3 templocation = Vector3.zero;
        if (Waitinglocations.Count <= 0)
        {
            return Vector3.zero;
        }
        else
        {
            if (SpawnOrder == SpawnSequence.Random)
            {
                int ran = Random.Range(0, Waitinglocations.Count);
                templocation = Waitinglocations[ran];
            }
            else
            {
                templocation = Waitinglocations[0];
            }
            return templocation;
        }
    }

    Artifacts DrawSpawnArtifact()
    {
        Artifacts art = null;
        if (WaitingArtifacts.Count <= 0)
        {
            return null;
        }
        else
        {
            if (SpawnOrder == SpawnSequence.Random)
            {
                int ran = Random.Range(0, WaitingArtifacts.Count);
                art = WaitingArtifacts[ran];
            }
            else
            {
                art = WaitingArtifacts[0];

            }
            return art;
        }
    }

    public void ArtifactIsDiscovered(Artifacts art)
    {
        if(SiteArtifacts.Remove(art))
        {
            DiscoveredArtifacts.Add(art);
            Debug.LogFormat("Artifact {0} is discovered",art);
        }
        else
        {
            Debug.LogError("Artifact not Exist");
        }
    }
}
