# Blocksharp 
a fully implemented Blockchain based on the Actor model using Akka.net.

this Blockchain is generic meaning it can be used for multiple purposes, such as voting or a cryptocurrency or any form of decentralized app 

this Blockchain uses PBFT (practical byzantine fault tolerance) consensus Algorithm 

# why not OOP 
  Designing big scalable distributed systems requires a model that is distributed by default.
  
  The traditional object-oriented programming paradigm makes it harder to create such systems and raises huge and big problems, we summarized them as the Three   illusions which are the following: 
  - the illusion of encapsulation
  - the illusion of shared memory
  - the illusion of call stack
  
  more detailed informations can be found here https://getakka.net/
  
## tools
- Entity core as the ORM (object relational mapper) 
- Mysql database
- Akka.net
- Nsec (cryptography library)

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
# Actor hierarchy 
- as we all know actors form a hierarchy and the following diagram is the actor hierarchy of the blockchain 
![actor hieararchy](https://user-images.githubusercontent.com/24751547/97779637-989aa400-1b7f-11eb-9358-54e00e5fb183.png)

# kubernetes config
- below is a simple diagram that shows the simple network simulated in kubernetes
![kubernetes](https://user-images.githubusercontent.com/24751547/97779662-cda6f680-1b7f-11eb-9dce-dfb944d566a6.png)


# installation
- downlaod and install minikube 

# testing locally 
- download and build this dockerfile and build it in minikube https://github.com/revoltez/netcorewithef 

a simple image that contains Entity core as a base image for the app, this can be merged with the application image (PBFT), they are seperated so that i dont download all the required libariries everytime i test.

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
- processing PBFT certificates
![PREPREPARE](https://user-images.githubusercontent.com/24751547/97487438-2a769700-195d-11eb-9854-ae2294b8b63b.png)

- counting votes
![Counting Vote](https://user-images.githubusercontent.com/24751547/97487512-3cf0d080-195d-11eb-8a89-12b61494ff71.png)

- Receiving client transactions
![transactions Added](https://user-images.githubusercontent.com/24751547/97487502-395d4980-195d-11eb-8997-cb5b2c370afa.png)

- view change
![View Change](https://user-images.githubusercontent.com/24751547/97487474-33676880-195d-11eb-9bca-f4bed5c1fb43.png)


# contact information 
email : <salih.houadef@gmail.com>
linkedin : <linkedin.com/in/houadef-salih>
