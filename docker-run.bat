@echo off

docker run -it -e DSX_ENVIRONMENT=%DSX_ENVIRONMENT% --rm dsx-image %*