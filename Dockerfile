FROM mcr.microsoft.com/dotnet/core/sdk:6.0.403

ADD ./ /src
WORKDIR /src

RUN dotnet publish -o /out

FROM mcr.microsoft.com/dotnet/core/aspnet:6.0.403
COPY --from=0 /out /app

WORKDIR /app
CMD [ "/app/Rex" ]