dotnet publish -c release 
# this is necessary to copy the directory context to Docker deamon
docker build -t pbft  .