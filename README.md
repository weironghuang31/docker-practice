# Load Balance With Docker Compose

## docker-compose.yml
```yml
version: '2'
services:
  web:
    image: wadehuang36/loadbalance-sample
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

than you can RUN `docker-compose ps`

|Name|Command|State|Ports|
|----|-------|-----|-----|
|loadbalancesample_lb_1|/sbin/tini -- dockercloud- ...|Up|0.0.0.0:80->80/tcp|
|loadbalancesample_web_1|node app.js|Up|0.0.0.0:32769->3000/tcp|
|loadbalancesample_web_2|node app.js|Up|0.0.0.0:32771->3000/tcp|
|loadbalancesample_web_3|node app.js|Up|0.0.0.0:32770->3000/tcp|

Visit [localhost](http://localhost) and refresh many times, you can find the hostnames are different.

## Mechanism
Because /etc/var/docker.sock:/etc/var/docker.sock is added in the file(.sock file is Unix domain socket file), so HAProxy and communicate to the api of docker host. The it can know how many linked containers and its hostnames, so it can create the config by itself. You can use below command to see the generated config.

`docker exec loadbalancesample_lb_1 cat haproxy.cfg`