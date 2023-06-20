# https://stefanscherer.github.io/use-appveyor-to-build-multi-arch-docker-image/

# Variables to initialize in appveyor
# ARCH                     : Architecture for linux builds

# Variables to manage initialize in this script
$imageName = "parrot";    # name of the image

# Script begin
$ErrorActionPreference = 'Stop';

Write-Host Starting build

Set-Location .\Src\Black.Beard.Parrot\Black.Beard.ParrotServices

if ($isWindows) 
{

  # The running command stopped because the preference variable "ErrorActionPreference" or common parameter is set to Stop: 
  # error during connect: Post http:////./pipe/docker_engine/v1.40/build?buildargs={}&cachefrom=[]&cgroupparent=&cpuperiod=0&cpuquota=0&cpusetcpus=&cpusetmems=&cpushares=0&dockerfile=Dockerfile.windows&labels=%7B%7D&memory=0&memswap=0&networkmode=default&pull=1&rm=1&session=pv0909b63j0jxsnv1pvzo2y1s&shmsize=0&t=parrot&target=&ulimits=null&version=1: open //./pipe/docker_engine: 
  # The system cannot find the file specified. In the default daemon configuration on Windows, the docker client must be run elevated to connect. This error may also indicate that the docker daemon is not running.
  
    docker build -t $imageName -f Dockerfile.windows

} else 
{
  # docker pull golang
  # docker build --platform "linux/$env:ARCH" -t $imageName # --build-arg "arch=$env:ARCH" .
  docker build -t $imageName .
}

docker images
