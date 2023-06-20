

# https://stefanscherer.github.io/use-appveyor-to-build-multi-arch-docker-image/
# https://github.com/3shape/containerized-structure-test


# SET DOCKER_CERT_PATH=%UserProfile%\.docker\machine\machines\HypervDefault
# SET DOCKER_MACHINE_NAME=HypervDefault
# SET DOCKER_HOST=tcp://192.168.1.15:2376
# SET DOCKER_TLS_VERIFY=1

# Variables to manage initialize in this script
$imageName = "parrot";    # name of the image

# Script begin
$ErrorActionPreference = 'Stop';

Write-Host Starting build

Set-Location .\Src\Black.Beard.Parrot\Black.Beard.ParrotServices
Write-Host setting working directory to $pwd

docker info
$os = If ($isWindows) {'windows'} Else {'linux'}

Write-Host docker build --tag $imageName --file "${os}.dockerfile" $pwd
docker build --tag $imageName --file "${os}.Dockerfile" .

Write-Host build ended

docker images
