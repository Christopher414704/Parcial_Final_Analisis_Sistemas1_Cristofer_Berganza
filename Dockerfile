FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

RUN mkdir -p /docker-entrypoint-initdb.d \
    && chown -R mssql:root /docker-entrypoint-initdb.d /var/opt/mssql

COPY db/init.sql /docker-entrypoint-initdb.d/init.sql
COPY entrypoint.sh /entrypoint.sh

RUN chmod +x /entrypoint.sh

USER mssql

EXPOSE 1433

ENTRYPOINT ["/bin/bash", "/entrypoint.sh"]
