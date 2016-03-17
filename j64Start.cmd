@echo off

rem Step 1 - run a restore on the Xmpp app
echo "Running dnu restore for Xmpp"
cd src/j64.Harmony.Xmpp
call dnu restore >restore.log

rem Step 2 - run a restore on the WebApi app
echo "Running dnu restore for WebApi"
cd ../j64.Harmony.WebApi
call dnu restore >restore.log

rem Step 3 - run the web app
echo "Running the j64Harmony Web App"
cd .
dnx web



