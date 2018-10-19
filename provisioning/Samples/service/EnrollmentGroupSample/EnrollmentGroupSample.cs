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
    public class EnrollmentGroupSample
    {
        private const string EnrollmentGroupId = "enrollmentgrouptest";
        ProvisioningServiceClient _provisioningServiceClient;
        X509Certificate2 _groupIssuerCertificate;
		private readonly string X509AttestationMechanism = "x509";
		private const string IotHubHostName = "my-iothub-hostname";

        private readonly string OperationCreate = "create";
        private readonly string OperationUpdate = "update";
        private readonly string OperationDelete = "delete";

        public EnrollmentGroupSample(ProvisioningServiceClient provisioningServiceClient, X509Certificate2 groupIssuerCertificate)
        {
            _provisioningServiceClient = provisioningServiceClient;
            _groupIssuerCertificate = groupIssuerCertificate;
        }

        public async Task RunSampleAsync()
        {
            await QueryEnrollmentGroupAsync().ConfigureAwait(false);

            List<EnrollmentGroupRequest> enrollments = await CreateEnrollmentGroupAsync().ConfigureAwait(false);
            await UpdateEnrollmentGroupAsync(enrollments).ConfigureAwait(false);
            await DeleteEnrollmentGroupAsync(enrollments).ConfigureAwait(false);
        }

        public async Task QueryEnrollmentGroupAsync()
        {
            Console.WriteLine("\nCreating a query for enrollmentGroups...");
            QuerySpecification querySpecification = new QuerySpecification("SELECT * FROM enrollmentGroups");
            IList<EnrollmentGroupResponse> queryResult = await _provisioningServiceClient.QueryEnrollmentGroupsAsync(querySpecification).ConfigureAwait(false);
            foreach (EnrollmentGroupResponse enrollmentGroup in queryResult)
            {
                Console.WriteLine(JsonConvert.SerializeObject(enrollmentGroup, Formatting.Indented));
                await EnumerateRegistrationsInGroup(enrollmentGroup).ConfigureAwait(false);
            }
        }

        private async Task EnumerateRegistrationsInGroup(EnrollmentGroupResponse group)
        {
            Console.WriteLine($"\nCreating a query for registrations within group '{group.EnrollmentGroupId}'...");
            IList<DeviceRegistrationState> deviceRegistrationStates = await _provisioningServiceClient.QueryDeviceRegistrationStatesAsync(group.EnrollmentGroupId).ConfigureAwait(false);
            foreach (DeviceRegistrationState deviceRegistrationState in deviceRegistrationStates)
            {
                Console.WriteLine(JsonConvert.SerializeObject(deviceRegistrationState, Formatting.Indented));
            }
        }

        public async Task<List<EnrollmentGroupRequest>> CreateEnrollmentGroupAsync()
        {
            Console.WriteLine("\nCreating a new enrollmentGroup...");
            var attestation = new X509AttestationRequest(
                signingCertificates: new X509CertificatesRequest(
                    new X509CertificateWithInfoRequest(Convert.ToBase64String(_groupIssuerCertificate.Export(X509ContentType.Cert)))
                ));
            var attestationMechanism = new AttestationMechanismRequest(X509AttestationMechanism, x509: attestation);
            var enrollmentGroup =
                    new EnrollmentGroupRequest(
                            EnrollmentGroupId,
                            attestationMechanism);
            enrollmentGroup.IotHubHostName = IotHubHostName;        // This is mandatory if the DPS Allocation Policy is "Static"
            Console.WriteLine(JsonConvert.SerializeObject(enrollmentGroup, Formatting.Indented));

            List<EnrollmentGroupRequest> enrollmentGroups = new List<EnrollmentGroupRequest>() { enrollmentGroup };
            EnrollmentGroupOperation enrollmentGroupOperation = new EnrollmentGroupOperation(enrollmentGroups, OperationCreate);
            Console.WriteLine("\nRunning the operation to create the Enrollment Group...");
            EnrollmentOperationResult enrollmentGroupOperationResult =
                await _provisioningServiceClient.RunEnrollmentGroupsOperationAsync(enrollmentGroupOperation).ConfigureAwait(false);
            Console.WriteLine("\nResult of the Create enrollment...");
            Console.WriteLine(enrollmentGroupOperationResult.IsSuccessful ? "Succeeded" : "Failed");

            return enrollmentGroups;
        }

        public async Task UpdateEnrollmentGroupAsync(List<EnrollmentGroupRequest> enrollmentGroups)
        {
            var updatedEnrollments = new List<EnrollmentGroupResponse>();
            foreach (EnrollmentGroupRequest enrollmentGroup in enrollmentGroups)
            {
                String groupId = enrollmentGroup.EnrollmentGroupId;
                Console.WriteLine($"\nGetting the {nameof(enrollmentGroup)} information for {groupId}...");
                EnrollmentGroupResponse enrollment =
                    await _provisioningServiceClient.GetEnrollmentGroupAsync(groupId).ConfigureAwait(false);
                enrollment.InitialTwin = new InitialTwin(
                null,
                new InitialTwinProperties(
                    new TwinCollection(
                        new Dictionary<string, object>()
                        {
                            { "Brand", "Contoso" }
                        })));
                updatedEnrollments.Add(enrollment);
            }

            var enrollmentGroupOperation = new EnrollmentGroupOperation(updatedEnrollments, OperationUpdate);
            Console.WriteLine("\nRunning the operation to update the Enrollment Group...");
            EnrollmentOperationResult enrollmentGroupOperationResult =
                await _provisioningServiceClient.RunEnrollmentGroupsOperationAsync(enrollmentGroupOperation).ConfigureAwait(false);
            Console.Write("\nResult of the Update enrollment...");
            Console.WriteLine(enrollmentGroupOperationResult.IsSuccessful ? "Succeeded" : "Failed");
        }

        public async Task DeleteEnrollmentGroupAsync(List<EnrollmentGroupRequest> enrollmentGroups)
        {
            var enrollmentGroupsOperation = new EnrollmentGroupOperation(enrollmentGroups, OperationDelete);
            Console.WriteLine("\nRunning the operation to delete the Enrollment Groups...");
            EnrollmentOperationResult enrollmentGroupsOperationResult =
                await _provisioningServiceClient.RunEnrollmentGroupsOperationAsync(enrollmentGroupsOperation).ConfigureAwait(false);
            Console.Write("\nResult of the Delete enrollment...");
            Console.WriteLine(enrollmentGroupsOperationResult.IsSuccessful ? "Succeeded" : "Failed");
        }
    }
}
