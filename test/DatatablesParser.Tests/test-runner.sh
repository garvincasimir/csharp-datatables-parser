#!/bin/bash

echo "Restore and build while db servers are starting"
dotnet restore
dotnet build
echo "Testing connections to test db servers"
while !  nc -w 1 mssql 1433 && nc -w 1 mysql 3306  ; do 
    sleep 1; 
    echo "Test db servers not ready. Trying again"
done
echo "Test DB Servers started"
dotnet test