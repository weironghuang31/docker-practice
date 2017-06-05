using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Newtonsoft.Json;
using Container = Docker.DotNet.Models.ContainerInspectResponse;
using Network = Docker.DotNet.Models.NetworkResponse;

namespace Docker_Engine_API
{
    static class Program
    {
        static Uri dockerUri = new Uri("unix:///var/run/docker.sock");
        static DockerClient dockerClient = new DockerClientConfiguration(dockerUri).CreateClient();

        static void Main(string[] args)
        {
            var task = Task.Run(PrintLinkContainers);
            while (!(task.IsFaulted || task.IsCompleted))
            {
                Thread.Sleep(10);
            }

            if (task.IsCompleted)
            {
                Console.WriteLine($"Complete");
            }
            else
            {
                Console.WriteLine($"Fail:{task.Exception}");
            }
        }

        static async Task<Container> GetCurrentContainer()
        {
            var hostname = System.Environment.GetEnvironmentVariable("HOSTNAME");

            var container = await dockerClient.Containers.InspectContainerAsync(hostname);

            return container;
        }

        static async Task<Network> GetNetwork(Container container)
        {
            var networkId = container.NetworkSettings.Networks.Keys.FirstOrDefault();
            if (networkId == null)
            {
                throw new Exception("This container doesn't have any network setting");
            }

            var network = await dockerClient.Networks.InspectNetworkAsync(networkId);

            return network;
        }
        static async Task PrintLinkContainers()
        {
            var container = await GetCurrentContainer();
            var network = await GetNetwork(container);

            // remove self
            var containers = network.Containers.Where(c => c.Key != container.ID);
            Console.WriteLine($"containers:{containers.ToJson()}");
        }

        static string ToJson(this object obj)
        {
            if (obj == null)
            {
                return null;
            }
            else
            {
                return JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
        }
    }
}
