// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Provisioning.Service.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Provisioning.Service.Samples
{
    using DeviceCapabilities = Microsoft.Azure.Devices.Provisioning.Service.Models.DeviceCapabilities;
    public class EnrollmentSample
    {
        private const string RegistrationIdTpm = "myvalid-registratioid-csharp-Tpm";
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

        private const string RegistrationIdX509 = "myvalid-registratioid-csharp-x509";
        X509Certificate2 _clientCertificate;

		private const string IotHubHostName = "my-iothub-hostname";
        ProvisioningServiceClient _provisioningServiceClient;

        private readonly string OperationCreate = "create";
        private readonly string OperationUpdate = "update";
        private readonly string OperationDelete = "delete";

        public EnrollmentSample(ProvisioningServiceClient provisioningServiceClient, X509Certificate2 clientCertificate)
        {
            _provisioningServiceClient = provisioningServiceClient;
            _clientCertificate = clientCertificate;
        }

        public async Task RunSampleAsync()
        {
            await QueryIndividualEnrollmentsAsync().ConfigureAwait(false);

            // COMMENT: this does not look good - create returns the request object
            List<IndividualEnrollmentRequest> enrollmentRequestTpm = await CreateIndividualEnrollmentTpmAsync().ConfigureAwait(false);
            List<IndividualEnrollmentRequest> enrollmentRequestX509 = await CreateIndividualEnrollmentX509Async().ConfigureAwait(false);
            await UpdateIndividualEnrollmentAsync(enrollmentRequestTpm).ConfigureAwait(false);
            await DeleteIndividualEnrollmentAsync(enrollmentRequestTpm).ConfigureAwait(false);            
        }

        public async Task QueryIndividualEnrollmentsAsync()
        {
            Console.WriteLine("\nCreating a query for enrollments...");
            QuerySpecification querySpecification = new QuerySpecification("SELECT * FROM enrollments");

            IList<IndividualEnrollmentResponse> queryResult = await _provisioningServiceClient.QueryIndividualEnrollmentsAsync(querySpecification).ConfigureAwait(false);
            foreach (IndividualEnrollmentResponse individualEnrollment in queryResult)
            {
                Console.WriteLine(JsonConvert.SerializeObject(individualEnrollment, Formatting.Indented));
            }
        }

        public async Task<List<IndividualEnrollmentRequest>> CreateIndividualEnrollmentTpmAsync()
        {
            Console.WriteLine("\nCreating a new TPM individualEnrollment...");
            TpmAttestation attestation = new TpmAttestation(TpmEndorsementKey);
            var tpmEnrollmentRequest = new TpmIndividualEnrollmentRequest(RegistrationIdTpm, attestation);

            // The following parameters are optional:
            tpmEnrollmentRequest.DeviceId = OptionalDeviceId;
            tpmEnrollmentRequest.ProvisioningStatus = OptionalProvisioningStatus;
            IDictionary<string, object> pros = new Dictionary<string, object>() { { "Brand", "Contoso"} };
            tpmEnrollmentRequest.InitialTwin = new InitialTwin(
                null,
                new InitialTwinProperties(
                    new Models.TwinCollection(
                        new Dictionary<string, object>() {
                            { "Brand", "Contoso" },
                            { "Model", "SSC4" },
                            { "Color", "White" }
                        })
                    ));
            tpmEnrollmentRequest.Capabilities = OptionalEdgeCapabilityEnabled;
            tpmEnrollmentRequest.IotHubHostName = IotHubHostName;       // This is mandatory if the DPS Allocation Policy is "Static"

            var individualEnrollments = new List<IndividualEnrollmentRequest>() { tpmEnrollmentRequest };
            IndividualEnrollmentOperation individualnrollmentOperation = new IndividualEnrollmentOperation(individualEnrollments, OperationCreate);
            Console.WriteLine("\nRunning the operation to create the individualEnrollments...");
            EnrollmentOperationResult individualEnrollmentOperationResult =
                await _provisioningServiceClient.RunIndividualEnrollmentOperationAsync(individualnrollmentOperation).ConfigureAwait(false);
            Console.WriteLine("\nResult of the Create enrollment...");
            Console.WriteLine(individualEnrollmentOperationResult.IsSuccessful ? "Succeeded" : "Failed");

            return individualEnrollments;
        }

        public async Task<List<IndividualEnrollmentRequest>> CreateIndividualEnrollmentX509Async()
        {
            Console.WriteLine("\nCreating a new X509 individualEnrollment...");
            var x509Certificate = new X509CertificatesRequest(
                primary: new X509CertificateWithInfoRequest(Convert.ToBase64String(_clientCertificate.Export(X509ContentType.Cert))));

            var x509EnrollmentRequest = new X509CertificateIndividualEnrollmentRequest(RegistrationIdX509, x509Certificate);

            // The following parameters are optional:
            x509EnrollmentRequest.DeviceId = OptionalDeviceId;
            x509EnrollmentRequest.ProvisioningStatus = OptionalProvisioningStatus;
            IDictionary<string, object> pros = new Dictionary<string, object>() { { "Brand", "Contoso" } };
            x509EnrollmentRequest.InitialTwin = new InitialTwin(
                null,
                new InitialTwinProperties(
                    new Models.TwinCollection(
                        new Dictionary<string, object>() {
                            { "Brand", "Contoso" },
                            { "Model", "SSC4" },
                            { "Color", "White" }
                        })
                    ));
            x509EnrollmentRequest.Capabilities = OptionalEdgeCapabilityEnabled;
            x509EnrollmentRequest.IotHubHostName = IotHubHostName;       // This is mandatory if the DPS Allocation Policy is "Static"

            var individualEnrollments = new List<IndividualEnrollmentRequest>() { x509EnrollmentRequest };
            IndividualEnrollmentOperation individualnrollmentOperation = new IndividualEnrollmentOperation(individualEnrollments, OperationCreate);
            Console.WriteLine("\nRunning the operation to create the individualEnrollments...");
            EnrollmentOperationResult individualEnrollmentOperationResult =
                await _provisioningServiceClient.RunIndividualEnrollmentOperationAsync(individualnrollmentOperation).ConfigureAwait(false);
            Console.WriteLine("\nResult of the Create enrollment...");
            Console.WriteLine(individualEnrollmentOperationResult.IsSuccessful ? "Succeeded" : "Failed");

            return individualEnrollments;
        }

        public async Task UpdateIndividualEnrollmentAsync(List<IndividualEnrollmentRequest> individualEnrollments)
        {
            var updatedEnrollments = new List<IndividualEnrollmentRequest>();
            foreach (IndividualEnrollmentRequest individualEnrollment in individualEnrollments)
            {
                String registrationId = individualEnrollment.RegistrationId;
                Console.WriteLine($"\nGetting the {nameof(individualEnrollment)} information for {registrationId}...");
                IndividualEnrollmentResponse enrollment =
                    await _provisioningServiceClient.GetIndividualEnrollmentAsync(registrationId).ConfigureAwait(false);

                // COMMENT: Need to copy all properties over??? Can do selective update of only required fields???
                var enrollmentRequest = new IndividualEnrollmentRequest();
                enrollmentRequest.RegistrationId = enrollment.RegistrationId;
                enrollmentRequest.Etag = enrollment.Etag;
                enrollmentRequest.DeviceId = "updated_the_device_id";
                updatedEnrollments.Add(enrollmentRequest);
            }

            IndividualEnrollmentOperation individualnrollmentOperation = new IndividualEnrollmentOperation(updatedEnrollments, OperationUpdate);
            Console.WriteLine("\nRunning the operation to update the individualEnrollments...");
            EnrollmentOperationResult individualEnrollmentOperationResult =
                await _provisioningServiceClient.RunIndividualEnrollmentOperationAsync(individualnrollmentOperation).ConfigureAwait(false);
            Console.Write("\nResult of the Update enrollment...");
            Console.WriteLine(individualEnrollmentOperationResult.IsSuccessful ? "Succeeded" : "Failed");
        }

        public async Task DeleteIndividualEnrollmentAsync(List<IndividualEnrollmentRequest> individualEnrollments)
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
