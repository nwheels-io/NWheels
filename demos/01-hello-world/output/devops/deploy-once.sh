#!/bin/bash

SCRIPT_DIR=$(cd `dirname $0` && pwd)
GKE_DIR=$SCRIPT_DIR/gke

gcloud auth configure-docker

kubectl create namespace demo-hello-world

gcloud compute addresses create demo-hello-world-hello-world-site-ip --global

# gcloud compute addresses delete demo-hello-world-hello-world-site-ip --global

# gcloud container clusters resize todo-1 --size 1
