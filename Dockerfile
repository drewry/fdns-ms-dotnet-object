# Build stage
FROM microsoft/dotnet:2.2.103-sdk-alpine3.8 as build

ENV DOTNET_CLI_TELEMETRY_OPTOUT true

COPY src /src
WORKDIR /src

RUN dotnet publish -c Release

# Run stage
FROM microsoft/dotnet:2.2.1-aspnetcore-runtime-alpine3.8 as run

RUN apk update && apk upgrade --no-cache

ARG SYSTEM_NAME
ARG OBJECT_PORT
ARG OBJECT_MONGO_CONNECTION_STRING
ARG OBJECT_MONGO_USE_SSL
ARG OBJECT_IMMUTABLE
ARG OBJECT_FLUENTD_HOST
ARG OBJECT_FLUENTD_PORT
ARG OBJECT_PROXY_HOSTNAME
ARG OAUTH2_ACCESS_TOKEN_URI
ARG OAUTH2_READINESS_CHECK_URI
ARG OAUTH2_PROTECTED_URIS
ARG OAUTH2_CLIENT_ID
ARG OAUTH2_CLIENT_SECRET
ARG SSL_VERIFYING_DISABLE

ENV SYSTEM_NAME ${SYSTEM_NAME}
ENV OBJECT_PORT ${OBJECT_PORT}
ENV OBJECT_MONGO_CONNECTION_STRING ${OBJECT_MONGO_CONNECTION_STRING}
ENV OBJECT_MONGO_USE_SSL ${OBJECT_MONGO_USE_SSL}
ENV OBJECT_IMMUTABLE ${OBJECT_IMMUTABLE}
ENV OBJECT_FLUENTD_HOST ${OBJECT_FLUENTD_HOST}
ENV OBJECT_FLUENTD_PORT ${OBJECT_FLUENTD_PORT}
ENV OBJECT_PROXY_HOSTNAME ${OBJECT_PROXY_HOSTNAME}
ENV OAUTH2_ACCESS_TOKEN_URI ${OAUTH2_ACCESS_TOKEN_URI}
ENV OAUTH2_READINESS_CHECK_URI ${OAUTH2_READINESS_CHECK_URI}
ENV OAUTH2_PROTECTED_URIS ${OAUTH2_PROTECTED_URIS}
ENV OAUTH2_CLIENT_ID ${OAUTH2_CLIENT_ID}
ENV OAUTH2_CLIENT_SECRET ${OAUTH2_CLIENT_SECRET}
ENV SSL_VERIFYING_DISABLE ${SSL_VERIFYING_DISABLE}

EXPOSE ${OBJECT_PORT}/tcp
ENV ASPNETCORE_URLS http://*:${OBJECT_PORT}

COPY --from=build /src/bin/Release/netcoreapp2.2/publish /app
WORKDIR /app

# don't run as root user
RUN chown 1001:0 /app/Foundation.ObjectService.WebUI.dll
RUN chmod g+rwx /app/Foundation.ObjectService.WebUI.dll
USER 1001

ENTRYPOINT [ "dotnet", "Foundation.ObjectService.WebUI.dll" ]

