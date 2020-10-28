# Blocksharp 
a fully implemented Blockchain based on the Actor model using Akka.net
this Blockchain is generic meaning it can be used for multiple purposes, such as voting or a cryptocurrency or any form of decentralized app 

# Prerequisites
- basic knwoeldge on Akka.net and akka.remote 
- c#
- docker
- kubernetes, minikube must be installed to test locally

# project structure 
there are two apps and one library shared between the the two apps,
- the first is the Blockchain app which is in the PBFT folder 

- the second app is the server app which is in the startupserver folder

- the library that is used between the two apps is in the BlockchainTypes folder

- kubernetesconfigs is a folder that contains few shell scripts to automate the building process

# installation
- downlaod and install minikube 

# testing locally 
- download and build this dockerfile and build it in minikube https://github.com/revoltez/netcorewithef 

a simple image that contains Entity core as a base image for the app, this can be merged with the application image (PBFT), we seperated them so that we dont have to download all the required libariries everytime we test.

- start minikube and execute RebuildCluster.sh 
```
minikube start
eval $(minikube docker-env)
./Rebuildcluster.sh
```

- check the server ip address by checking the logs 
```
kubectl logs server 
```
- get the list of all runnnig pods 
```
kubectl get pods
```
- attach to the pbftnode and wait for it to complete building and updating the database and then provide it with the server address
```
kubectl attach -it pod-name -c pbftnode
```
# screenshots of a voting system 


# contact information 
email : <eziorevoltez@gmail.com>
linkedin : <linkedin.com/in/houadef-salih-2b92a0188>
