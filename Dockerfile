# 1: Build the exe
# FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS builder
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster as builder
WORKDIR /app

# 1a: Prepare for static linking
RUN apt-get update && \
    apt-get install -y libtool libzmq3-dev

# Copy csproj and restore as distinct layers
COPY client.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
FROM mcr.microsoft.com/dotnet/aspnet:3.1-buster-slim

RUN apt-get update && \
    apt-get install -y libtool libzmq3-dev

WORKDIR /app
RUN chmod -R 777 /app
COPY --from=builder /app/out .
RUN ls -la

ENTRYPOINT ["./client"]