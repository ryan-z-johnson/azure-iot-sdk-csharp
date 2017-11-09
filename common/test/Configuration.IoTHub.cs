// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Azure.Devices.E2ETests
{
    public static partial class Configuration
    {
        public static partial class IoTHub
        {
            // TODO: Revert to IOTHUB_CONN_STRING_CSHARP for IoT Edge public preview
            public static string ConnectionString => GetValue("IOTHUB_CONN_STRING_CSHARP_WITH_MODULES"); // GetValue("IOTHUB_CONN_STRING_CSHARP");

            // TODO: Revert to (IOTHUB_EVENTHUB_CONSUMER_GROUP, $Default) for IoT Edge public preview
            public static string ConsumerGroup => GetValue("$Default"); // GetValue("IOTHUB_EVENTHUB_CONSUMER_GROUP", "$Default");

            public static X509Certificate2 GetCertificateWithPrivateKey() 
                => GetBase64EncodedCertificate("IOTHUB_X509_PFX_CERTIFICATE");
        }
    }
}
