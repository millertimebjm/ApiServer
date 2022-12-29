docker stop $(docker ps -qf ancestor=silverminenordic-api:1.0.0) && docker rm $(docker ps -qf ancestor=silverminenordic-api:1.0.0 -a)
docker rmi $(docker image ls -q silverminenordic-api:1.0.0)

dotnet publish --os linux --arch x64 /t:PublishContainer -c Release
docker run -d -p 9080:9080 --restart=always silverminenordic-api:1.0.0
