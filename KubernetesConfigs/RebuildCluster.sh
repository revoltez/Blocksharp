docker rmi pbft
docker rmi startupserverimage
echo "Create PBFT image"
cd ../PBFT
./Build.sh
echo "create Server Image"
cd ../StartupServer
./Build.sh
echo "deploying app" 
cd ../KubernetesConfigs
./DeployApp.sh
