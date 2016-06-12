@echo off

rem Step 1 - run a restore on the Xmpp app
echo "Running dotnet restore for Xmpp"
cd src/j64.Harmony.Xmpp
call dotnet restore >restore.log

rem Step 2 - run a restore on the Web app
echo "Running dotnet restore for Web"
cd ../j64.Harmony.Web
call dotnet restore >restore.log

rem Step 3 - run the web app
echo "Running the j64Harmony Web App"
cd .
dotnet run



