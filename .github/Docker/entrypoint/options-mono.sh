#!/bin/sh

## Setup Mono Options
export LANG=en_US.CP1252
export LC_COLLATE=C
export MONO_GC_PARAMS="concurrent-sweep"
if [ "${DOL_MEMORY_LIMIT}x" != "x" ]; then MONO_GC_PARAMS="${MONO_GC_PARAMS},max-heap-size=${DOL_MEMORY_LIMIT}"; fi
