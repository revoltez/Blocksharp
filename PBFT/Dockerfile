FROM netcorewithef
WORKDIR /App
RUN dotnet add package BlockchainTypes --version 2.1.3
COPY /Actors/Replica/Brokers/BlockchainContext.cs /Actors/Replica/Brokers/LogContext.cs  /bin/release/netcoreapp3.1/publish/ ./
COPY StartupContainer.sh ./
RUN chmod +x StartupContainer.sh
EXPOSE 5000
ENTRYPOINT ["/bin/bash","StartupContainer.sh"]