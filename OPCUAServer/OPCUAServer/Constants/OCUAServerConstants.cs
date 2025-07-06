using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.OPCUAServer.Constants
{
    public static class OCUAServerConstants
    {
        public const string ApplicationName = "UnifiedAutomation GettingStartedServer";
        public const string ApplicationUri = "urn:localhost:UnifiedAutomation:GettingStartedServer";
        public const string ProductName = "UnifiedAutomation GettingStartedServer";
        public const string BaseAddress = "opc.tcp://localhost:48030";
        public const string OrganizationNamespaceUri = "http://se.com/BuildingAutomation/";
        public const string OrganizationInstanceNamespaceUri = "http://se.com/OPCUAServer/";
        public const string AutomationModelFilename = "buildingautomation.xml";
        public const string Controllers = "Controllers";
        public const uint AirConditionerControllerType = 1003;
        public const string LicenseFile = "License.lic";
    }
}
