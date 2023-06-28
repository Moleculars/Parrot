# Parrot dynamic service for hosting mock services


## Quick start
Launch the docker container
```batch
    sudo docker run blackbeardteam/parrot -p 80:{port} -p 443:{port https}
```

in interactive mode
```batch
    sudo docker run -it blackbeardteam/parrot bash
```

Launch a navigator and open the swagger page

``` batch
   explorer.exe "https://{url}:{port}/swagger/index.html"
```


## Quick start to use the service by code with curl.

Arguments
- **url** address of the docker container
- **port** port number mapped port on the container instance.
- **contract name** : custom name you want to give to yours service
- **contract file** openApi v3.\* contract

### Upload the contract
```batch
   curl -X POST "https://{url}:{port}/Manager/mock/{contract name}/upload" -H "accept: */*" -H "Content-Type: multipart/form-data" -F "upfile=@{contract file}.json;type=application/json"
```
  
### Launch the new mock service
```batch
   curl -X PUT "https://{url}:{port}/Manager/mock/{contract name}/run" -H "accept: */*"
```

### Stop a service
```batch
   curl -X PUT "https://{url}:{port}/Manager/mock/{contract name}/kill" -H "accept: */*"
```

### fetch the list of mock template uploaded.
```batch
   curl -X GET "https://{url}:{port}/Manager/mock" -H "accept: application/json"
```

### fetch the list of mock template launched.
```batch
   curl -X GET "https://{url}:{port}/Manager/mock/runnings" -H "accept: application/json"
```


# How to test on windows

https://learn.microsoft.com/en-us/windows/wsl/install

```batch
wsl --install
```

https://docs.docker.com/engine/install/ubuntu/


## Install using the apt repository
Before you install Docker Engine for the first time on a new host machine, you need to set up the Docker repository. Afterward, you can install and update Docker from the repository.

Set up the repository
Update the apt package index and install packages to allow apt to use a repository over HTTPS:

```bash
 sudo apt-get update
 sudo apt-get install ca-certificates curl gnupg
```

Add Docker’s official GPG key:

```bash
 sudo install -m 0755 -d /etc/apt/keyrings
 curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
 sudo chmod a+r /etc/apt/keyrings/docker.gpg
```

Use the following command to set up the repository:

```bash
 echo \
  "deb [arch="$(dpkg --print-architecture)" signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  "$(. /etc/os-release && echo "$VERSION_CODENAME")" stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
```


## Install Docker Engine
Update the apt package index:

```bash
sudo apt-get update
```

Install Docker Engine, containerd, and Docker Compose.

Latest
Specific version

To install the latest version, run:

```bash
sudo apt-get install docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
```
Verify that the Docker Engine installation is successful by running the hello-world image.


```bash
sudo docker run hello-world
```