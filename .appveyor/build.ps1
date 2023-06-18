# https://stefanscherer.github.io/use-appveyor-to-build-multi-arch-docker-image/

$ErrorActionPreference = 'Stop';
$files = ""
Write-Host Starting build

if ($isWindows) 
{
    Set-Location ..\Src\Black.Beard.Parrot\Black.Beard.ParrotServices
    docker build --pull -t whoami -f Dockerfile.windows .

} else 
{
  docker pull golang
  docker build --platform "linux/$env:ARCH" -t whoami --build-arg "arch=$env:ARCH" .
}

docker images
