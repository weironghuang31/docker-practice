# Increment Build in Docker

I have met a project which takes more than 20 minutes to build a brand new image by Docker. Therefore, I have tried some methods to speed up the process such as make dockerfile better to keep caches effective. Finally, I tried to use two types of build. The first type is build a brand new image as the base image. The second type is increased build which is based on the base image. So the second type of build won't build anything from scratch, even if caches are ineffective which save a lot of developing time. Let's start to see how it works.

## Create two files of dockerfile and docker-compose.yml
1. Normal dockerfile and docker-compose.yml which includes all steps.

### dockerfile
``` dockerfile
FROM node:7

WORKDIR /app
CMD ["npm", "start"]

# install RVM, Ruby, and composs
RUN \curl -L https://get.rvm.io | bash -s stable
RUN /bin/bash -l -c "rvm requirements"
RUN /bin/bash -l -c "rvm install 2.0"
RUN /bin/bash -l -c "gem install compass"

# install dependencies of two sub-modules
COPY ./module1/package.json /app/module1/package.json
RUN cd /module1 && npm install

COPY ./module2/package.json /app/module2/package.json
RUN cd /app/module2 && npm install

# install dependencies of main project
COPY ./package.json /app/package.json
COPY ./bower.json /app/bower.json
RUN cd /app && npm install && bower install

# pre-compile js, less and sass files
COPY ./app/public /app/public
COPY ./app/assert /app/assert
COPY ./Gruntfile.js /app/Gruntfile.js
RUN gulp build

# copy source code
COPY ./ /app/
```

### docker-compose.yml
``` yml
version: '3'
services:
  www:
    image: example_www:base
    image: example_www:latest
    build: 
      context: ./example_www
      dockerfile: dockerfile
```

-----

2. Lite dockerfile and docker-compose.yml which only includes some steps for re-build.

### dockerfile-increment
``` dockerfile
# use base image which is builded from first dockerfile
FROM example_www:base

WORKDIR /app
CMD ["npm", "start"]

# install dependencies of two sub-modules
COPY ./module1/package.json /app/module1/package.json
RUN cd /module1 && npm install

COPY ./module2/package.json /app/module2/package.json
RUN cd /app/module2 && npm install

# install dependencies of main project
COPY ./package.json /app/package.json
COPY ./bower.json /app/bower.json
RUN cd /app && npm install && bower install

# pre-compile js, less and sass files
COPY ./public /app/public
COPY ./assert /app/assert
COPY ./Gruntfile.js /app/Gruntfile.js
RUN gulp build

# copy source code
COPY ./ /app/
```

### docker-compose-increment.yml
```
version: '3'
services:
  www:
    image: example_www:latest
    build: 
      context: ./example_www
      dockerfile: dockerfile-increment
```

As you can see, the first dockerfile installs all dependencies such as Rudy, Compass and all npm packages. The second dockerfile-increment skips some steps. Also, because it is from the base image, the base image has installed all npm packages already. Although, one of package.json is modified and causes cache ineffective. Docker won't install all the npm package again.

>The reason that the commands, copy package.json, npm install, gulp build and others are before `COPY ./ /app/`, is because I want to make cache effective as much as possible. Docker compares the copied files, if they are the same as the last build, Docker will use cache, but if the files are changed, the caches below the copy step will become ineffective. In this example, as long as any package.json isn't modified, the caches stays effective. On the other hand, if we put `COPY ./ /app/` in the beginning. One of files is modified causes all of caches ineffective, so probably Docker will run `npm install` every time the project builds.


## Build
run
``` bash
docker-compose build
# or 
docker-compose up
```
to build the base image or pure image when the first time and deployment. Docker image is read only. Any changes are in a new layer overlay the base image, so the increment image is bigger the pure image. And we might not want to deploy that image to the production server. 

------
run
``` bash
docker-compose -f docker-compose-increment.yml build
# or 
docker-compose -f docker-compose-increment.yml up
```
to build when development.

>Futhermore, because I put all the build steps in dockerfile, so anyone use git clone the project on any machine which has docker-cli. He or she can just run `docker-compose up` to build without installing any requirements.

## References
- [Best practices for writing Dockerfiles](https://docs.docker.com/engine/userguide/eng-image/dockerfile_best-practices/)

- [Get started with Docker Compose](https://docs.docker.com/compose/gettingstarted/#step-5-update-the-application)


