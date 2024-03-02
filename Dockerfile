FROM mcr.microsoft.com/dotnet/sdk:6.0 AS fetch-env
COPY . .
RUN dotnet restore -s https://api.nuget.org/v3/index.json -s https://www.myget.org/F/aelf-project-dev/api/v3/index.json

FROM fetch-env AS build-env
ARG servicepath
RUN dotnet publish src/$servicepath/$servicepath.csproj -o /output

FROM mcr.microsoft.com/dotnet/aspnet:6.0
COPY --from=build-env /output .
ARG servicepath
ENV RUNCMD="dotnet $servicepath.dll"
CMD $RUNCMD
