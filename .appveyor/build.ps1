# https://stefanscherer.github.io/use-appveyor-to-build-multi-arch-docker-image/

# Variables to initialize in appveyor
# ARCH                     : Architecture for linux builds

# Variables to manage initialize in this script
$imageName = "parrot";    # name of the image



# Script begin
$ErrorActionPreference = 'Stop';

Write-Host Starting build
if ($isWindows) 
{
    Set-Location ..\Src\Black.Beard.Parrot\Black.Beard.ParrotServices
    docker build --pull -t $imageName -f Dockerfile.windows .

} else 
{
  # docker pull golang
  docker build --platform "linux/$env:ARCH" -t $imageName # --build-arg "arch=$env:ARCH" .
}

docker images