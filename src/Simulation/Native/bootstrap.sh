#!/usr/bin/env bash
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

if [[ $(uname) == Darwin ]]; then
	GCC_VERSION="-7"
	brew install gcc@7
else
	GCC_VERSION=""
fi

if [[ -z $BuildConfiguration ]]; then
	BuildConfiguration="Release"
fi

if [[ -z $BuildFolder ]]; then
	BuildFolder="build"
fi

if [[ -n $GCC_VERSION ]]; then
	echo "Building in $BuildFolder with configuration $BuildConfiguration with gcc$GCC_VERSION"
else
	echo "Building in $BuildFolder with configuration $BuildConfiguration with clang"
fi

if [ ! -d $BuildFolder ]; then
	mkdir -p $BuildFolder;
fi

pushd $BuildFolder
if [[ -f CMakeCache.txt ]]; then
	rm CMakeCache.txt;
fi
if [[ -n $GCC_VERSION ]]; then
	cmake -DBUILD_SHARED_LIBS:BOOL="1" -DCMAKE_C_COMPILER=gcc$GCC_VERSION -DCMAKE_CXX_COMPILER=g++$GCC_VERSION -DCMAKE_BUILD_TYPE=$BuildConfiguration ..
else
	cmake -DBUILD_SHARED_LIBS:BOOL="1" -DCMAKE_C_COMPILER=clang -DCMAKE_CXX_COMPILER=clang++ -D_CMAKE_TOOLCHAIN_PREFIX=llvm- -DCMAKE_LINKER=ld.lld -DCMAKE_BUILD_TYPE=$BuildConfiguration ..
fi
popd

