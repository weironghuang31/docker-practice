# Load Balance With Docker Compose

## docker-compose.yml
```yml
version: '2'
services:
  web:
    image: wadehuang36/loadbalance-example
    build: .
    ports:
      - 3000
  lb:
    image: dockercloud/haproxy
    ports:
      - 80:80
    links:
      - web
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
```

## Usage
RUN `docker-compose up -d`

RUN `docker-compose scale web=3`

than RUN `docker-compose ps` you can see there are 4 containers running.

|Name|Command|State|Ports|
|----|-------|-----|-----|
|loadbalanceexample_lb_1|/sbin/tini -- dockercloud- ...|Up|0.0.0.0:80->80/tcp|
|loadbalanceexample_web_1|node app.js|Up|0.0.0.0:32769->3000/tcp|
|loadbalanceexample_web_2|node app.js|Up|0.0.0.0:32770->3000/tcp|
|loadbalanceexample_web_3|node app.js|Up|0.0.0.0:32771->3000/tcp|

Visit [localhost](http://localhost) and refresh many times, you can find the hostnames are different.

## Mechanism
In the below table, you can see that the three ports of web containers are 32769, 32770 and 32771. And the ports maps to 3000. That because we don't specify the port of web in docker-compose.yml. Therefore, docker assigns random ports to web containers. And  
/etc/var/docker.sock:/etc/var/docker.sock is added in the file(.sock file is Unix domain socket file), so HAProxy can communicate to the api of docker host. Then it can know how many linked containers and their hostnames, so it can create the config by itself. You can use below command to see the generated config.

`docker exec loadbalanceexample_lb_1 cat haproxy.cfg`