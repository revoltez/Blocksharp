<p align="center">
<img src="https://user-images.githubusercontent.com/24751547/203262649-9d8a8c30-487f-49a4-8d48-5dd2c11d6d0d.png" heigh="250px" width="250px" class="center"></p>
</p>


# Blocksharp 
Actor based, horizontally scalable Blockchain using Akka.net.

This Blockchain aims to provide an easy way to create blockchains by simply injecting your own actor that should represent the blockchain runtime or in other words(the blockchain state transition function)

This Blockchain uses pBFT (practical byzantine fault tolerance) consensus Algorithm 
> __Note__
*this project is still under heavy development*

[![](https://img.shields.io/badge/Donate-yellow?style=for-the-badge)](https://www.patreon.com/free_college)
# why not OOP 
  Designing big scalable distributed systems requires a model that is distributed by default.
  
  The traditional object-oriented programming paradigm makes it harder to create such systems and raises huge and big problems,
  more detailed informations can be found here [What Problems Does the Actor Model Solve?](https://getakka.net/articles/intro/what-problems-does-actor-model-solve.html)
  
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
- the PBFT folder represents the consensus engine 

- startupserver folder (acts as bootstrap node, used to easily connect validators)

- BlockchainTypes folder: a library that groups all the used types in this project.

- kubernetesconfigs is a folder that contains few shell scripts to automate the building process
# Actor hierarchy 
- as we all know actors form a hierarchy and the following diagram is the actor hierarchy of the blockchain 
![actor hieararchy](https://user-images.githubusercontent.com/24751547/97779637-989aa400-1b7f-11eb-9358-54e00e5fb183.png)

# kubernetes config
- we used kubernetes to get more realistic and easier testing for a real network.
- below is a simple diagram that shows the simple network simulated in kubernetes
![kubernetes](https://user-images.githubusercontent.com/24751547/97779662-cda6f680-1b7f-11eb-9dce-dfb944d566a6.png)

# testing locally 
- download and build this dockerfile and build it in minikube https://github.com/revoltez/netcorewithef 

a simple image that contains Entity core as a base image for the app, this can be merged with the application image (PBFT), they are seperated so that i dont download all the required libariries everytime i test :)

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
linkedin : <www.linkedin.com/in/houadef-salih>
