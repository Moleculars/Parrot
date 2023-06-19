# Parrot dynamic service for hosting mock services


## Quick start
Launch the docker container
```batch
   docker run blackbeardteam/parrot -p 80:{port} -p 443:{port https}
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