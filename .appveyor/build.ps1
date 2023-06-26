

# https://stefanscherer.github.io/use-appveyor-to-build-multi-arch-docker-image/
# https://github.com/3shape/containerized-structure-test

# Variables to manage initialize in this script
$imageName = "parrot";    # name of the image

# Script begin
$ErrorActionPreference = 'Stop';

Write-Host Starting local build
dotnet build ".\Src\Black.Beard.Curl\Black.Beard.Curl.csproj" -c release /p:Version=$Env:APPVEYOR_BUILD_VERSION
dotnet build ".\Src\Black.Beard.OpenApi\Black.Beard.OpenApi.csproj" -c release /p:Version=$Env:APPVEYOR_BUILD_VERSION
dotnet build ".\Src\Black.Beard.OpenApiServices\Black.Beard.OpenApiServices.csproj" -c release /p:Version=$Env:APPVEYOR_BUILD_VERSION
dotnet build ".\Src\Black.Beard.ParrotServices\Black.Beard.ParrotServices.csproj" -c release /p:Version=$Env:APPVEYOR_BUILD_VERSION

Write-Host Starting build

Set-Location .\Src
Write-Host setting working directory to $pwd

docker info
$os = If ($isWindows) {'Windows'} Else {'Ubuntu'}

Write-Host docker build --tag $imageName --file "${os}.dockerfile" $pwd
docker build --tag $imageName --file "Dockerfile.${os}" .

Write-Host build ended

#docker images
