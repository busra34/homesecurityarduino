using AdvancedServiceControl;

namespace ServiceModel.WinServices
{
    public class Program
    {
        static void Main(string[] args)
        {
            var advancedServiceRunner = new AdvancedServiceRunner("SensorModelService");
            advancedServiceRunner.AddServiceComponentFactory(c => new SensorModelComponent(c));


            //advancedServiceRunner.AddServiceComponentFactory(c =>
            //    new HealthServiceComponent(c, advancedServiceRunner.HealthStatus,
            //    "http://*:8880"));


            advancedServiceRunner.Startup(args);
        }
    }
}
