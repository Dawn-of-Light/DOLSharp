#!/bin/sh

## Setup Dotnet Options
export LANG=en_US.CP1252
export LC_COLLATE=C
export COMPlus_gcConcurrent="1"
export COMPlus_gcServer="1"
if [ "${DOL_MEMORY_LIMIT}x" != "x" ]; then export COMPlus_GCHeapHardLimit="$(printf '%X' "${DOL_MEMORY_LIMIT}")"; fi
