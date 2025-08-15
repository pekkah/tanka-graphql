FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 4001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["samples/GraphQL.Samples.ApolloFederation.Compatibility/GraphQL.Samples.ApolloFederation.Compatibility.csproj", "samples/GraphQL.Samples.ApolloFederation.Compatibility/"]
COPY ["src/GraphQL.Extensions.ApolloFederation/GraphQL.Extensions.ApolloFederation.csproj", "src/GraphQL.Extensions.ApolloFederation/"]
COPY ["src/GraphQL/GraphQL.csproj", "src/GraphQL/"]
COPY ["src/GraphQL.Language/GraphQL.Language.csproj", "src/GraphQL.Language/"]

# Restore dependencies
RUN dotnet restore "samples/GraphQL.Samples.ApolloFederation.Compatibility/GraphQL.Samples.ApolloFederation.Compatibility.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/samples/GraphQL.Samples.ApolloFederation.Compatibility"
RUN dotnet build "GraphQL.Samples.ApolloFederation.Compatibility.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GraphQL.Samples.ApolloFederation.Compatibility.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GraphQL.Samples.ApolloFederation.Compatibility.dll"]