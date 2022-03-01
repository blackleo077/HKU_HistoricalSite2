using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;

using UnityEngine;
//using StreamLOD = Oculus.Avatar2.OvrAvatarEntity.StreamLOD;



    public class PacketData : IDisposable
    {
        public NativeArray<byte> data;
        public int lodID;
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
            Debug.LogError("Dispose fn");
            if (data.IsCreated)
            {
                data.Dispose();
                Debug.LogError("Dispose data");
            }
            data = default;
        }

        public bool Unretained => refCount == 0;
        public PacketData Retain() { ++refCount; return this; }
        public bool Release()
        {
            Debug.LogError("Release data");
            return --refCount == 0;
        }
    }
