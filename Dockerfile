FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /App
COPY data/Templates /Templates
COPY data/SampleData /SampleData
COPY --from=build-env /App/out .

ENV TemplateDirectory=/Templates

ENTRYPOINT ["dotnet", "Microsoft.Health.Convert.Web.dll"]
