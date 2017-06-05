# Docker Engine API Example 

This is an example of using Docker Engine API.

# Motivation
I was curious that how [DockerCloud/HAProxy](https://hub.docker.com/r/dockercloud/haproxy/) can know the linked containers and generate its config by itself. After few hours researched, I got the answer [Docker Engine API](https://docs.docker.com/engine/api/). The capacity Docker Engine API is equal to docker-cli. Actually, docker-cli is use Docker Engine API to connect the Docker daemon. Therefore, I can use Docker Engine API to find the containers in the same network as [DockerCloud/HAProxy](https://hub.docker.com/r/dockercloud/haproxy/) does.

# SDKs
Docker Engine API has many [SDKs](https://docs.docker.com/engine/api/sdks/). In this example, I used [Docker.DotNet](https://github.com/Microsoft/Docker.DotNet) in .Net Core, but you probably can find the SDK for your need.

# docker-compose
```yaml
version: '3'
services:
  web:
    image: wadehuang36/loadbalance-example
    ports:
      - 3000
  app:
    image: docker-engine-api-example
    build: .
    links:
      - web
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
```

>/var/run/docker.sock is the address for UNIX domain socket

run
```
docker-compose up -d web
docker-compose scale web=3
```
to start other containers

# Connect Docker daemon
```csharp
Uri dockerUri = new Uri("unix:///var/run/docker.sock");
DockerClient dockerClient = new DockerClientConfiguration(dockerUri).CreateClient();
```

# Inspect the Current Container
```csharp
static async Task<Container> GetCurrentContainer()
{
    var hostname = System.Environment.GetEnvironmentVariable("HOSTNAME");

    var container = await dockerClient.Containers.InspectContainerAsync(hostname);

    return container;
}
```

# Inspect Network
```csharp
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
```

# Print the Containers in the Network
```csharp
static async Task PrintLinkContainers()
{
    var container = await GetCurrentContainer();
    var network = await GetNetwork(container);

    // remove self
    var containers = network.Containers.Where(c => c.Key != container.ID);
    Console.WriteLine($"containers:{containers.ToJson()}");
}
```

# Run
```bash
dotnet build
dotnet publish -o out
docker-compose up app
```

You will see the information of the containers in output.

> if you use Visual Studio code, you can exec `Tasks: Run Task` command and select `buildAndRunOnDocker` which is the combination of the below three commands.