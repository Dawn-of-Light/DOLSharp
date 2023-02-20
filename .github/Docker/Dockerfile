FROM --platform=$BUILDPLATFORM alpine as download_stage

ARG BUILD_VERSION=latest
ARG GITHUB_API_URL="https://api.github.com/repos/Dawn-of-Light/DOLSharp"

RUN set -ex; \
    # Set Constants
    DOL_DOTNET_ARCHIVE_NAME="DOLServer_Release.zip"; \
    DOL_GITHUB_API_URL="${GITHUB_API_URL}/releases/latest"; \
    [ "$BUILD_VERSION" != "latest" ] && DOL_GITHUB_API_URL="${GITHUB_API_URL}/releases/tags/$BUILD_VERSION"; \
    # Install Build Dependencies
    apk add --no-cache --update \
        curl jq unzip ca-certificates; \
    update-ca-certificates; \
    # Downloading DOL Release
    DOL_DOTNET_LATEST_RELEASE_URL=$(curl -s "$DOL_GITHUB_API_URL" |  jq -r ".assets[] | select(.name == \"$DOL_DOTNET_ARCHIVE_NAME\") | .browser_download_url"); \
    curl -L -o "/$DOL_DOTNET_ARCHIVE_NAME" "$DOL_DOTNET_LATEST_RELEASE_URL"; \
    unzip "/$DOL_DOTNET_ARCHIVE_NAME" -d /dawn-of-light-dotnet; 


FROM mcr.microsoft.com/dotnet/runtime:6.0 as release_dotnet

ARG BUILD_DATE=now
ARG VCS_REF=local
ARG BUILD_VERSION=latest

LABEL org.label-schema.build-date=$BUILD_DATE \
      org.label-schema.version=$BUILD_VERSION \
      org.label-schema.vcs-ref=$VCS_REF \
      org.label-schema.vcs-url="https://github.com/Dawn-of-Light/DOLSharp.git" \
      org.label-schema.name="dawn-of-light" \
      org.label-schema.description="Dawn of Light (DOL) - Dark Age of Camelot (DAOC) Server Emulator" \
      org.label-schema.usage="https://github.com/Dawn-of-Light/DOLSharp/blob/master/.github/Docker/README.md" \
      org.label-schema.schema-version="2.0.0-rc1" \
      maintainer="Leodagan <leodagan@durron.net>"

RUN set -ex; \
    # Get Tmux and procps
    apt-get update; \
    apt-get install --no-install-recommends -y tmux procps; \
    # Cleanup
    apt-get clean -y; \
    rm -rf /var/lib/apt/lists/* /var/cache/* /tmp/* /var/log/* ~/.cache; \
    # Prerequisites
    mkdir -p /dawn-of-light/config /dawn-of-light/database;

COPY --from=download_stage /dawn-of-light-dotnet /dawn-of-light

COPY entrypoint/entrypoint.sh /entrypoint.sh
COPY entrypoint/options-dotnet.sh /options.sh

WORKDIR /dawn-of-light

CMD [ "/entrypoint.sh", "dotnet", "DOLServer.dll" ]


FROM --platform=$BUILDPLATFORM download_stage as EveOfDarkness_build_stage

RUN set -ex; \
    # Set Constants
    DB_PUBLIC_API_URL="https://api.github.com/repos/Eve-of-Darkness/db-public/releases/latest"; \
    DB_PUBLIC_DOWNLOAD_URL=$(curl -s "$DB_PUBLIC_API_URL" |  jq -r ".assets[] | select(.name|startswith(\"public-db.sqlite\")) | .browser_download_url"); \
    DOL_DB_FILE="/dol.sqlite3.db"; \
    # Install Build Dependencies
    apk add --no-cache --update \
        sqlite p7zip; \
    # Get DOL Database
    curl -L -o /db.zip "$DB_PUBLIC_DOWNLOAD_URL"; \
    7z x /db.zip -o/db; \
    # Insert database content
    sqlite3 "$DOL_DB_FILE" < /db/public-db.sqlite.sql; \
    # Set Server Properties
    echo 'UPDATE `ServerProperty` SET `Value` = "Welcome to the Administrator Sandbox Dark Age of Camelot Shard - This Server is meant for testing and experimenting Admin Commands" WHERE `Key` = "starting_msg"' | sqlite3 "$DOL_DB_FILE"; \
    echo 'UPDATE `ServerProperty` SET `Value` = "The Server Database is reset twice a day, feel free to edit or test anything you want !" WHERE `Key` = "motd"' | sqlite3 "$DOL_DB_FILE"; \
    echo 'UPDATE `ServerProperty` SET `Value` = "/code" WHERE `Key` = "disabled_commands"' | sqlite3 "$DOL_DB_FILE"; \
    echo 'UPDATE `ServerProperty` SET `Value` = "False" WHERE `Key` = "load_examples"' | sqlite3 "$DOL_DB_FILE"; \
    echo 'UPDATE `ServerProperty` SET `Value` = "12" WHERE `Key` = "hours_uptime_between_shutdown"' | sqlite3 "$DOL_DB_FILE";


FROM release_dotnet as release_sandbox_dotnet

LABEL org.label-schema.build-date=$BUILD_DATE \
      org.label-schema.version=$BUILD_VERSION \
      org.label-schema.vcs-ref=$VCS_REF \
      org.label-schema.vcs-url="https://github.com/Dawn-of-Light/DOLSharp.git" \
      org.label-schema.name="dawn-of-light" \
      org.label-schema.description="Dawn of Light (DOL) Sandbox - Dark Age of Camelot (DAOC) Server Emulator" \
      org.label-schema.usage="https://github.com/Dawn-of-Light/DOLSharp/blob/master/.github/Docker/README.md" \
      org.label-schema.schema-version="2.0.0-rc1" \
      maintainer="Leodagan <leodagan@durron.net>"

RUN set -ex; \
    mkdir -p /dawn-of-light/database/sandbox

COPY --from=EveOfDarkness_build_stage /dol.sqlite3.db /dawn-of-light/database/dol.sqlite3.db
COPY entrypoint/sandbox.sh /sandbox.sh
COPY scripts/ /dawn-of-light/scripts/Utility

