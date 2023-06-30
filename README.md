# Parrot dynamic service for hosting mock services


## Quick start to launch the service

the service runs under linux docker.

For launch the docker container
```batch    
    sudo docker run -p 127.0.0.1:80:80/tcp blackbeardteam/parrot
```

Or in interactive mode
```batch
    sudo docker run -it --entrypoint /bin/bash blackbeardteam/parrot
```

Launch a navigator and open the swagger page

``` batch
   explorer.exe "https://{url}:80/swagger/index.html"
```


## Quick start to use the service by code with curl.

Arguments
- **url** address of the docker container
- **contract name** : custom name you want to give to yours service
- **contract file** openApi v3.\* contract

### Upload the contract on the service
```batch
   curl -X POST "https://localhost:80/Manager/mock/{contract name}/upload" -H "accept: */*" -H "Content-Type: multipart/form-data" -F "upfile=@swagger.json;type=application/json"
```

for this test the file to upload is named swagger.json.


### Launch the new mocked service
```batch
   curl -X PUT "https://{url}:80/Manager/mock/{contract name}/run" -H "accept: */*"
```

### Stop the service
```batch
   curl -X PUT "https://{url}:80/Manager/mock/{contract name}/kill" -H "accept: */*"
```

### Fetch the list of mock template uploaded.
```batch
   curl -X GET "https://{url}:80/Manager/mock" -H "accept: application/json"
```

### Fetch the list of mock template launched.
```batch
   curl -X GET "https://{url}:80/Manager/mock/runnings" -H "accept: application/json"
```


# How to test on windows

Source : https://learn.microsoft.com/en-us/windows/wsl/install

for install a linux virtual machine
```batch
wsl --install -d Ubuntu
```

Now we need to install Docker
source : https://docs.docker.com/engine/install/ubuntu/


## Install using the apt repository
Before you install Docker Engine for the first time on a new host machine, you need to set up the Docker repository. Afterward, you can install and update Docker from the repository.

Set up the repository
Update the apt package index and install packages to allow apt to use a repository over HTTPS:

```bash
 sudo apt-get update
 sudo apt-get install ca-certificates curl gnupg
```

Add Docker�s official GPG key:

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

## If you have troubleshoot.

You have a message like this.
```html
Cannot connect to the Docker daemon at unix:/var/run/docker.sock
```

first try to launch the service.
```bash
sudo dockerd
```

Check the service run.

```bash
# check if your system is using `systemd` or `sysvinit`
ps -p 1 -o comm=
```

If the command doesn't return systemd, and in my case, Ubuntu-20.04 on WSL, the command returned init, then use the command pattern

```bash
# start services using sysvinit
sudo service docker start
```

If the command return systemd/
```bash
# start services using systemd
sudo systemctl start docker
```

