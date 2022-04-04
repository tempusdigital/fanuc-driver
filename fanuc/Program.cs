using System.Threading.Tasks;
using l99.driver.@base;
using MQTTnet;
using MQTTnet.Server;

namespace l99.driver.fanuc
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var mqttFactory = new MqttFactory();

            // The port for the default endpoint is 1883.
            // The default endpoint is NOT encrypted!
            // Use the builder classes where possible.
            var mqttServerOptions = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(1885)
                .Build();

            using (var mqttServer = mqttFactory.CreateMqttServer())
            {
                await mqttServer.StartAsync(mqttServerOptions);

                dynamic config = await Bootstrap.Start(args);
                Machines machines = await Machines.CreateMachines(config);
                await machines.RunAsync();
                await Bootstrap.Stop();

                await mqttServer.StopAsync();
            }
        }
    }
}