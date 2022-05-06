FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["TwilioFlow2Mermaid.App.csproj", "TwilioFlow2Mermaid.App/"]
RUN dotnet restore "TwilioFlow2Mermaid.App/TwilioFlow2Mermaid.App.csproj"
COPY . ./TwilioFlow2Mermaid.App/

WORKDIR "/src/TwilioFlow2Mermaid.App"
RUN dotnet build "TwilioFlow2Mermaid.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TwilioFlow2Mermaid.App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TwilioFlow2Mermaid.App.dll"]

#RUN echo "--abc--"
#CMD /bin/bash