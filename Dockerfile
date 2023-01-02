FROM mcr.microsoft.com/dotnet/core/sdk:7.0.101

ADD ./ /src
WORKDIR /src

RUN dotnet publish -o /out

FROM mcr.microsoft.com/dotnet/core/aspnet:7.0.101
COPY --from=0 /out /app

WORKDIR /app
CMD [ "/app/Rex" ]