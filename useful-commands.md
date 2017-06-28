# Useful Docker Commands
this page includes some useful docker commands which might help using docker.

## Use Related Path in windows
we know `./` is related path of current path on UNIX-like systems, but it doesn't work on mounting volumes on docker on windows. You have to use absolute path such as `c:\users\name\folder` for mounting volumes. Alternatively, you can use ${pwd} in PowerShell to get current absolute path. (`pwd` is a alias of `Get-Location` on PowerShell)

```bash
docker run --rm -v ${pwd}:/folder alpine ls /folder
```

## Remove Unused Images
```bash
# Remove the images that tag and repository are <none> 
docker rmi $(docker images -q -f dangling=true)

# This is new command which serve as above command
docker image prune
```
Document of [docker-image-prune](https://docs.docker.com/engine/reference/commandline/image_prune/)