# Demo: Hello World

## Goal

Deploy a static "Hello, world!" HTML page to a CDN, and monitor its health and visits.

Note that because this demo contains no application data or logic, it feels more like an infrastructure-as-code tool. Don't get confused though, this is only the beginning of the story. Subsequent demos will include more application layers and aspects.   

## Solution

We will use:

- UI model with Static HTML technology adapter 
- DevOps model with GKE (Google Kubernetes Engine) technology adapter

### Prerequisites

- A GCP (Google Cloud Platform) account with GKE enabled
- A public IP address on GCP

Your local machine:

- Linux, macOS, or Windows
- .NET Core SDK 2.1 or higher
- NWheels CLI

The following tools normally run on build servers. However, since we don't include CI/CD aspect in the current demo, these tools will have to work on your machine: 

- Docker
- Google Cloud SDK, configured with your GCP account
- kubectl

Some examples assume you are using VSCode IDE with C# package (OmniSharp).  

## Walk-through

### Write code

1. Create an empty NWheels project
    ```
    $ nwheels new HelloWorld
    $ cd HelloWorld
    $ code .
    ```

1. Define UI of the "Hello, world!" page

1. Attach Static HTML technology adapter to the UI

1. Define deployment for the static HTML site

1. Define production environment using GKE technology adapter  

### Build and deploy

1. Build the project, which creates static HTML, Dockerfile, and Kubernetes YAML
    ```
    $ nwheels build
    ```

1. Run locally
    ```
    $ nwheels run
    ```

1. Deploy to production. The commands below are normally executed by generated scripts running on a build server. However, since we don't include CI/CD aspect in the current demo, we have to perform these commands manually: 
    ```
    $ docker build ...........
    $ docker push ...........
    $ gcloud get-credentials
    $ kubectl apply .........
    ```
    
### Test and monitor   

1. Test the site: visit the web page hosted on Google CDN
1. Check health and visitor statistics

## Wrapping up 

Congratulations! You've just performed a full development iteration!

In the next demo, we will create a full-stack application that manages TODO items list. 

