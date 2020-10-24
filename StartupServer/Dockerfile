FROM mcr.microsoft.com/dotnet/core/runtime:3.1
RUN mkdir App/
WORKDIR /App
COPY bin/Release/netcoreapp3.1/publish/ ./
ENTRYPOINT [ "dotnet","StartupServer.dll" ]