param (
    [switch]$reset = $false    
)

$ErrorActionPreference = "Stop"

Push-Location $PSScriptRoot

#------------------------------------------- variables ------------------------------------------------------------------------------

$dockerComposeFilePath = Join-Path $PSScriptRoot $(If ($IsMacOS) { "./docker-compose.macos.yaml" } Else { "./docker-compose.yaml" })
$dockerProjectName = "azure_essential_demo"

$dbInstance = "127.0.0.1,14331"
$dbName = "AzureMasterclass"
$dbSaUsername = "sa"
$dbSaPassword = "Pa55w0rd!"
$dbUsername = "azureMasterClass"
$dbPassword = "Pa55w0rd!"
$dbConnectionString = "Data Source=$dbInstance;User Id=$dbSaUsername;Password=$dbSaPassword;Initial Catalog=master;"

$dbUpProjectFilePath = "../src/AzureMasterclass.DbUp/AzureMasterclass.DbUp.csproj"
$dbUpExeFilePath = "../src/AzureMasterclass.DbUp//bin/Release/net7.0/Thiess.MyThiess.DatabaseUp.dll"

$waitSeconds = 30

#------------------------------------------- modules --------------------------------------------------------------------------------

Write-Host "Adding PowerShell modules" -ForegroundColor Magenta

# references
Install-Module -Name "SqlServer" -AllowClobber
Update-Module -Name "SqlServer"
Import-Module -Name "SqlServer"
Install-Module -Name "Az" -Scope CurrentUser -Repository PSGallery -Force -AllowClobber
Update-Module -Name "Az"
Import-Module -Name "Az"

#------------------------------------------- docker ---------------------------------------------------------------------------------

# run docker compose
if ($reset)
{
    Write-Host "Recreating docker containers" -ForegroundColor Magenta

    docker-compose --file "$dockerComposeFilePath" --project-name "$dockerProjectName" up --detach --force-recreate
} 
else
{
    Write-Host "Creating docker containers" -ForegroundColor Magenta
    
    docker-compose --file "$dockerComposeFilePath" --project-name "$dockerProjectName" up --detach
}

#------------------------------------------- database --------------------------------------------------------------------------------

Write-Host "Wait $waitSeconds seconds for SqlServer to start" -ForegroundColor Magenta
# wait for sql server to start
Start-Sleep -Seconds $waitSeconds

# configure db
Write-Host "Initialise database" -ForegroundColor Magenta

$dbConnection = New-Object System.Data.SqlClient.SqlConnection $dbConnectionString
$dbConnection.Open()
$dbServer = New-Object Microsoft.SqlServer.Management.Smo.Server($dbConnection)

if (-Not ($dbServer.Logins.Contains($dbUsername)))
{
    Write-Host "Create user login" -ForegroundColor Magenta
    # create new user login
    $dbUserLogin = New-Object -TypeName Microsoft.SqlServer.Management.Smo.Login -ArgumentList $dbServer, $dbUsername
    $dbUserLogin.LoginType = [Microsoft.SqlServer.Management.Smo.LoginType]::SqlLogin
    $dbUserLogin.Create($dbPassword)
}

if (-Not ($dbServer.Databases.Contains($dbName)))
{
    Write-Host "Create database" -ForegroundColor Magenta
    # create database and set owner
    $db = New-Object -TypeName Microsoft.SqlServer.Management.Smo.Database -ArgumentList $dbServer, $dbName
    $db.Create()
    $db.SetOwner($dbUsername)
}

$dbConnection.Close()

# initialize db
Write-Host "Run DbUp" -ForegroundColor Magenta
dotnet build "$dbUpProjectFilePath" --configuration Release
dotnet "$dbUpExeFilePath" "$dbConnectionString".Replace("master", $dbName)

#------------------------------------------- blob storage ----------------------------------------------------------------------------

# configure blob storage
Write-Host "Initializing blob storage" -ForegroundColor Magenta

$blobStorageContainerName = "storage0"

New-AzStorageContainer $blobStorageContainerName -Permission Off -Context (New-AzStorageContext -Local)

#-------------------------------------------------------------------------------------------------------------------------------------

Pop-Location
