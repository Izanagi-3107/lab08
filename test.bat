@echo off
cd /d "%~dp0"
dotnet run --project LabW08.csproj -- --test
pause
