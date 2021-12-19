using AdvancedServiceControl;
using System.ComponentModel;

namespace ServiceModel.WinServices
{
    [RunInstaller(true)]
    public class SensorModelInstaller : AdvancedServiceInstaller
    {
        public SensorModelInstaller() : base("SensorModelService", "SensorModelService", "SensorModelService")
        {
        }
    }
}
