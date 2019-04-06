#!/bin/bash

SCRIPT_DIR=$(cd `dirname $0` && pwd)
GKE_DIR=$SCRIPT_DIR/gke

docker build -t hello-world-site 

docker tag hello-world-site gcr.io/galvanic-wall-235207/demo-hello-world

docker push gcr.io/galvanic-wall-235207/demo-hello-world

kubectl apply -f $GKE_DIR
