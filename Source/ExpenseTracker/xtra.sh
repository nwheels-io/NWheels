#!/bin/bash
cd "$(dirname "$0")"
./../NWheels.Cli/bin/Debug/netcoreapp1.0/ubuntu.16.04-x64/publish/nwheels run ./bin/Debug/netstandard1.6/ExpenseTracker.dll "$@"
