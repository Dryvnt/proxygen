FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

COPY *.sln ./
COPY Proxygen/*.csproj ./Proxygen/
COPY Parsing/*.csproj ./Parsing/
COPY SharedModel/*.csproj ./SharedModel/
COPY Test/*.csproj ./Test/
COPY Cli/*.csproj ./Cli/
RUN dotnet restore

COPY Cli Cli
COPY Parsing Parsing
COPY Proxygen Proxygen
COPY SharedModel SharedModel
COPY Test Test
RUN dotnet publish Proxygen -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
CMD ["sleep", "604800"]
