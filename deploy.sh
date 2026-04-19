#!/bin/bash
BUILD_TARGET="linux-x64"
BUILD_FOLDER="./Backend/publish"
WEBAPP_NAME="scorecounter"
RESOURCE_GROUP="RG_ScoreCounter"

rm -f /tmp/archive.zip
rm -rf ${BUILD_FOLDER}

dotnet publish ./Backend -c Release -r ${BUILD_TARGET} -o ${BUILD_FOLDER}

cd ${BUILD_FOLDER}
zip -r /tmp/archive.zip .
cd -

az webapp deploy --name ${WEBAPP_NAME} --resource-group ${RESOURCE_GROUP} --src-path /tmp/archive.zip --type zip
