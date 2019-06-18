@echo off

docker rmi dsx-pack-image
docker build -t dsx-pack-image -f DockerfilePack .