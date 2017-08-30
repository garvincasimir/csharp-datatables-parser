FROM microsoft/dotnet:2.0.0-sdk
RUN apt-get update && apt-get install -y netcat
COPY /src /src
COPY /test /test
WORKDIR /test/DatatablesParser.Tests
ENTRYPOINT ./test-runner.sh
