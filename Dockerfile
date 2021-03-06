FROM mcr.microsoft.com/dotnet/core/sdk:3.1.411

ADD ./ /src
WORKDIR /src

RUN dotnet publish -o /out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.17
COPY --from=0 /out /app

WORKDIR /app
CMD [ "/app/Rex" ]