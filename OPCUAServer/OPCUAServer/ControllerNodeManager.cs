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
using System.Threading;
using System.IO;
using System.Reflection;
using OPCUAServer.SE.Resources;
using SE.OPCUAServer.Constants;
using UnifiedAutomation.UaBase;
using UnifiedAutomation.UaServer;

namespace SE.OPCUAServer
{
    public class ControllerNodeManager : BaseNodeManager
    {
        /// <summary>
        /// The identifier for the AirConditionerControllerType ObjectType.
        /// </summary>
       

        /// <summary>
        /// Gets or sets the index of the instance namespace.
        /// </summary>
        /// <value>
        /// The index of the instance namespace.
        /// </value>
        public ushort InstanceNamespaceIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the type namespace.
        /// </summary>
        /// <value>
        /// The index of the type namespace.
        /// </value>
        public ushort TypeNamespaceIndex { get; set; }
        /// <summary>
        /// Called when the node manager is started.
        /// </summary>

        public override void Startup()
        {
            try
            {
                Console.WriteLine(StringResource.StartControllerMessage);

                base.Startup();

                // save the namespaces used by this node manager.
                TypeNamespaceIndex = AddNamespaceUri(OCUAServerConstants.OrganizationNamespaceUri);
                InstanceNamespaceIndex = AddNamespaceUri(OCUAServerConstants.OrganizationInstanceNamespaceUri);

                // load the model.
                Console.WriteLine(StringResource.LoadControllerModelMessage);
                ImportUaNodeset(Assembly.GetEntryAssembly(), OCUAServerConstants.AutomationModelFilename);

                // initialize the underlying system.
                UnderlyingSystem.Initialize();

                // Create a Folder for Controllers
                CreateObjectSettings settings = new CreateObjectSettings()
                {
                    ParentNodeId = ObjectIds.ObjectsFolder,
                    ReferenceTypeId = ReferenceTypeIds.Organizes,
                    RequestedNodeId = new NodeId(OCUAServerConstants.Controllers, InstanceNamespaceIndex),
                    BrowseName = new QualifiedName(OCUAServerConstants.Controllers, InstanceNamespaceIndex),
                    TypeDefinitionId = ObjectTypeIds.FolderType
                };
                CreateObject(Server.DefaultRequestContext, settings);

                // Create controllers from configuration
                foreach (BlockConfiguration block in UnderlyingSystem.GetBlocks())
                {
                    // set type definition NodeId
                    NodeId typeDefinitionId = ObjectTypeIds.BaseObjectType;
                    if (block.Type == BlockType.AirConditioner)
                    {
                        typeDefinitionId = new NodeId(OCUAServerConstants.AirConditionerControllerType, TypeNamespaceIndex);
                    }

                    // create object.
                    settings = new CreateObjectSettings()
                    {
                        ParentNodeId = new NodeId(OCUAServerConstants.Controllers, InstanceNamespaceIndex),
                        ReferenceTypeId = ReferenceTypeIds.Organizes,
                        RequestedNodeId = new NodeId(block.Name, InstanceNamespaceIndex),
                        BrowseName = new QualifiedName(block.Name, TypeNamespaceIndex),
                        TypeDefinitionId = typeDefinitionId
                    };

                    CreateObject(Server.DefaultRequestContext, settings);
                    // Set NodeHandleType to ExternalPolled
                    foreach (BlockProperty property in block.Properties)
                    {
                        // the node was already created when the controller object was instantiated.
                        // this call links the node to the underlying system data.
                        VariableNode variable = SetVariableConfiguration(
                            new NodeId(block.Name, InstanceNamespaceIndex),
                            new QualifiedName(property.Name, TypeNamespaceIndex),
                            NodeHandleType.ExternalPolled,
                            new SystemAddress() { Address = block.Address, Offset = property.Offset });

                        // Add information about expected range
                        if (variable != null)
                        {
                            // in-memory nodes must be locked before updates.
                            // reads do not require locks for simple types and references.
                            // value reads require a lock.
                            lock (InMemoryNodeLock)
                            {
                                variable.AccessLevel = (property.Writeable) ? AccessLevels.CurrentReadOrWrite : AccessLevels.CurrentRead;
                            }

                            if (property.Range != null)
                            {
                                SetVariableDefaultValue(
                                    variable.NodeId,
                                    new QualifiedName(BrowseNames.EURange),
                                    new Variant(property.Range));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(StringResource.ControllerNodeManagerFailed + e.Message);
            }
        }

        /// <summary>
        /// Called when the node manager is stopped.
        /// </summary>
        public override void Shutdown()
        {
            try
            {
                Console.WriteLine(StringResource.StoppingControllerNodeManager);
                base.Shutdown();
            }
            catch (Exception e)
            {
                Console.WriteLine(StringResource.FailedToStopControllerNodeManager + e.Message); ;
            }
        }

        /// <summary>
        /// Reads the attributes.
        /// </summary>
        /// [Read]
        protected override void Read(
            RequestContext context,
            TransactionHandle transaction,
            IList<NodeAttributeOperationHandle> operationHandles,
            IList<ReadValueId> settings)
        {
            for (int ii = 0; ii < operationHandles.Count; ii++)
            {
                DataValue dv = null;

                // the data passed to CreateVariable is returned as the UserData in the handle.
                SystemAddress address = operationHandles[ii].NodeHandle.UserData as SystemAddress;

                if (address != null)
                {
                    // read the data from the underlying system.
                    object value = UnderlyingSystem.Read(address.Address, address.Offset);

                    if (value != null)
                    {
                        dv = new DataValue(new Variant(value, null), DateTime.UtcNow);

                        // apply any index range or encoding.
                        if (!String.IsNullOrEmpty(settings[ii].IndexRange) || !QualifiedName.IsNull(settings[ii].DataEncoding))
                        {
                            dv = ApplyIndexRangeAndEncoding(
                                operationHandles[ii].NodeHandle,
                                dv,
                                settings[ii].IndexRange,
                                settings[ii].DataEncoding);
                        }
                    }
                }

                // set an error if not found.
                if (dv == null)
                {
                    dv = new DataValue(new StatusCode(StatusCodes.BadNodeIdUnknown));
                }

                // return the data to the caller.
                ((ReadCompleteEventHandler)transaction.Callback)(
                    operationHandles[ii],
                    transaction.CallbackData,
                    dv,
                    false);
            }
        }
        /// <summary>
        /// Write the attributes
        /// </summary>
        /// [Write]
        protected override void Write(
            RequestContext context,
            TransactionHandle transaction,
            IList<NodeAttributeOperationHandle> operationHandles,
            IList<WriteValue> settings)
        {
            for (int ii = 0; ii < operationHandles.Count; ii++)
            {
                StatusCode error = StatusCodes.Good;

                // the data passed to CreateVariable is returned as the UserData in the handle.
                SystemAddress address = operationHandles[ii].NodeHandle.UserData as SystemAddress;

                if (address != null)
                {
                    if (!String.IsNullOrEmpty(settings[ii].IndexRange))
                    {
                        error = StatusCodes.BadIndexRangeInvalid;
                    }
                    else if (!UnderlyingSystem.Write(address.Address, address.Offset, settings[ii].Value.Value))
                    {
                        error = StatusCodes.BadUserAccessDenied;
                    }
                }
                else
                {
                    error = StatusCodes.BadNodeIdUnknown;
                }

                // return the data to the caller.
                ((WriteCompleteEventHandler)transaction.Callback)(
                    operationHandles[ii],
                    transaction.CallbackData,
                    error,
                    false);
            }
        }

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// [Create system in constructor]
        public ControllerNodeManager(ServerManager server) : base(server)
        {
            UnderlyingSystem = new UnderlyingSystem();
        }

        #endregion

        #region IDisposable
        /// <summary>
        /// An overrideable version of the Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO
            }
        }
        #endregion

        #region SystemAddress Class
        public class SystemAddress
        {
            public int Address;
            public int Offset;
        }
        #endregion

        #region Public Properties

        public UnderlyingSystem UnderlyingSystem
        {
            get;
            set;
        }
        #endregion
    }
}
