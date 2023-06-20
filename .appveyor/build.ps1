
# https://learn.microsoft.com/en-us/virtualization/windowscontainers/manage-docker/configure-docker-daemon#configure-docker-with-a-configuration-file
Copy-Item ./daemon.json C:\ProgramData\Docker\config\daemon.json

# https://stefanscherer.github.io/use-appveyor-to-build-multi-arch-docker-image/
# https://github.com/3shape/containerized-structure-test

sc config docker binpath= "\"C:\Program Files\docker\dockerd.exe\" --run-service -H tcp://0.0.0.0:2375"

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
docker build --tag $imageName --file "${os}.dockerfile" .

docker images
