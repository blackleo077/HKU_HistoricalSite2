// ----------------------------------------------------------------------------
// <copyright file="CustomTypes.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
// Sets up support for Unity-specific types. Can be a blueprint how to register your own Custom Types for sending.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


namespace Photon.Pun
{
    using System;
    using UnityEngine;
    using Photon.Realtime;
    using ExitGames.Client.Photon;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;



    /// <summary>
    /// Internally used class, containing de/serialization method for PUN specific classes.
    /// </summary>
    internal static class CustomTypes
    {
        /// <summary>Register de/serializer methods for PUN specific types. Makes the type usable in RaiseEvent, RPC and sync updates of PhotonViews.</summary>
        internal static void Register()
        {
            PhotonPeer.RegisterType(typeof(Player), (byte) 'P', SerializePhotonPlayer, DeserializePhotonPlayer);
            //edit Leo
            PhotonPeer.RegisterType(typeof(PacketData), (byte)'D', SerializePhotonPlayer, DeserializePhotonPlayer);
        }


        #region Custom De/Serializer Methods

        public static readonly byte[] memPlayer = new byte[4];

        private static short SerializePhotonPlayer(StreamBuffer outStream, object customobject)
        {
            int ID = ((Player) customobject).ActorNumber;

            lock (memPlayer)
            {
                byte[] bytes = memPlayer;
                int off = 0;
                Protocol.Serialize(ID, bytes, ref off);
                outStream.Write(bytes, 0, 4);
                return 4;
            }
        }

        private static object DeserializePhotonPlayer(StreamBuffer inStream, short length)
        {
            if (length != 4)
            {
                return null;
            }

            int ID;
            lock (memPlayer)
            {
                inStream.Read(memPlayer, 0, length);
                int off = 0;
                Protocol.Deserialize(out ID, memPlayer, ref off);
            }

            if (PhotonNetwork.CurrentRoom != null)
            {
                Player player = PhotonNetwork.CurrentRoom.GetPlayer(ID);
                return player;
            }
            return null;
        }


        public static readonly byte[] memPacketData = new byte[4];

        private static short SerializePhotonPacketData(StreamBuffer outStream, object customobject)
        {
            NativeArray<byte> data = ((PacketData)customobject).data;
            byte[] databyte = ToRawBytes(data);
            lock (memPlayer)
            {
                byte[] bytes = memPlayer;
                int off = 0;
                //Protocol.Serialize(databyte, bytes, ref off);
                outStream.Write(bytes, 0, 4);
                return 4;
            }
        }

        private static object DeserializePhotonPacketData(StreamBuffer inStream, short length)
        {
            if (length != 4)
            {
                return null;
            }

            int ID;
            lock (memPlayer)
            {
                inStream.Read(memPlayer, 0, length);
                int off = 0;
                Protocol.Deserialize(out ID, memPlayer, ref off);
            }

            if (PhotonNetwork.CurrentRoom != null)
            {
                Player player = PhotonNetwork.CurrentRoom.GetPlayer(ID);
                return player;
            }
            return null;
        }


        #endregion

        #region NativeArray decode
        public static byte[] ToRawBytes<T>(this NativeArray<T> arr) where T : struct
        {
            var slice = new NativeSlice<T>(arr).SliceConvert<byte>();
            var bytes = new byte[slice.Length];
            slice.CopyTo(bytes);
            return bytes;
        }

        public static void CopyFromRawBytes<T>(this NativeArray<T> arr, byte[] bytes) where T : struct
        {
            var byteArr = new NativeArray<byte>(bytes, Allocator.Temp);
            var slice = new NativeSlice<byte>(byteArr).SliceConvert<T>();

            UnityEngine.Debug.Assert(arr.Length == slice.Length);
            slice.CopyTo(arr);
        }

        public static NativeArray<T> FromRawBytes<T>(byte[] bytes, Allocator allocator) where T : struct
        {
            int structSize = UnsafeUtility.SizeOf<T>();

            UnityEngine.Debug.Assert(bytes.Length % structSize == 0);

            int length = bytes.Length / UnsafeUtility.SizeOf<T>();
            var arr = new NativeArray<T>(length, allocator);
            arr.CopyFromRawBytes(bytes);
            return arr;
        }
        #endregion
    }
}