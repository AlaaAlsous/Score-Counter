$BUILD_TARGET = "linux-x64"
$BUILD_FOLDER = "./Backend/publish"
$WEBAPP_NAME = "scorecounter"
$RESOURCE_GROUP = "RG_ScoreCounter"

Remove-Item -Force "$env:TEMP\archive.zip" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force $BUILD_FOLDER -ErrorAction SilentlyContinue

dotnet publish ./Backend -c Release -r $BUILD_TARGET -o $BUILD_FOLDER

Compress-Archive -Path "$BUILD_FOLDER/*" -DestinationPath "$env:TEMP\archive.zip"

az webapp deploy --name $WEBAPP_NAME --resource-group $RESOURCE_GROUP --src-path "$env:TEMP\archive.zip" --type zip
