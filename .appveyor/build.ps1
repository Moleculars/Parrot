

# https://stefanscherer.github.io/use-appveyor-to-build-multi-arch-docker-image/
# https://github.com/3shape/containerized-structure-test

# Variables to manage initialize in this script
$imageName = "parrot";    # name of the image

# Script begin
$ErrorActionPreference = 'Stop';



$taggedimage = $imageName + ':' + $APPVEYOR_BUILD_VERSION

Write-Host Starting build $taggedimage;

Set-Location .\Src
Write-Host setting working directory to $pwd;

docker info
$os = If ($isWindows) {'Windows'} Else {'Ubuntu'}

Write-Host docker build --tag $imageName --file "${os}.dockerfile" $pwd
$ID = $(docker build --tag $imageName --file "Dockerfile.${os}" .)
Write-Host image $ID generated


Write-Host retag $ID to $taggedimage
docker tag $ID $taggedimage


Write-Host build ended

docker images

Set-Location ..