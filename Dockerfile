FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /source
COPY . .
RUN dotnet restore -s https://api.nuget.org/v3/index.json -s https://www.myget.org/F/aelf-project-dev/api/v3/index.json
ARG servicepath
RUN dotnet publish src/$servicepath/$servicepath.csproj -o /output

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /output .
ARG servicepath
ENV RUNCMD="dotnet /app/$servicepath.dll"
CMD $RUNCMD
