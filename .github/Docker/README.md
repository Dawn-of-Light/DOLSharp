# Dawn Of Light Docker Image
Docker Image for Dawn of Light Server

https://hub.docker.com/r/dawnoflight/dolsharp

# Tags

| Tag | Description |
| --- | --- |
| latest | Net 6 Based Image running DOL with supplied database |
| sandbox | Net 6 Based Image with embedded Eve of Darkness Public Database (sqlite) |

# Env Vars
| Name | Values | Description |
| --- | --- | --- |
| DOL_MEMORY_LIMIT | byte | Heap Size Limit in bytes for Server Runtime |
| DOL_DB_TYPE | SQLITE, MYSQL | DOL Database Type |
| DOL_SQLITE_CACHE | byte | SQLite Cache Size in bytes |
| DOL_SQLITE_DB | path | SQLite path to database file |
| DOL_MYSQL_HOST | host | MySQL Server Host |
| DOL_MYSQL_PORT | port | MySQL Server Port (default: 3306) |
| DOL_MYSQL_DB | name | MySQL DB Name (default: dolserver) |
| DOL_MYSQL_USER | username | MySQL Connection Username (default: root) |
| DOL_MYSQL_PASSWORD | password | MySQL Connection Password (default: empty) |
| DOL_TCP_PORT | port | DOL TCP Listening Port (default: 10300) |
| DOL_REGION_PORT | port | DOL Region Listening Port (default: 10400) |
| DOL_UDP_PORT | port | DOL UDP Listening Port (default: 10400) |
| DOL_SERVER_NAME | name | DOL Server Long Name (default: Dawn Of Light) |
| DOL_SERVER_NAMESHORT | name | DOL Server Short Name (default: DOLSERVER) |
| DOL_SCRIPTS_ASSEMBLIES | path | Path to additional scripts Assemblies (default: System.dll,System.Xml.dll) |
| DOL_AUTO_ACCOUNTCREATION | bool | Enable auto account creation on login (default: True) |
| DOL_GAME_TYPE | type | Server Game Type (default: Normal) |
| DOL_DB_CONNECTIONSTRING | string | Override the entire Connection String |
| DOL_DB_AUTOSAVE | bool | Enable Periodic Auto saving to Database (default: True) |
| DOL_DB_AUTOSAVE_INTERVAL | int | Database Auto save period in minutes (default: 10) |
| DOL_CPU_USE | int | Number of Region Timer Thread to spawn for handling Game Events (default: 4) |

# Docker Compose for Sandbox image

```
version: '2'
services:
  dawn-of-light-sandbox:
    image: dawnoflight/dolsharp:sandbox-dotnet
    container_name: dol-sandbox-container
    restart: unless-stopped
    mem_limit: 4096M
    stop_grace_period: 35s
    command: /sandbox.sh dotnet DOLServer.dll
    environment:
      DOL_MEMORY_LIMIT: 4294967296
      DOL_CPU_USE: 4
      DOL_SERVER_NAME: Sandbox
      DOL_SERVER_NAMESHORT: SANDBOX
      DOL_SANDBOX_SERVERUPDATE_USERNAME: Your_ServerListUpdate_Username
      DOL_SANDBOX_SERVERUPDATE_PASSWORD: Your_ServerListUpdate_Password
    ports:
     - 10300:10300/tcp
     - 10400:10400/udp
    tmpfs:
     - /dawn-of-light/database/sandbox
     - /dawn-of-light/logs
```

