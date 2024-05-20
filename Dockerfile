FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG TARGETARCH
ARG TARGETPLATFORM

WORKDIR /app

COPY global.json global.json
COPY Directory.Build.props Directory.Build.props
COPY Proxygen.sln Proxygen.sln
COPY Proxygen/Proxygen.csproj Proxygen/Proxygen.csproj
COPY Proxygen/Update.csproj Proxygen/Update.csproj
COPY Proxygen/SharedModel.csproj Proxygen/SharedModel.csproj
COPY Proxygen/Test.csproj Proxygen/Test.csproj
RUN dotnet restore -a $TARGETARCH

COPY . .
RUN dotnet publish Proxygen -a $TARGETARCH --no-restore -c Release -o /out

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/runtime:8.0
LABEL org.opencontainers.image.source=https://github.com/dryvnt/proxygen

COPY --from=build-env /out /app
ENTRYPOINT ["/app/Proxygen"]
