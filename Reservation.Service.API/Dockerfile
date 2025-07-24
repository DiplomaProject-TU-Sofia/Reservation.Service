FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Reservation.Service/Reservation.Service.sln", "Reservation.Service/"]
COPY ["Reservation.Service/Reservation.Service.API/Reservation.Service.API.csproj", "Reservation.Service/Reservation.Service.API/"]
COPY ["Reservation.Service/Reservation.Service.Data/Reservation.Service.Data.csproj", "Reservation.Service/Reservation.Service.Data/"]
COPY ["Reservation.Service/Reservation.Service.Models/Reservation.Service.Models.csproj", "Reservation.Service/Reservation.Service.Models/"]
COPY ["Reservation.Service/DailyReminderFunction/DailyReminderFunction.csproj", "Reservation.Service/DailyReminderFunction/"]

RUN dotnet restore "Reservation.Service/Reservation.Service.sln"

COPY . .

WORKDIR "/src/Reservation.Service/Reservation.Service.API"

RUN dotnet build "../../Reservation.Service.sln" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "../../Reservation.Service.sln" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Reservation.Service.API.dll"]
