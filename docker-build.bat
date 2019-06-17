@echo off

docker rmi dsx-image
docker build -t dsx-image .