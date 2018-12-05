# Build stage
FROM microsoft/dotnet:2.2.100-sdk-alpine3.8 as build

RUN apk update && apk upgrade --no-cache

COPY src /src
WORKDIR /src

RUN dotnet publish -c Release

# Run stage
FROM microsoft/dotnet:2.2.0-aspnetcore-runtime-alpine3.8 as run

RUN apk update && apk upgrade --no-cache

EXPOSE 9090/tcp
ENV ASPNETCORE_URLS http://*:9090

ARG OBJECT_PORT
ARG OBJECT_MONGO_HOST
ARG OBJECT_MONGO_PORT
ARG OBJECT_MONGO_USER_DATABASE
ARG OBJECT_MONGO_USERNAME
ARG OBJECT_MONGO_PASSWORD
ARG OBJECT_IMMUTABLE
ARG OBJECT_FLUENTD_HOST
ARG OBJECT_FLUENTD_PORT
ARG OBJECT_PROXY_HOSTNAME
ARG OAUTH2_ACCESS_TOKEN_URI
ARG OAUTH2_PROTECTED_URIS
ARG OAUTH2_CLIENT_ID
ARG OAUTH2_CLIENT_SECRET
ARG SSL_VERIFYING_DISABLE

ENV OBJECT_PORT ${OBJECT_PORT}
ENV OBJECT_MONGO_HOST ${OBJECT_MONGO_HOST}
ENV OBJECT_MONGO_PORT ${OBJECT_MONGO_PORT}
ENV OBJECT_MONGO_USER_DATABASE ${OBJECT_MONGO_USER_DATABASE}
ENV OBJECT_MONGO_USERNAME ${OBJECT_MONGO_USERNAME}
ENV OBJECT_MONGO_PASSWORD ${OBJECT_MONGO_PASSWORD}
ENV OBJECT_IMMUTABLE ${OBJECT_IMMUTABLE}
ENV OBJECT_FLUENTD_HOST ${OBJECT_FLUENTD_HOST}
ENV OBJECT_FLUENTD_PORT ${OBJECT_FLUENTD_PORT}
ENV OBJECT_PROXY_HOSTNAME ${OBJECT_PROXY_HOSTNAME}
ENV OAUTH2_ACCESS_TOKEN_URI ${OAUTH2_ACCESS_TOKEN_URI}
ENV OAUTH2_PROTECTED_URIS ${OAUTH2_PROTECTED_URIS}
ENV OAUTH2_CLIENT_ID ${OAUTH2_CLIENT_ID}
ENV OAUTH2_CLIENT_SECRET ${OAUTH2_CLIENT_SECRET}
ENV SSL_VERIFYING_DISABLE ${SSL_VERIFYING_DISABLE}

COPY --from=build /src/bin/Release/netcoreapp2.2/publish /app
WORKDIR /app

# don't run as root user
RUN chown 1001:0 /app/Foundation.ObjectService.WebUI.dll
RUN chmod g+rwx /app/Foundation.ObjectService.WebUI.dll
USER 1001

ENTRYPOINT [ "dotnet", "Foundation.ObjectService.WebUI.dll" ]
