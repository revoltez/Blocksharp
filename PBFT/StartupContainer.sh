echo "creatingLogMigrations"
dotnet-ef migrations add CreateLogDB --context LogContext
echo "creatingblockchainMigrations"
dotnet-ef migrations add CreateBLockchainDB --context BlockchainContext
echo "updating Log database"
dotnet-ef database update --context LogContext 
echo "updating blockchaindb"
dotnet-ef database update --context BlockchainContext
echo "Starting PBFT"
dotnet PBFT.dll