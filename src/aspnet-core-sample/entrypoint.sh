#!/bin/bash

set -e
run_cmd="dotnet run --server.urls http://*:80"

exec $run_cmd