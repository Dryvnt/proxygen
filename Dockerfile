FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

COPY *.sln ./
COPY Proxygen/*.csproj ./Proxygen/
COPY Update/*.csproj ./Update/
COPY SharedModel/*.csproj ./SharedModel/
COPY Test/*.csproj ./Test/
COPY Cli/*.csproj ./Cli/
RUN dotnet restore

COPY Cli Cli
COPY Proxygen Proxygen
COPY Update Update
COPY SharedModel SharedModel
RUN dotnet publish Proxygen -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["/app/Proxygen"]
