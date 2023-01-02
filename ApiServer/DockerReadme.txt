cd ApiServer/SilvermineNordic.Api
dotnet build

# Delete docker container FORCED if it exists
if [[ ! -z "$(docker ps -qf ancestor=silverminenordic-api:1.0.0 -a)" ]]; then
  docker rm -f $(docker ps -qf ancestor=silverminenordic-api:1.0.0 -a)
fi

# Remove the image if it exists
if [[ ! -z "$(docker image ls -q silverminenordic-api:1.0.0)" ]]; then
  docker rmi $(docker image ls -q silverminenordic-api:1.0.0)
fi

dotnet publish --os linux --arch x64 /t:PublishContainer -c Release
docker run -d -p 9080:9080 --restart=always silverminenordic-api:1.0.0
rm -rf /tmp/Containers