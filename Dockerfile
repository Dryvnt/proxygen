FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
ARG TARGETARCH
ARG TARGETPLATFORM

WORKDIR /app

COPY global.json global.json
COPY Directory.Build.props Directory.Build.props
COPY Directory.Build.targets Directory.Build.targets
COPY Directory.Packages.props Directory.Packages.props
COPY Proxygen.sln Proxygen.sln
COPY Proxygen/Proxygen.csproj Proxygen/Proxygen.csproj
COPY Update/Update.csproj Update/Update.csproj
COPY SharedModel/SharedModel.csproj SharedModel/SharedModel.csproj
COPY Test/Test.csproj Test/Test.csproj
RUN dotnet restore -a $TARGETARCH

COPY . .
RUN dotnet publish Proxygen -a $TARGETARCH --no-restore -c Release -o /out

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0
LABEL org.opencontainers.image.source=https://github.com/dryvnt/proxygen

COPY --from=build-env /out /app
WORKDIR /app
ENTRYPOINT ["/app/Proxygen"]
