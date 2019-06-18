@echo off

docker rmi dsx-bash-image
docker build -t dsx-bash-image -f DockerfileBash .