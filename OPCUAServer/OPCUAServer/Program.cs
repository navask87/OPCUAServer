/******************************************************************************
** Copyright (c) 2006-2023 Unified Automation GmbH All rights reserved.
**
** Software License Agreement ("SLA") Version 2.8
**
** Unless explicitly acquired and licensed from Licensor under another
** license, the contents of this file are subject to the Software License
** Agreement ("SLA") Version 2.8, or subsequent versions
** as allowed by the SLA, and You may not copy or use this file in either
** source code or executable form, except in compliance with the terms and
** conditions of the SLA.
**
** All software distributed under the SLA is provided strictly on an
** "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED,
** AND LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
** LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
** PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the SLA for specific
** language governing rights and limitations under the SLA.
**
** Project: .NET based OPC UA Client Server SDK
**
** Description: OPC Unified Architecture Software Development Kit.
**
** The complete license agreement can be found here:
** http://unifiedautomation.com/License/SLA/2.8/
******************************************************************************/

using System;
using OPCUAServer.SE.Resources;
using SE.OPCUAServer;
using SE.OPCUAServer.Constants;
using UnifiedAutomation.UaBase;
namespace SE.OPCUAServer
{
    partial class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // The license file must be loaded from an embedded resource.
                ApplicationLicenseManager.AddProcessLicenses(typeof(Program).Assembly, OCUAServerConstants.LicenseFile);

                // Start the server.
                Console.WriteLine(StringResource.StartingServer);
                OPCUAServerManager server = new OPCUAServerManager();

#if NETFRAMEWORK
                ApplicationInstance application = ApplicationInstance.Default;
                // Blocks the server from automatically start as service in non user-interactive environments.
                application.NoAutoStartService = true;
#else
                ApplicationInstanceBase application = ApplicationInstanceBase.Default;
                ConfigureOpcUaApplicationFromCode();
                application.SecurityProvider = new BouncyCastleSecurityProvider();
#endif

                application.UntrustedCertificate += Application_UntrustedCertificate;


                application.AutoCreateCertificate = true;
                application.Start(server, null, server);
                
                PrintEndpoints(server);

                // Block until the server exits.
                Console.WriteLine(StringResource.ExitProgramMessage);
                Console.ReadLine();

                // Stop the server.
                Console.WriteLine(StringResource.StoppingServer);
                server.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(StringResource.Error, e.Message);
                Console.WriteLine(StringResource.ExitProgramMessage);
                Console.ReadLine();
            }
        }

        private static void Application_UntrustedCertificate(object sender, UntrustedCertificateEventArgs e)
        {
            Console.WriteLine(StringResource.UntrustedCertificate, e.Certificate.CommonName, e.Certificate.SubjectName);
            Console.WriteLine(StringResource.MoveCertificate,
                e.Certificate.CommonName,
                e.Certificate.Thumbprint,
                e.Application.RejectedStore.StorePath,
                e.Application.TrustedStore.StorePath);
        }

        /// <summary>
        /// Prints the available EndpointDescriptions to the command line.
        /// </summary>
        /// <param name="server"></param>
        static void PrintEndpoints(OPCUAServerManager server)
        {
            // print the endpoints.
            Console.WriteLine(string.Empty);
            Console.WriteLine(StringResource.ListeningAtEndPoints);

            foreach (EndpointDescription endpoint in server.Application.Endpoints)
            {
                StatusCode error = server.Application.GetEndpointStatus(endpoint);
                Console.WriteLine(StringResource.Status, endpoint, error.ToString(true));
            }

            Console.WriteLine(string.Empty);
        }
    }
}
