#!/bin/bash

dotnet restore
dotnet build
echo "Waiting for test db servers"
while !  nc mssql 1433 && nc mysql 3306  ; do 
    sleep 1; 
    echo "noop"
done
echo "Test DB Servers started"
dotnet test