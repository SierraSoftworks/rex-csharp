FROM mcr.microsoft.com/dotnet/sdk:10.0

ADD ./ /src
WORKDIR /src

RUN dotnet publish -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
COPY --from=0 /out /app

WORKDIR /app
CMD [ "/app/Rex" ]