// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Provisioning.Service.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Provisioning.Service.Samples
{
    using DeviceCapabilities = Microsoft.Azure.Devices.Provisioning.Service.Models.DeviceCapabilities;
    public class EnrollmentSample
    {
        private const string RegistrationId = "myvalid-registratioid-csharp";
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

		private readonly string TpmAttestationType = "tpm";
		private const string IotHubHostName = "my-iothub-hostname";
        ProvisioningServiceClient _provisioningServiceClient;

        private readonly string OperationCreate = "create";
        private readonly string OperationUpdate = "update";
        private readonly string OperationDelete = "delete";

        public EnrollmentSample(ProvisioningServiceClient provisioningServiceClient)
        {
            _provisioningServiceClient = provisioningServiceClient;
        }

        public async Task RunSampleAsync()
        {
            await QueryIndividualEnrollmentsAsync().ConfigureAwait(false);

            List<IndividualEnrollment> enrollments = await CreateIndividualEnrollmentTpmAsync().ConfigureAwait(false);
            await UpdateIndividualEnrollmentAsync(enrollments).ConfigureAwait(false);
            await DeleteIndividualEnrollmentAsync(enrollments).ConfigureAwait(false);            
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

        public async Task<List<IndividualEnrollment>> CreateIndividualEnrollmentTpmAsync()
        {
            Console.WriteLine("\nCreating a new individualEnrollment...");
            TpmAttestation attestation = new TpmAttestation(TpmEndorsementKey);
            AttestationMechanism attestationMechanism = new AttestationMechanism(TpmAttestationType, attestation);
            IndividualEnrollment individualEnrollment =
                    new IndividualEnrollment(
                            RegistrationId,
                            attestationMechanism);

            // The following parameters are optional:
            individualEnrollment.DeviceId = OptionalDeviceId;
            individualEnrollment.ProvisioningStatus = OptionalProvisioningStatus;
            IDictionary<string, object> pros = new Dictionary<string, object>() { { "Brand", "Contoso"} };
            individualEnrollment.InitialTwin = new InitialTwin(
                null,
                new InitialTwinProperties(
                    new Models.TwinCollection(
                        new Dictionary<string, object>() {
                            { "Brand", "Contoso" },
                            { "Model", "SSC4" },
                            { "Color", "White" }
                        })
                    ));
            individualEnrollment.Capabilities = OptionalEdgeCapabilityEnabled;
            individualEnrollment.IotHubHostName = IotHubHostName;       // This is mandatory if the DPS Allocation Policy is "Static"

            List<IndividualEnrollment> individualEnrollments = new List<IndividualEnrollment>() { individualEnrollment };
            IndividualEnrollmentOperation individualnrollmentOperation = new IndividualEnrollmentOperation(individualEnrollments, OperationCreate);
            Console.WriteLine("\nRunning the operation to create the individualEnrollments...");
            EnrollmentOperationResult individualEnrollmentOperationResult =
                await _provisioningServiceClient.RunIndividualEnrollmentOperationAsync(individualnrollmentOperation).ConfigureAwait(false);
            Console.WriteLine("\nResult of the Create enrollment...");
            Console.WriteLine(individualEnrollmentOperationResult.IsSuccessful ? "Succeeded" : "Failed");

            return individualEnrollments;
        }

        public async Task UpdateIndividualEnrollmentAsync(List<IndividualEnrollment> individualEnrollments)
        {
            List<IndividualEnrollment> updatedEnrollments = new List<IndividualEnrollment>();
            foreach (IndividualEnrollment individualEnrollment in individualEnrollments)
            {
                String registrationId = individualEnrollment.RegistrationId;
                Console.WriteLine($"\nGetting the {nameof(individualEnrollment)} information for {registrationId}...");
                IndividualEnrollment enrollment =
                    await _provisioningServiceClient.GetIndividualEnrollmentAsync(registrationId).ConfigureAwait(false);
                enrollment.DeviceId = "updated_the_device_id";
                updatedEnrollments.Add(enrollment);
            }

            IndividualEnrollmentOperation individualnrollmentOperation = new IndividualEnrollmentOperation(updatedEnrollments, OperationUpdate);
            Console.WriteLine("\nRunning the operation to update the individualEnrollments...");
            EnrollmentOperationResult individualEnrollmentOperationResult =
                await _provisioningServiceClient.RunIndividualEnrollmentOperationAsync(individualnrollmentOperation).ConfigureAwait(false);
            Console.Write("\nResult of the Update enrollment...");
            Console.WriteLine(individualEnrollmentOperationResult.IsSuccessful ? "Succeeded" : "Failed");
        }

        public async Task DeleteIndividualEnrollmentAsync(List<IndividualEnrollment> individualEnrollments)
        {
            IndividualEnrollmentOperation individualnrollmentOperation = new IndividualEnrollmentOperation(individualEnrollments, OperationDelete);
            Console.WriteLine("\nRunning the operation to delete the individualEnrollments...");
            EnrollmentOperationResult individualEnrollmentOperationResult =
                await _provisioningServiceClient.RunIndividualEnrollmentOperationAsync(individualnrollmentOperation).ConfigureAwait(false);
            Console.Write("\nResult of the Delete enrollment...");
            Console.WriteLine(individualEnrollmentOperationResult.IsSuccessful ? "Succeeded" : "Failed");
        }
    }
}
