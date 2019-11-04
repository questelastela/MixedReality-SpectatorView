﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if !UNITY_EDITOR && UNITY_WSA
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
#endif

namespace Microsoft.MixedReality.SpectatorView
{
    /// <summary>
    /// Loads calibration data from the Pictures library on the device and transfers that data
    /// to the compositor upon connection.
    /// </summary>
    [RequireComponent(typeof(INetworkManager))]
    public class CalibrationDataProvider : MonoBehaviour
    {
        private INetworkManager networkManager;

        private void Awake()
        {
            networkManager = GetComponent<INetworkManager>();
            networkManager.Connected += NetworkManagerConnected;
        }

        private void OnDestroy()
        {
            networkManager.Connected -= NetworkManagerConnected;
        }

        private void NetworkManagerConnected(INetworkConnection obj)
        {
            SendCalibrationDataAsync();
        }

#if !UNITY_EDITOR && UNITY_WSA
        private async void SendCalibrationDataAsync()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter message = new BinaryWriter(memoryStream))
            {
                StorageFile file = (await KnownFolders.PicturesLibrary.TryGetItemAsync(@"CalibrationData.json").AsTask()) as StorageFile;
                if (file != null)
                {
                    byte[] contents = (await FileIO.ReadBufferAsync(file)).ToArray();
                    message.Write("CalibrationData");
                    message.Write(contents.Length);
                    message.Write(contents);
                    networkManager.Broadcast(memoryStream.ToArray());
                }
            }
        }
#else
        private void SendCalibrationDataAsync()
        {
        }
#endif
    }
}