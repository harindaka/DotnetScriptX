FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS base
WORKDIR /app 
COPY . .

# RUN apk update \
#    && apk add --no-cache curl \ 
#    && apk add jq \
#    && rm -rf /var/cache/apk/*

RUN dotnet tool install -g dotnet-script

CMD ["sh", "run", "hello-world"]
