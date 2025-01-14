﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.Smb2;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.Rsvd;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.Sqos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Protocols.TestManager.Detector;

namespace Microsoft.Protocols.TestManager.FileServerPlugin
{

    public class DetectionResultControl
    {
        public List<ResultItemMap> LoadDetectionInfo(DetectionInfo detectionInfo)
        {
            this.info = detectionInfo;

            // Add/Update detected Dialects
            AddDialect(this.info.smb2Info.MaxSupportedDialectRevision);

            // Add/Update detected Capablities
            AddCapability(Capabilities_Values.GLOBAL_CAP_DFS, "DFS (Distributed File System)");
            AddCapability(Capabilities_Values.GLOBAL_CAP_DIRECTORY_LEASING, "Directory Leasing");
            AddCapability(Capabilities_Values.GLOBAL_CAP_ENCRYPTION, "Encryption");
            AddCapability(Capabilities_Values.GLOBAL_CAP_LARGE_MTU, "Large MTU (multi-credit operations)");
            AddCapability(Capabilities_Values.GLOBAL_CAP_LEASING, "Leasing");
            AddCapability(Capabilities_Values.GLOBAL_CAP_MULTI_CHANNEL, "Multiple Channel");
            AddCapability(Capabilities_Values.GLOBAL_CAP_PERSISTENT_HANDLES, "Persistent Handle");

            // Add compression capabilities
            AddCompressionCapabilities(detectionInfo);

            // Add encryption capabilities
            AddSMB2EncryptionAlgorithms(detectionInfo);

            // Add/Update detected IoCtl codes
            AddIoctlCode(CtlCode_Values.FSCTL_OFFLOAD_READ, this.info.F_CopyOffload[0]);
            AddIoctlCode(CtlCode_Values.FSCTL_OFFLOAD_WRITE, this.info.F_CopyOffload[1]);
            AddIoctlCode(CtlCode_Values.FSCTL_FILE_LEVEL_TRIM, this.info.F_FileLevelTrim);
            AddIoctlCode(CtlCode_Values.FSCTL_GET_INTEGRITY_INFORMATION, this.info.F_IntegrityInfo[0]);
            AddIoctlCode(CtlCode_Values.FSCTL_SET_INTEGRITY_INFORMATION, this.info.F_IntegrityInfo[1]);
            AddIoctlCode(CtlCode_Values.FSCTL_LMR_REQUEST_RESILIENCY, this.info.F_ResilientHandle);
            AddIoctlCode(CtlCode_Values.FSCTL_VALIDATE_NEGOTIATE_INFO, this.info.F_ValidateNegotiateInfo);
            AddIoctlCode(CtlCode_Values.FSCTL_SRV_ENUMERATE_SNAPSHOTS, this.info.F_EnumerateSnapShots);

            // Add/Update detected Create Contexts
            AddCreateContext(CreateContextTypeValue.SMB2_CREATE_APP_INSTANCE_ID, this.info.F_AppInstanceId);
            AddCreateContext(
                CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_REQUEST,
                (info.F_HandleV1_BatchOplock == DetectResult.Supported || info.F_HandleV1_LeaseV1 == DetectResult.Supported) ?
                DetectResult.Supported : DetectResult.UnSupported);
            AddCreateContext(
                CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_RECONNECT,
                (info.F_HandleV1_BatchOplock == DetectResult.Supported || info.F_HandleV1_LeaseV1 == DetectResult.Supported) ?
                DetectResult.Supported : DetectResult.UnSupported);
            AddCreateContext(
                CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_REQUEST_V2,
                (info.F_HandleV2_BatchOplock == DetectResult.Supported
                || info.F_HandleV2_LeaseV1 == DetectResult.Supported
                || info.F_HandleV2_LeaseV2 == DetectResult.Supported) ?
                DetectResult.Supported : DetectResult.UnSupported);
            AddCreateContext(
                CreateContextTypeValue.SMB2_CREATE_DURABLE_HANDLE_RECONNECT_V2,
                (info.F_HandleV2_BatchOplock == DetectResult.Supported
                || info.F_HandleV2_LeaseV1 == DetectResult.Supported
                || info.F_HandleV2_LeaseV2 == DetectResult.Supported) ?
                DetectResult.Supported : DetectResult.UnSupported);
            AddCreateContext(CreateContextTypeValue.SMB2_CREATE_REQUEST_LEASE, this.info.F_Leasing_V1);
            AddCreateContext(CreateContextTypeValue.SMB2_CREATE_REQUEST_LEASE_V2, this.info.F_Leasing_V2);

            // Add/Updata detected RSVD/SQOS version
            AddRSVD(info);
            AddSQOS(info);

            //Bind the data to the control
            resultItemMapList.Add(dialectsItems);
            resultItemMapList.Add(capabilitiesItems);
            resultItemMapList.Add(compressionItems);
            resultItemMapList.Add(encryptionAlgorithmItems);
            resultItemMapList.Add(ioctlCodesItems);
            resultItemMapList.Add(createContextsItems);
            resultItemMapList.Add(rsvdItems);
            resultItemMapList.Add(sqosItems);

            return resultItemMapList;
        }

        #region Properties

        private DetectionInfo info = null;

        private const string dialectsDescription = "\"Max Supported Dialect\" is the selected one in the Negotiate Response by server when a Negotiate Request is sent to SUT with Dialects Smb2.002, Smb2.1, Smb3.0 and Smb3.02.";
        private const string capabilitiesDescription = "\"Capabilities\" are found supported or not supported by analyzing the flags set in Negotiate Response when a Negotiate Request is sent to SUT with all defined flags in TD set in Capabilities field.";
        private const string ioctlCodesDescription = "\"IoCtl Codes\" are found supported or not supported by analyzing IOCTL Responses when the following IOCTL Requests are sent to SUT.";
        private const string createContextsDescription = "\"Creat Contexts\" are found supported or not supported by analyzing Create Responses when the Create Requests with the following create contexts are sent to SUT.";
        private const string rsvdDescription = "\"RSVD Implementation\" is detected by sending Create Request with SVHDX_OPEN_DEVICE_CONTEXT\\SVHDX_OPEN_DEVICE_CONTEXT_V2.";
        private const string sqosDescription = "\"SQOS Implementation\" is detected by sending SQOS get status request.";
        private const string compressionDescription = "\"Supported SMB2 compression algorithms\" is detected by sending NEGOTIATE request with compression negotiate context when SMB2 dialect is greater than 3.1.1.";
        private const string encryptionAlgorithmsDescription = "\"Supported SMB2 encryption algorithms\" is detected by sending NEGOTIATE request with encryption negotiate context when SMB2 dialect is greater than 3.1.1";

        private ResultItemMap dialectsItems = new ResultItemMap() { Header = "Max Smb Version Supported", Description = dialectsDescription };
        private ResultItemMap capabilitiesItems = new ResultItemMap() { Header = "Capabilities", Description = capabilitiesDescription };
        private ResultItemMap ioctlCodesItems = new ResultItemMap() { Header = "IoCtl Codes", Description = ioctlCodesDescription };
        private ResultItemMap createContextsItems = new ResultItemMap() { Header = "Create Contexts", Description = createContextsDescription };

        private ResultItemMap rsvdItems = new ResultItemMap() { Header = "Remote Shared Virtual Disk (RSVD)", Description = rsvdDescription };
        private ResultItemMap sqosItems = new ResultItemMap() { Header = "Storage Quality of Service (SQOS)", Description = sqosDescription };

        private ResultItemMap compressionItems = new ResultItemMap() { Header = "SMB2 Compression Feature", Description = compressionDescription };

        private ResultItemMap encryptionAlgorithmItems = new ResultItemMap() { Header = "SMB2 Encryption Algorithms", Description = encryptionAlgorithmsDescription };

        private List<ResultItemMap> resultItemMapList = new List<ResultItemMap>();

        #endregion

        #region Private functions

        private void AddDialect(DialectRevision dialect)
        {
            string maxSmbVersionSupported = string.Empty;
            switch (dialect)
            {
                case DialectRevision.Smb2002:
                    maxSmbVersionSupported = "Smb 2.002";
                    break;
                case DialectRevision.Smb21:
                    maxSmbVersionSupported = "Smb 2.1";
                    break;
                case DialectRevision.Smb30:
                    maxSmbVersionSupported = "Smb 3.0";
                    break;
                case DialectRevision.Smb302:
                    maxSmbVersionSupported = "Smb 3.02";
                    break;
                case DialectRevision.Smb311:
                    maxSmbVersionSupported = "Smb 3.1.1";
                    break;
                case DialectRevision.Smb2Wildcard:
                    break;
                case DialectRevision.Smb2Unknown:
                    break;
                default:
                    break;
            }
            AddResultItem(ref this.dialectsItems, maxSmbVersionSupported, DetectResult.Supported);
        }

        private void AddCapability(Capabilities_Values capabilityName, string featureName)
        {
            if (info.CheckHigherDialect(info.smb2Info.MaxSupportedDialectRevision, DialectRevision.Smb311) && capabilityName == Capabilities_Values.GLOBAL_CAP_ENCRYPTION)
            {
                AddResultItem(ref this.capabilitiesItems, featureName, info.smb2Info.SelectedCipherID > EncryptionAlgorithm.ENCRYPTION_NONE ? DetectResult.Supported : DetectResult.UnSupported);
            }
            else
            {
                AddResultItem(ref this.capabilitiesItems, featureName, this.info.smb2Info.SupportedCapabilities.HasFlag(capabilityName) ? DetectResult.Supported : DetectResult.UnSupported);
            }
        }

        private void AddIoctlCode(CtlCode_Values value, DetectResult result)
        {
            AddResultItem(ref this.ioctlCodesItems, value.ToString(), result);
        }

        private void AddCreateContext(CreateContextTypeValue value, DetectResult result)
        {
            AddResultItem(ref this.createContextsItems, value.ToString(), result);
        }

        private void AddResultItem(ref ResultItemMap resultItemMap, string value, DetectResult result)
        {
            string imagePath = string.Empty;
            switch (result)
            {
                case DetectResult.Supported:
                    imagePath = "/FileServerPlugin;component/Icons/supported.png";
                    break;
                case DetectResult.UnSupported:
                    imagePath = "/FileServerPlugin;component/Icons/unsupported.png";
                    break;
                case DetectResult.DetectFail:
                    imagePath = "/FileServerPlugin;component/Icons/undetected.png";
                    break;
                default:
                    break;
            }

            ResultItem item = new ResultItem() { DetectedResult = result, ImageUrl = imagePath, Name = value };
            resultItemMap.ResultItemList.Add(item);
        }

        private void AddRSVD(DetectionInfo info)
        {
            if (info.RsvdSupport == DetectResult.Supported)
            {
                if (info.RsvdVersion == RSVD_PROTOCOL_VERSION.RSVD_PROTOCOL_VERSION_1)
                {
                    AddResultItem(ref this.rsvdItems, "RSVD Protocol version 1", DetectResult.Supported);
                    AddResultItem(ref this.rsvdItems, "RSVD Protocol version 2", DetectResult.UnSupported);
                }
                else
                {
                    AddResultItem(ref this.rsvdItems, "RSVD Protocol version 1", DetectResult.Supported);
                    AddResultItem(ref this.rsvdItems, "RSVD Protocol version 2", DetectResult.Supported);
                }
            }
            // DetectResult.UnSupported and DetectResult.DetectFail
            else
            {
                AddResultItem(ref this.rsvdItems, "RSVD", info.RsvdSupport);
            }
        }

        private void AddSQOS(DetectionInfo info)
        {
            if (info.SqosSupport == DetectResult.Supported)
            {
                if (info.SqosVersion == SQOS_PROTOCOL_VERSION.Sqos10)
                {
                    AddResultItem(ref this.sqosItems, "SQOS dialect 1.0", DetectResult.Supported);
                    AddResultItem(ref this.sqosItems, "SQOS dialect 1.1", DetectResult.UnSupported);
                }
                else
                {
                    AddResultItem(ref this.sqosItems, "SQOS dialect 1.0", DetectResult.Supported);
                    AddResultItem(ref this.sqosItems, "SQOS dialect 1.1", DetectResult.Supported);
                }
            }
            // DetectResult.UnSupported and DetectResult.DetectFail
            else
            {
                AddResultItem(ref this.sqosItems, "SQOS", info.SqosSupport);
            }
        }

        private void AddCompressionCapabilities(DetectionInfo info)
        {
            var excludedCompressionAlogrithms = new CompressionAlgorithm[]
            {
                CompressionAlgorithm.NONE,
                CompressionAlgorithm.Unsupported,
            };

            var possibleCompressionAlogrithms = Enum.GetValues(typeof(CompressionAlgorithm)).Cast<CompressionAlgorithm>().Except(excludedCompressionAlogrithms);

            foreach (var compressionAlgorithm in possibleCompressionAlogrithms)
            {
                if (info.smb2Info.SupportedCompressionAlgorithms.Contains(compressionAlgorithm))
                {
                    AddResultItem(ref this.compressionItems, compressionAlgorithm.ToString(), DetectResult.Supported);
                }
                else
                {
                    AddResultItem(ref this.compressionItems, compressionAlgorithm.ToString(), DetectResult.UnSupported);
                }
            }

            var chainedCompressionResult = info.smb2Info.IsChainedCompressionSupported ? DetectResult.Supported : DetectResult.UnSupported;

            AddResultItem(ref this.compressionItems, "Chained compression", chainedCompressionResult);
        }

        private void AddSMB2EncryptionAlgorithms(DetectionInfo info)
        {
            var excludedEncryptionAlogrithms = new EncryptionAlgorithm[]
            {
                EncryptionAlgorithm.ENCRYPTION_NONE,
                EncryptionAlgorithm.ENCRYPTION_INVALID,
            };

            var possibleEncryptionAlogrithms = Enum.GetValues(typeof(EncryptionAlgorithm)).Cast<EncryptionAlgorithm>().Except(excludedEncryptionAlogrithms);

            foreach (var encryptionAlgorithm in possibleEncryptionAlogrithms)
            {
                if (info.smb2Info.SutSupportedEncryptionAlgorithms.Contains(encryptionAlgorithm))
                {
                    AddResultItem(ref this.encryptionAlgorithmItems, encryptionAlgorithm.ToString(), DetectResult.Supported);
                }
                else
                {
                    AddResultItem(ref this.encryptionAlgorithmItems, encryptionAlgorithm.ToString(), DetectResult.UnSupported);
                }
            }
        }
        #endregion
    }
}
