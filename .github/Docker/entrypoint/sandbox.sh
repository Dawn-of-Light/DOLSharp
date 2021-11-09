#!/bin/sh

export DOL_SQLITE_DB="/dawn-of-light/database/sandbox/dol.sqlite3.db"

cp -f /dawn-of-light/database/dol.sqlite3.db "$DOL_SQLITE_DB"

sed -i \
    -e 's/protected static string UserName = ".*";/protected static string UserName = "'"${DOL_SANDBOX_SERVERUPDATE_USERNAME}"'";/' \
    -e 's/protected static string Password = ".*";/protected static string Password = "'"${DOL_SANDBOX_SERVERUPDATE_PASSWORD}"'";/' \
    /dawn-of-light/scripts/Utility/ServerListUpdate.cs

exec /entrypoint.sh "$@"
