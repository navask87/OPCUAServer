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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OPCUAServer.SE.Resources;
using UnifiedAutomation.UaBase;
using UnifiedAutomation.UaServer;

namespace SE.OPCUAServer
{
    /// <summary>
    /// The OPCUAServerManager class is responsible for managing the lifecycle of the OPC UA server.
    /// It extends the ServerManager class provided by the Unified Automation SDK.
    /// 
    /// This class overrides the OnRootNodeManagerStarted method to initialize and start custom
    /// node managers, such as the ControllerNodeManager, which handle specific sets of nodes
    /// within the OPC UA server's address space.
    /// 
    /// Usage:
    /// - This class is instantiated and used as part of the server initialization process.
    /// - It ensures that all required node managers are properly created and started.
    /// 
    /// Key Responsibilities:
    /// - Managing the root node manager lifecycle.
    /// - Creating and starting custom node managers for specific functionalities.
    /// 
    /// Dependencies:
    /// - UnifiedAutomation.UaBase
    /// - UnifiedAutomation.UaServer
    /// 
    /// Example:
    /// When the server starts, the OnRootNodeManagerStarted method is triggered, and it creates
    /// and starts the ControllerNodeManager to manage specific nodes in the server's address space.
    /// </summary>
    internal class OPCUAServerManager : ServerManager
    {
        /// <summary>  
        /// Called when the root node manager has been started.  
        ///  
        /// This method is responsible for initializing and starting custom node managers  
        /// that handle specific sets of nodes within the OPC UA server's address space.  
        ///  
        /// In this implementation, the method creates an instance of the ControllerNodeManager,  
        /// which is responsible for managing nodes related to the controller functionality.  
        /// The ControllerNodeManager is then started to ensure it is ready to handle requests.  
        ///  
        /// Usage:  
        /// - This method is automatically invoked by the server framework when the root node  
        ///   manager is fully initialized and ready.  
        /// - Custom node managers should be created and started within this method.  
        ///  
        /// Dependencies:  
        /// - ControllerNodeManager: A custom node manager responsible for managing specific  
        ///   nodes in the server's address space.  
        ///  
        /// Example:  
        /// When the server starts, this method is triggered, and it initializes the  
        /// ControllerNodeManager to manage nodes related to the controller.  
        /// </summary>
        protected override void OnRootNodeManagerStarted(RootNodeManager nodeManager)
        {
            Console.WriteLine(StringResource.CreatingNodeManagers);

            ControllerNodeManager boiler = new ControllerNodeManager(this);
            boiler.Startup();
        }
    }
}
