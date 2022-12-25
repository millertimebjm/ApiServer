sudo docker ps
sudo docker stop <container_id>
sudo docker rm <container_id>
sudo docker images
sudo docker rmi <image_id>

sudo dotnet publish --os linux --arch x64 /t:PublishContainer -c Release
sudo docker images 
sudo docker run -d -p 9080:9080 --restart=always <image_id>