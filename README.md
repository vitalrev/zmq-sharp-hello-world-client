### Build docker image

```
./buildDocker.sh
```

## Run docker container

```
docker run -it -e ZMQ_SERVER_HOST=lissi-cloud-dev.westeurope.cloudapp.azure.com zmq-sharp-hello-client:latest

docker run -it -e ZMQ_SERVER_HOST=lissi-cloud-dev.westeurope.cloudapp.azure.com \
-e ZMQ_SOCKS_PROXY= \
-e ZMQ_PROXY_USER= \
-e ZMQ_PROXY_PASSWORD= \
zmq-sharp-hello-client:latest

```
