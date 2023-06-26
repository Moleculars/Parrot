

# https://stefanscherer.github.io/use-appveyor-to-build-multi-arch-docker-image/
# https://github.com/3shape/containerized-structure-test

# Variables to manage initialize in this script
$imageName = "parrot";    # name of the image

# Script begin
$ErrorActionPreference = 'Stop';

Write-Host Starting build

Set-Location .\Src
Write-Host setting working directory to $pwd

docker info
$os = If ($isWindows) {'Windows'} Else {'Ubuntu'}

Write-Host docker build --tag $imageName --file "${os}.dockerfile" $pwd
ID = $(docker build --tag $imageName --file "Dockerfile.${os}" .)
docker tag $ID $imageName:$Env:APPVEYOR_BUILD_VERSION


Write-Host build ended

docker images

Set-Location ..