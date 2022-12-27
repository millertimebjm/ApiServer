sudo docker stop $(sudo docker ps -qf ancestor=silverminenordic-api) && sudo docker rm $(sudo docker ps -qf ancestor=silverminenordic-api)
sudo docker image ls -q silverminenordic-api:1.0.0 | sudo xargs docker image rm

sudo dotnet publish --os linux --arch x64 /t:PublishContainer -c Release
sudo docker run -d -p 9080:9080 --restart=always silverminenordic-api:1.0.0