FROM microsoft/aspnetcore-build:2.0.0-preview2 as builder
COPY . /workspace
WORKDIR /workspace
RUN mkdir /publish
RUN dotnet publish -o /publish src/aspnet-core-sample/aspnet-core-sample.csproj 

FROM microsoft/aspnetcore:2.0.0-preview2
EXPOSE 80/tcp
COPY --from=builder /publish /app
WORKDIR /app
ENTRYPOINT ["dotnet", "aspnet-core-sample.dll"]