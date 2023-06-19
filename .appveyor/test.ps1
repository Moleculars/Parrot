

# Variables to initialize in appveyor
# ARCH                     : Architecture for linux builds

# Variables to manage initialize in this script
$imageName = "parrot";    # name of the image




# Script begin
$containerName = $imageName + 'Test';
Write-Host Starting test

if ($env:ARCH -ne "amd64") {
  Write-Host "Arch $env:ARCH detected. Skip testing."
  exit 0
}

$ErrorActionPreference = 'SilentlyContinue';
docker kill $containerName
docker rm -f $containerName

$ErrorActionPreference = 'Stop';
Write-Host Starting container
docker run --name $containerName -p 8080:8080 -d whoami
Start-Sleep 10

$ErrorActionPreference = 'SilentlyContinue';

docker logs $containerName
docker kill $containerName
docker rm -f $containerName