using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Oculus.Avatar2;

using Unity.Collections;

using UnityEngine;
using Photon.Pun;

using StreamLOD = Oculus.Avatar2.OvrAvatarEntity.StreamLOD;

public class AvatarNetworkController : MonoBehaviourPunCallbacks
{
    private const string logScope = "SampleRemoteLoopbackManager";

    // Const & Static Variables
    private const float PLAYBACK_SMOOTH_FACTOR = 0.25f;
    private const int MAX_PACKETS_PER_FRAME = 3;

    private static readonly float[] StreamLodSnapshotIntervalSeconds = new float[OvrAvatarEntity.StreamLODCount] { 1f / 72, 2f / 72, 3f / 72, 4f / 72 };

    #region Internal Classes

    class PacketData : IDisposable
    {
        public NativeArray<byte> data;
        public StreamLOD lod;
        public float fakeLatency;
        public UInt32 dataByteCount;

        private uint refCount = 0;

        public PacketData() { }

        ~PacketData()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (data.IsCreated)
            {
                data.Dispose();
                Debug.LogError("Data is Disposed");
            }
            data = default;
        }

        public bool Unretained => refCount == 0;
        public PacketData Retain() { ++refCount; return this; }
        public bool Release()
        {
            return --refCount == 0;
        }
    };

    class LoopbackState
    {
        public List<PacketData> packetQueue = new List<PacketData>(64);
        public List<byte[]> packetData = new List<byte[]>(32);
        public StreamLOD requestedLod = StreamLOD.Low;
        public float smoothedPlaybackDelay = 0f;
    };

    [System.Serializable]
    public class SimulatedLatencySettings
    {
        [Range(0.0f, 0.5f)]
        public float fakeLatencyMax = 0.25f; //250 ms max latency

        [Range(0.0f, 0.5f)]
        public float fakeLatencyMin = 0.02f; //20ms min latency

        [Range(0.0f, 1.0f)]
        public float latencyWeight = 0.25f; // How much the latest sample impacts the current latency

        [Range(0, 10)]
        public int maxSamples = 4; //How many samples in our window

        internal float averageWindow = 0f;
        internal float latencySum = 0f;
        internal List<float> latencyValues = new List<float>();

        public float NextValue()
        {
            averageWindow = latencySum / (float)latencyValues.Count;
            float randomLatency = UnityEngine.Random.Range(fakeLatencyMin, fakeLatencyMax);
            float fakeLatency = averageWindow * (1f - latencyWeight) + latencyWeight * randomLatency;

            if (latencyValues.Count >= maxSamples)
            {
                latencySum -= latencyValues.First().Value;
                latencyValues.RemoveFirst();
            }

            latencySum += fakeLatency;
            latencyValues.AddLast(fakeLatency);

            return fakeLatency;
        }
    };

    #endregion

    // Serialized Variables
    [SerializeField] private OvrAvatarEntity _localAvatar = null;
    [SerializeField] private List<OvrAvatarEntity> _loopbackAvatars = null;
    [SerializeField] private SimulatedLatencySettings _simulatedLatencySettings = new SimulatedLatencySettings();

    // Private Variables
    private Dictionary<OvrAvatarEntity, LoopbackState> _loopbackStates =
        new Dictionary<OvrAvatarEntity, LoopbackState>();

    private readonly List<PacketData> _packetPool = new List<PacketData>(32);
    private readonly List<PacketData> _deadList = new List<PacketData>(16);

    private PacketData GetPacketForEntityAtLOD(OvrAvatarEntity entity, StreamLOD lod)
    {
        PacketData packet;
        int poolCount = _packetPool.Count;
        if (poolCount > 0)
        {
            var lastIdx = poolCount - 1;
            packet = _packetPool[lastIdx];
            _packetPool.RemoveAt(lastIdx);
        }
        else
        {
            packet = new PacketData();
        }

        packet.lod = lod;
        return packet.Retain();
    }
    private void ReturnPacket(PacketData packet)
    {
        Debug.Assert(packet.Unretained);
        _packetPool.Add(packet);
    }

    private readonly float[] _streamLodSnapshotElapsedTime = new float[OvrAvatarEntity.StreamLODCount];

    byte[] _packetBuffer = new byte[16 * 1024];
    GCHandle _pinnedBuffer;

    public List<OvrAvatarEntity> LoopbackAvatars
    {
        get
        {
            return _loopbackAvatars;
        }

        set
        {
            _loopbackAvatars = value;
            CreateStates();
        }
    }

    #region Core Unity Functions

    protected void Start()
    {
        // Check for other LoopbackManagers in the current scene
        var loopbackManagers = FindObjectsOfType<SampleRemoteLoopbackManager>();
        if (loopbackManagers.Length > 1)
        {
            foreach (var loopbackManager in loopbackManagers)
            {
                if (loopbackManager == this || !loopbackManager.isActiveAndEnabled) { continue; }

                OvrAvatarLog.LogError($"Multiple active LoopbackManagers detected! Please update the scene."
                    , logScope, this);
                break;
            }
        }

        // assume _useAdvancedLodSystem is enabled
        AvatarLODManager.Instance.firstPersonAvatarLod = _localAvatar.AvatarLOD;
        AvatarLODManager.Instance.enableDynamicStreaming = true;

        float firstValue = UnityEngine.Random.Range(_simulatedLatencySettings.fakeLatencyMin, _simulatedLatencySettings.fakeLatencyMax);
        _simulatedLatencySettings.latencyValues.Insert(0, firstValue);
        _simulatedLatencySettings.latencySum += firstValue;

        _pinnedBuffer = GCHandle.Alloc(_packetBuffer, GCHandleType.Pinned);

        CreateStates();
    }

    private void CreateStates()
    {
        foreach (var item in _loopbackStates)
        {
            foreach (var packet in item.Value.packetQueue)
            {
                if (packet.Release())
                {
                    ReturnPacket(packet);
                }
            }
        }
        _loopbackStates.Clear();

        foreach (var loopbackAvatar in _loopbackAvatars)
        {
            _loopbackStates.Add(loopbackAvatar, new LoopbackState());
        }
    }

    private void OnDestroy()
    {
        Debug.LogError("Destory Avatar");
        if (_pinnedBuffer.IsAllocated)
        {
            _pinnedBuffer.Free();
        }

        foreach (var item in _loopbackStates)
        {
            foreach (var packet in item.Value.packetQueue)
            {
                if (packet.Release())
                {
                    ReturnPacket(packet);
                }
            }
        }

        foreach (var packet in _packetPool)
        {
            packet.Dispose();
        }
        _packetPool.Clear();
    }

    private void Update()
    {
        for (int i = 0; i < OvrAvatarEntity.StreamLODCount; ++i)
        {
            // Assume remote Avatar StreamLOD sizes are the same
            float streamBytesPerSecond = _localAvatar.GetLastByteSizeForLodIndex(i) / StreamLodSnapshotIntervalSeconds[i];
            AvatarLODManager.Instance.dynamicStreamLodBitsPerSecond[i] = (long)(streamBytesPerSecond * 8);
        }

        foreach (var item in _loopbackStates)
        {
            var loopbackAvatar = item.Key;
            var loopbackState = item.Value;

            if (!loopbackAvatar.IsCreated)
            {
                continue;
            }

            UpdatePlaybackTimeDelay(loopbackAvatar, loopbackState);

            if (loopbackState.packetData.Count > 0)
            {
                Debug.Log("received queue :" + loopbackState.packetData.Count);
                foreach (var datebyte in loopbackState.packetData)
                {
                    ReceivePacketData(loopbackAvatar, datebyte, loopbackState.requestedLod);
                }
            }

            // "Send" the lod that "remote" avatar wants to use back over the network
            // TODO delay this reception for an accurate test
            loopbackState.requestedLod = loopbackAvatar.activeStreamLod;
        }
    }

    private void LateUpdate()
    {
        // Local avatar has fully updated this frame and can send data to the network
        if (PhotonNetwork.IsConnected)
        {
            if (photonView.IsMine)
            {
                SendSnapshot();
            }
        }
    }

    #endregion

    #region Local Avatar

    private void SendSnapshot()
    {
        if (!_localAvatar.HasJoints) { return; }

        for (int streamLod = (int)StreamLOD.High; streamLod <= (int)StreamLOD.Low; ++streamLod)
        {
            int packetsSentThisFrame = 0;
            _streamLodSnapshotElapsedTime[streamLod] += Time.unscaledDeltaTime;
            while (_streamLodSnapshotElapsedTime[streamLod] > StreamLodSnapshotIntervalSeconds[streamLod])
            {
                SendAvatarData((StreamLOD)streamLod);
                _streamLodSnapshotElapsedTime[streamLod] -= StreamLodSnapshotIntervalSeconds[streamLod];
                if (++packetsSentThisFrame >= MAX_PACKETS_PER_FRAME)
                {
                    _streamLodSnapshotElapsedTime[streamLod] = 0;
                    break;
                }
            }
        }
    }


    private void SendAvatarData(StreamLOD lod)
    {
        byte[] databytes = _localAvatar.RecordStreamData(lod);
        photonView.RPC("SendPacketRPC", RpcTarget.Others, databytes);
    }

    [PunRPC]
    private void SendPacketRPC(byte[] databytes, int lod)
    {
        Debug.Log("SendPacketRPC");
        AddPacketToQueue(databytes, lod);
    }

    private void AddPacketToQueue(byte[] databyte, int lod)
    {
        Debug.Log("AddPacketToQueue");
        foreach (var loopbackState in _loopbackStates.Values)
        {
            if (loopbackState.requestedLod == (StreamLOD)lod)
            {
                loopbackState.packetData.Add(databyte);
            }
        }
    }


    #endregion

    #region "Remote" Loopback Avatar

    private void UpdatePlaybackTimeDelay(OvrAvatarEntity loopbackAvatar, LoopbackState loopbackState)
    {
        // In a real network, maximum packet variation should be computed from the network jitter
        float latencyVariationS = (_simulatedLatencySettings.fakeLatencyMax - _simulatedLatencySettings.fakeLatencyMin);

        // Push back the playback time by the snapshot interval
        float snapshotIntervalS = StreamLodSnapshotIntervalSeconds[(int)loopbackAvatar.activeStreamLod];

        // Sum the latency variation and snapshot rate to determine the playback position
        float playbackDelayS = latencyVariationS + snapshotIntervalS;

        // blend to the target using PLAYBACK_SMOOTH_FACTOR
        loopbackState.smoothedPlaybackDelay = Mathf.Lerp(loopbackState.smoothedPlaybackDelay, playbackDelayS, PLAYBACK_SMOOTH_FACTOR);

        loopbackAvatar.SetPlaybackTimeDelay(loopbackState.smoothedPlaybackDelay);
    }

    private void ReceivePacketData(OvrAvatarEntity loopbackAvatar, in NativeSlice<byte> data, StreamLOD lod)
    {
        loopbackAvatar.ApplyStreamData(in data);
    }

    private void ReceivePacketData(OvrAvatarEntity loopbackAvatar, byte[] data, StreamLOD lod)
    {
        loopbackAvatar.ApplyStreamData( data);
    }

    #endregion
}
