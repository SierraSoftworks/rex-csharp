FROM mcr.microsoft.com/dotnet/core/sdk:8.0.203

ADD ./ /src
WORKDIR /src

RUN dotnet publish -o /out

FROM mcr.microsoft.com/dotnet/core/aspnet:8.0.203
COPY --from=0 /out /app

WORKDIR /app
CMD [ "/app/Rex" ]