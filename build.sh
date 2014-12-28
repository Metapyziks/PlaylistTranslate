#!/bin/bash

DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

cd $DIR

PREMAKE_DIR=/home/ziks/Downloads/premake5-hg/src/premake-dev

premake5 "--scripts=$PREMAKE_DIR" vs2013
premake5 "--scripts=$PREMAKE_DIR" gmake

make
