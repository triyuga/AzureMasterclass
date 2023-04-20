# Index

- `setup-development-environment.ps1` is a script to set up local enviroment using docker containers.
  - Usage : `pwsh setup-development-environment.ps1 [-reset]`
- Either `docker-compose.yaml` or `docker-compose.mac.yaml` is the docker-compose files to be called.
  - 'db' and 'blob' are the services currently in use.
- `generate-typescript-api-client.ps1` is a script using 'autorest' to generate http client code based on the server code via OpenAPI.
  - Usage : `pwsh generate-typescript-api-client.ps1` 
  