FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Reservation.Service.API/Reservation.Service.csproj", "Reservation.Service.API/"]
COPY ["Reservation.Service.Data/Reservation.Service.Data.csproj", "Reservation.Service.Data/"]
COPY ["Reservation.Service.Models/Reservation.Service.Models.csproj", "Reservation.Service.Models/"]
COPY ["DailyReminderFunction/DailyReminderFunction.csproj", "DailyReminderFunction/"]

RUN dotnet restore "Reservation.Service.API/Reservation.Service.csproj"

COPY . .

WORKDIR "/src/Reservation.Service/Reservation.Service.API"

RUN dotnet build "../../Reservation.Service.sln" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "../../Reservation.Service.sln" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Reservation.Service.API.dll"]
