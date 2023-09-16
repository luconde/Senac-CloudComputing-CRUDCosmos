# O módulo do Azure Powershell é necessário que esteja instalado no computador
# Também é necessário que as configurações seja executadas anteriormente
# Link de instalação: https://learn.microsoft.com/en-us/powershell/azure/install-azure-powershell?view=azps-10.2.0

# Valores Padrões
$defaultTenantId = "<Insira o Tenant Id>"                           # Substituicao
$defaultResourceGroupName = "<Insira o Nome do Resource Group>"     # Substituicao
$defaultDatabaseName = "<Insira o Database Name>"                   # Substituicao
$defaultAccountName = "<Insira o Account Name>"                     # Substituicao

# Input dos parâmetros para processar
$tenantId = Read-Host "Entre com o Tenant Id: [$($defaultTenantId)]"
$tenantId = ($defaultTenantId, $tenantId)[[bool]$tenantId]

$resourceGroupName = Read-Host "Entre com o Resource Group: [$($defaultResourceGroupName)]"
$resourceGroupName = ($defaultResourceGroupName, $resourceGroupName)[[bool]$resourceGroupName]

$databaseName = Read-Host "Entre com o Database Name: [$($defaultDatabaseName)]"
$databaseName = ($defaultDatabaseName, $databaseName)[[bool]$databaseName]

$accountName = Read-Host "Entre com o Account Name: [$($defaultAccountName)]"
$accountName = ($defaultAccountName, $accountName)[[bool]$accountName]

# Conecte-se à sua conta do Azure
Connect-AzAccount -TenantId $tenantId

# Exclua o banco de dados no Cosmos DB
Write-Host "Apagando o Graph"
Remove-AzCosmosDBGraph -ResourceGroupName $resourceGroupName -AccountName $accountName -DatabaseName $databaseName -ApiName "Gremlin"

# Exclua a conta do Cosmos DB
Write-Host "Apagando o Account Name"
Remove-AzCosmosDBAccount -ResourceGroupName $resourceGroupName -Name $accountName

# Exclua o grupo de recursos
Write-Host "Apagando o Resource Group"
Remove-AzResourceGroup -Name $resourceGroupName -Force

# Finalizacao
Write-Host "Banco criado com sucesso!"
