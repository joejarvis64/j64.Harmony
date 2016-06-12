#!/bin/sh


# Step 1 - run a restore on the Xmpp app
echo "Running dotnet restore for Xmpp"
cd src/j64.Harmony.Xmpp
dotnet restore >restore.log
if [ $? -ne 0 ]; then
  echo "ERROR: could not run \"dnu restore\" for j64.Harmony.Xmpp.  Try to run it manually"
  exit 1
fi


# Step 2 - run a restore on the Web app
echo "Running dotnet restore for Web"
cd ../j64.Harmony.Web
dotnet restore >restore.log
if [ $? -ne 0 ]; then
  echo "ERROR: could not run \"dotnet restore\" for j64.Harmony.Web.  Try to run it manually"
  exit 1
fi

# Step 3 - run the web app
cd .
dotnet run
