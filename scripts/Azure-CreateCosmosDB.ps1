# O módulo do Azure Powershell é necessário que esteja instalado no computador
# Também é necessário que as configurações seja executadas anteriormente
# Link de instalação: https://learn.microsoft.com/en-us/powershell/azure/install-azure-powershell?view=azps-10.2.0

# Valores Padrões
$defaultTenantId = "<Insira o Tenant Id>"                           # Substituicao
$defaultLocation = "<Insira a Location>"                            # Substituicao
$defaultResourceGroupName = "<Insira o Nome do Resource Group>"     # Substituicao
$defaultAccountName = "<Insira o Account Name>"                     # Substituicao
$defaultDatabaseName = "<Insira o Database Name>"                   # Substituicao
$defaultContainerName = "<Insira o Container Name>"                 # Substituicao
$defaultGraphName = "<Insira o Graph Name>"                         # Substituicao

# Valores default
$api = "Gremlin"
$consistencyLevel = "Session"
$conflictResolutionPath = "/_ts"
$partitionKeys = @("/email")
$graphRUs = 400

# Input dos parâmetros para processar
$tenantId = Read-Host "Entre com o Tenant Id: [$($defaultTenantId)]"
$tenantId = ($defaultTenantId, $tenantId)[[bool]$tenantId]

$resourceGroupName = Read-Host "Entre com o Resource Group: [$($defaultResourceGroupName)]"
$resourceGroupName = ($defaultResourceGroupName, $resourceGroupName)[[bool]$resourceGroupName]

$location = Read-Host "Entre com o Location: [$($defaultLocation)]"
$location = ($defaultAccountName, $defaultLocation)[[bool]$defaultLocation]

$accountName = Read-Host "Entre com o Account Name: [$($defaultAccountName)]"
$accountName = ($defaultAccountName, $accountName)[[bool]$accountName]

$databaseName = Read-Host "Entre com o Database Name: [$($defaultDatabaseName)]"
$databaseName = ($defaultDatabaseName, $databaseName)[[bool]$databaseName]

$containerName = Read-Host "Entre com o Container Name: [$($defaultContainerName)]"
$containerName = ($defaultContainerName, $containerName)[[bool]$containerName]

$graphName = Read-Host "Entre com o Graph Name: [$($defaultGraphName)]"
$graphName = ($defaultGraphName, $graphName)[[bool]$graphName]

# Autentique-se no Azure
Connect-AzAccount -TenantId $tenantId

# Criar um novo Resource Group
Write-Host "Criando Resource Group"
New-AzResourceGroup -Name $resourceGroupName -Location $location

# Crie uma Cosmos DB Account
Write-Host "Criando o Cosmos DB Account"
$account = New-AzCosmosDBAccount -ResourceGroupName $resourceGroupName -Location $location -Name $accountName -ApiKind $api -DefaultConsistencyLevel $consistencyLevel -EnableAutomaticFailover:$true -EnableFreeTier:$true

# Crie um database no Cosmos DB
Write-Host "Criando o Cosmos DB Gremilin"
$database = New-AzCosmosDBGremlinDatabase -ParentObject $account -Name $databaseName

# Prepare conflict resolution policy object for graph
Write-Host "Criando o Cosmos DB Gremilin Graph"
$conflictResolutionPolicy = New-AzCosmosDBGremlinConflictResolutionPolicy -Type LastWriterWins -Path $conflictResolutionPath
$graph = New-AzCosmosDBGremlinGraph -ParentObject $database -Name $graphName -Throughput $graphRUs -PartitionKeyKind Hash -PartitionKeyPath $partitionKeys -ConflictResolutionPolicy $conflictResolutionPolicy

$graph

# Finalizacao
Write-Host "Banco criado com sucesso!"