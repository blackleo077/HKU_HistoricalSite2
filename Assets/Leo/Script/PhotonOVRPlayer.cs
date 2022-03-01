using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;


using Oculus.Avatar;
using Oculus.Platform;
using Oculus.Platform.Models;
using Oculus.Avatar2;

public class PhotonOVRPlayer : MonoBehaviourPun
{
    OVRManager ovrManager;
    PhotonGameManager GM;


    [SerializeField]
    public ulong AvatarID;

    bool BodyisLoaded = false;
    bool BodyNetworkSyncExists = false;

    [SerializeField] FadeController fadeController;
    [SerializeField] OVRCameraRig myCameraRig;
    [SerializeField] Camera mycamera;
    [SerializeField] OVRInputController inputController;
    [SerializeField] BoundaryController boundaryController;
    [SerializeField] TeleportController teleportController;
    [SerializeField] LocomotionController locomotionController;
    [SerializeField] LocomotionTeleport locomotionTeleport;
    [SerializeField] SimpleCapsuleWithStickMovement stickmovement;
    [SerializeField] RemoteLoopbackManager RemoteAvatarController;

    [SerializeField] PhotonAvatarController LocalAvatar;
    [SerializeField] PhotonAvatarController RemoteAvatar;
    [SerializeField] AvatarLayer DontRenderLayer;
    [SerializeField] GameObject RemoteAvatarPrefab;

    GameObject Robot;
    OvrAvatarComponent[] SyncRobotComponent;

    private void Awake()
    {
        if (PhotonNetwork.IsConnected)
        {
            GM = GameObject.FindObjectOfType<PhotonGameManager>();
            GM.GetSpawnPlayer().AddOVRPlayer(this);
        }

        if (photonView.IsMine)
        {
            ovrManager = myCameraRig.gameObject.AddComponent<OVRManager>(); // singleton
            ovrManager.AllowRecenter = false;
            ovrManager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
        }
        else
        {
            RemoveUnnecessaryComponents();
        }


        //if is mine
        //add avatar
        //set my id to avatar
        //ovr init

        //if not mine
        //get targert id
        //add avatar component
        //avatar script read my avatar id
        //set id to avatar
        
        //control avatar appearence layer
        if (photonView.IsMine)
        {
            //addRemoteAvatarInitial();
            // RemoteAvatar.FirstPersonLayer = DontRenderLayer;
            // RemoteAvatar.ThirdPersonLayer = DontRenderLayer;
            LocalAvatar.gameObject.SetActive(true);
            RemoteAvatar.gameObject.SetActive(true);
        }
        else
        {
            LocalAvatar.gameObject.SetActive(false);
            RemoteAvatar.gameObject.SetActive(true);
            GetNetworkAvatarID();
        }
        
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            Debug.LogError("Keep camera enable");
            mycamera.enabled = true;
            mycamera.gameObject.AddComponent<OVRScreenFade>();  // singleton


            fadeController = gameObject.AddComponent<FadeController>();
            fadeController.Register(GetComponentInChildren<LocomotionTeleport>());
        }
        else
        {
            Debug.LogError("disable camera ");
            mycamera.enabled = false;
        }
    }
    #region



    #endregion

   

    void GetNetworkAvatarID()
    {
        photonView.RPC("GetOculusUserID", photonView.Owner, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    void GetOculusUserID(int playeractionnumber)
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if(playeractionnumber == p.ActorNumber)
            {
                Debug.LogErrorFormat("Send my avatarid {0} to player {1}", LocalAvatar.GetAvatarID(), p);
                photonView.RPC("SetOculusUserID", p, LocalAvatar.GetAvatarID().ToString());
            }
        }
    }

    [PunRPC]
    void SetOculusUserID(string avatarid)
    {
        Debug.LogError("Set AvatarID "+ avatarid);
        AvatarID =  ulong.Parse( avatarid);
        RemoteAvatar.SetRemoteAvatar(AvatarID);

    }


    #region Public method

    public void RemoveUnnecessaryComponents()
    {
        Debug.LogError("Remove unnesseary");
        Destroy(myCameraRig);
        DestroyAudioListenerInChildren();
        DestroyPhysicsComponents();
        Destroy(locomotionController.gameObject);
        Destroy(stickmovement);
        Destroy(boundaryController);

    }

    public void RecallPlayers()
    {

        Vector3 targetPos = transform.position;
        GM.MoveToMaster(targetPos);
        /*
        List<PhotonOVRPlayer> players = GM.GetSpawnPlayer().GetPhotonOVRPlayer();
        Debug.LogErrorFormat("Exist players: {0}",players.Count);
        foreach (PhotonOVRPlayer p in players)
        {
            Debug.LogError(p.photonView.ViewID+"Manual call rpc");
            //p.TeleportTo(targetPos);
        }

        Debug.LogError("Photon network players : " +PhotonNetwork.PlayerList.Length);
        
        photonView.RPC("TeleportTo", RpcTarget.All,targetPos);
        */
    }

    public Camera GetCamera()
    {
        return mycamera;
    }

    public OVRCameraRig GetOVRRig()
    {
        return myCameraRig;
    }

    public OVRManager GetOVRManager()
    {
        return ovrManager;
    }

    public void testRPC(string msg)
    {
        if (photonView.IsMine)
        {
            Debug.LogError(photonView.ViewID + "receive rpc call " + msg);
        }
        else
        {
            Debug.LogError(photonView.ViewID + " Not my object" + msg);
        }
    }

    public void TeleportTo(Vector3 tplocation)
    {
        if (photonView.IsMine)
        {
            Debug.LogError("Mine Object"+photonView);
            Debug.LogError(photonView.ViewID + " receive RPC Client Call Teleport to " + tplocation);
            StartCoroutine(SmoothTeleport(tplocation));
        }
        else
        {

            Debug.LogError("Not My Object" + photonView);
        }
    }
    IEnumerator SmoothTeleport(Vector3 location)
    {
        if(fadeController)
            fadeController.TPStart();
        yield return new WaitForSeconds(0.5f);
        teleportController.Teleport(location);
        if(fadeController)
            fadeController.TPEnd();

        Debug.LogErrorFormat("Player {0} tp complete", photonView.ViewID);
    }
    #endregion

    #region destroy method 
    private void DestoryComponents()
    {
        MonoBehaviour[] components = GetComponentsInChildren<MonoBehaviour>();

        foreach (MonoBehaviour c in components)
        {
            if (c is PhotonView || c is PhotonOVRPlayer || c is MonoBehaviourPun|| c is FadeController || c is TeleportController)
            {
            }
            else
            {
                Debug.LogError("Destroy "+c);
                Destroy(c);
            }

        }
    }

    private void DestroyPhysicsComponents()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rgb in rigidbodies)
        {
            Destroy(rgb);
        }
        foreach (Collider c in colliders)
        {
            Destroy(c);
        }
    }

    private void DestroyCameraInChildren()
    {
        Camera[] cameras = GetComponentsInChildren<Camera>();
        Debug.LogError("Destroy cameras");
        foreach (Camera cam in cameras)
        {
            Destroy(cam);
        }
    }

    private void DestroyAudioListenerInChildren()
    {
        AudioListener[] audioListeners = GetComponentsInChildren<AudioListener>();
        foreach (AudioListener aud in audioListeners)
        {
            Destroy(aud);
        }
    }


    private void OnDestroy()
    {

    }

    #endregion
}
