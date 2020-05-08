#!/usr/bin/env bash
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

set -x
set -e

pushd src/Simulation/Native
./bootstrap.sh
popd

# Next steps are only needed for developers environment, they are skipped for cloud builds.
if [ ! "$AGENT_ID" == "" ]; then exit; fi

# Make sure everything is ready and builds locally.
pushd src/Simulation/Native
if [[ -z $MAKEFLAGS ]]; then
	export MAKEFLAGS="-j$(nproc)"
fi
cmake --build build
popd

dotnet build src/Simulation/CsharpGeneration.App --configuration Release
dotnet build Simulation.sln --configuration Release
