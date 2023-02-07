#!/bin/sh

BASE_FOLDER=$(dirname "$(realpath "$0")")
dotnet "$BASE_FOLDER/DOLServer.dll"