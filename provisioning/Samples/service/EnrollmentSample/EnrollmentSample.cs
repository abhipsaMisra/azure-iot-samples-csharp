﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Provisioning.Service.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Provisioning.Service.Samples
{
    public class EnrollmentSample
    {
        private const string RegistrationIdTpm = "myvalid-registratioid-csharp-tpm";
        private const string RegistrationIdX509 = "myvalid-registratioid-csharp-x509";
        private const string TpmEndorsementKey =
            "AToAAQALAAMAsgAgg3GXZ0SEs/gakMyNRqXXJP1S124GUgtk8qHaGzMUaaoABgCAAEMAEAgAAAAAAAEAxsj2gUS" +
            "cTk1UjuioeTlfGYZrrimExB+bScH75adUMRIi2UOMxG1kw4y+9RW/IVoMl4e620VxZad0ARX2gUqVjYO7KPVt3d" +
            "yKhZS3dkcvfBisBhP1XH9B33VqHG9SHnbnQXdBUaCgKAfxome8UmBKfe+naTsE5fkvjb/do3/dD6l4sGBwFCnKR" +
            "dln4XpM03zLpoHFao8zOwt8l/uP3qUIxmCYv9A7m69Ms+5/pCkTu/rK4mRDsfhZ0QLfbzVI6zQFOKF/rwsfBtFe" +
            "WlWtcuJMKlXdD8TXWElTzgh7JS4qhFzreL0c1mI0GCj+Aws0usZh7dLIVPnlgZcBhgy1SSDQMQ==";

        // Optional parameters
        private const string OptionalDeviceId = "iothubtpmdevice1";
        private const string OptionalProvisioningStatus = "enabled";
        private DeviceCapabilities OptionalEdgeCapabilityEnabled = new DeviceCapabilities {IotEdge = true };
        private DeviceCapabilities OptionalEdgeCapabilityDisabled = new DeviceCapabilities { IotEdge = false };

        private readonly string OperationCreate = "create";
        private readonly string OperationUpdate = "update";
        private readonly string OperationDelete = "delete";

        private readonly string TpmAttestationType = "tpm";
		private const string IotHubHostName = "my-iothub-hostname";
        ProvisioningServiceClient _provisioningServiceClient;
        X509Certificate2 _individualCertificate;

        public EnrollmentSample(ProvisioningServiceClient provisioningServiceClient, X509Certificate2 individualCertificate)
        {
            _provisioningServiceClient = provisioningServiceClient;
            _individualCertificate = individualCertificate;
        }

        public async Task RunSampleAsync()
        {
            await QueryIndividualEnrollmentsAsync().ConfigureAwait(false);

            await CreateIndividualEnrollmentTpmAsync().ConfigureAwait(false);
            await UpdateIndividualEnrollmentTpmAsync().ConfigureAwait(false);
            await DeleteIndividualEnrollmentTpmAsync().ConfigureAwait(false);            
        }

        public async Task QueryIndividualEnrollmentsAsync()
        {
            Console.WriteLine("\nCreating a query for enrollments...");
            QuerySpecification querySpecification = new QuerySpecification("SELECT * FROM enrollments");

            IList<IndividualEnrollment> queryResult = await _provisioningServiceClient.QueryIndividualEnrollmentsAsync(querySpecification).ConfigureAwait(false);
            foreach (IndividualEnrollment individualEnrollment in queryResult)
            {
                Console.WriteLine(JsonConvert.SerializeObject(individualEnrollment, Formatting.Indented));
            }
        }

        public async Task CreateIndividualEnrollmentTpmAsync()
        {
            Console.WriteLine("\nCreating a new individualEnrollment...");
            TpmAttestation attestation = new TpmAttestation(TpmEndorsementKey);
            AttestationMechanism attestationMechanism = new AttestationMechanism(TpmAttestationType, attestation);
            IndividualEnrollment individualEnrollmentTpm =
                    new IndividualEnrollment(
                            RegistrationIdTpm,
                            attestationMechanism);

            // The following parameters are optional:
            individualEnrollmentTpm.DeviceId = OptionalDeviceId;
            individualEnrollmentTpm.ProvisioningStatus = OptionalProvisioningStatus;
            IDictionary<string, object> pros = new Dictionary<string, object>() { { "Brand", "Contoso"} };
            individualEnrollmentTpm.InitialTwin = new InitialTwin(
                null,
                new InitialTwinProperties(
                    new Models.TwinCollection(
                        new Dictionary<string, object>() {
                            { "Brand", "Contoso" },
                            { "Model", "SSC4" },
                            { "Color", "White" }
                        })
                    ));
            individualEnrollmentTpm.Capabilities = OptionalEdgeCapabilityEnabled;
            individualEnrollmentTpm.IotHubHostName = IotHubHostName;       // This is mandatory if the DPS Allocation Policy is "Static"

            List<IndividualEnrollment> individualEnrollments = new List<IndividualEnrollment>() { individualEnrollmentTpm };
            BulkEnrollmentOperation enrollmentOperation = new BulkEnrollmentOperation(individualEnrollments, OperationCreate);
            Console.WriteLine("\nAdding new individualEnrollment...");
            Console.WriteLine(JsonConvert.SerializeObject(individualEnrollmentTpm, Formatting.Indented));
            BulkEnrollmentOperationResult individualEnrollmentResult =
                await _provisioningServiceClient.RunBulkEnrollmentOperationAsync(enrollmentOperation).ConfigureAwait(false);
            Console.WriteLine(JsonConvert.SerializeObject(individualEnrollmentResult, Formatting.Indented));
        }

        public async Task CreateIndividualEnrollmentX509Async()
        {
            Console.WriteLine("\nCreating a new individualEnrollment...");
            TpmAttestation attestation = new TpmAttestation(TpmEndorsementKey);
            AttestationMechanism attestationMechanism = new AttestationMechanism(TpmAttestationType, attestation);
            IndividualEnrollment individualEnrollmentTpm =
                    new IndividualEnrollment(
                            RegistrationIdTpm,
                            attestationMechanism);

            // The following parameters are optional:
            individualEnrollmentTpm.DeviceId = OptionalDeviceId;
            individualEnrollmentTpm.ProvisioningStatus = OptionalProvisioningStatus;
            IDictionary<string, object> pros = new Dictionary<string, object>() { { "Brand", "Contoso" } };
            individualEnrollmentTpm.InitialTwin = new InitialTwin(
                null,
                new InitialTwinProperties(
                    new Models.TwinCollection(
                        new Dictionary<string, object>() {
                            { "Brand", "Contoso" },
                            { "Model", "SSC4" },
                            { "Color", "White" }
                        })
                    ));
            individualEnrollmentTpm.Capabilities = OptionalEdgeCapabilityEnabled;
            individualEnrollmentTpm.IotHubHostName = IotHubHostName;       // This is mandatory if the DPS Allocation Policy is "Static"

            List<IndividualEnrollment> individualEnrollments = new List<IndividualEnrollment>() { individualEnrollmentTpm };
            BulkEnrollmentOperation enrollmentOperation = new BulkEnrollmentOperation(individualEnrollments, OperationCreate);
            Console.WriteLine("\nAdding new individualEnrollment...");
            Console.WriteLine(JsonConvert.SerializeObject(individualEnrollmentTpm, Formatting.Indented));
            BulkEnrollmentOperationResult individualEnrollmentResult =
                await _provisioningServiceClient.RunBulkEnrollmentOperationAsync(enrollmentOperation).ConfigureAwait(false);
            Console.WriteLine(JsonConvert.SerializeObject(individualEnrollmentResult, Formatting.Indented));
        }

        public async Task<IndividualEnrollment> GetIndividualEnrollmentInfoTpmAsync()
        {
            Console.WriteLine("\nGetting the individualEnrollment information...");
            IndividualEnrollment getResult =
                await _provisioningServiceClient.GetIndividualEnrollmentAsync(RegistrationIdTpm).ConfigureAwait(false);
            Console.WriteLine(JsonConvert.SerializeObject(getResult, Formatting.Indented));

            return getResult;
        }

        public async Task UpdateIndividualEnrollmentTpmAsync()
        {
            var individualEnrollment = await GetIndividualEnrollmentInfoTpmAsync().ConfigureAwait(false);
            individualEnrollment.InitialTwin.Properties.Desired.AdditionalProperties["Color"] = "Yellow";
            individualEnrollment.Capabilities = OptionalEdgeCapabilityDisabled;

            List<IndividualEnrollment> individualEnrollments = new List<IndividualEnrollment>() { individualEnrollment };
            BulkEnrollmentOperation enrollmentOperation = new BulkEnrollmentOperation(individualEnrollments, OperationUpdate);
            BulkEnrollmentOperationResult individualEnrollmentResult =
                await _provisioningServiceClient.RunBulkEnrollmentOperationAsync(enrollmentOperation).ConfigureAwait(false);
            Console.WriteLine(JsonConvert.SerializeObject(individualEnrollmentResult, Formatting.Indented));
        }

        public async Task DeleteIndividualEnrollmentTpmAsync()
        {
            var individualEnrollment = await GetIndividualEnrollmentInfoTpmAsync().ConfigureAwait(false);

            Console.WriteLine("\nDeleting the individualEnrollment...");
            List<IndividualEnrollment> individualEnrollments = new List<IndividualEnrollment>() { individualEnrollment };
            BulkEnrollmentOperation enrollmentOperation = new BulkEnrollmentOperation(individualEnrollments, OperationDelete);
            await _provisioningServiceClient.DeleteIndividualEnrollmentAsync(RegistrationIdTpm).ConfigureAwait(false);
        }
    }
}
