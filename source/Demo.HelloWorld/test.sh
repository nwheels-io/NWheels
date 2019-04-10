#!/bin/bash

SCRIPT_DIR=$(cd `dirname $0` && pwd)

dotnet $SCRIPT_DIR/../NWheels.Cli/bin/Debug/netcoreapp2.1/dotnet-nw.dll $SCRIPT_DIR/Demo.HelloWorld.csproj
