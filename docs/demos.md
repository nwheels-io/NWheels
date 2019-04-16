
Create a minimal software system, which includes a static web page saying "Hello, World!", a continuous delivery pipeline on CodeFresh, and a production environment on Google CDN. 

To follow this example, you will need:

- A GitHub account
- A CodeFresh account (free plan is enough)
- A billable GCP account with GKE enabled

### Step 1. Create git repo

Create a new repo and clone it to your machine. 









### Step 4. Deploy to CDN

In this example, we demonstrate deployment to either Google Cloud, Azure, or Amazon Web Services. 

1. Create file `Main.cs`:

    ```c#
    using NWheels.DevOps.Model;

    namespace HelloWorld
    {
        class Main : SystemMain
        {
            object Production => 
                new ProductionEnvironment()
                //TODO: adapt to a specific cloud
                ;
        }
    }
    ```

#### Deploying to Google CDN

To follow this step, you need these prerequisites:

- An account on Google Cloud Platform, with GKE enabled
- Installed or your machine: Google Cloud SDK and `kubectl`.

Normally, deployments are performed by build servers. Yet in this demo we don't declare a CI/CD pipeline, that's why we deploy from local machine. 

1. Replace the content of `Main.cs` as shown below. Replace `your-google-project-id` with your GCP project ID, and `your-google-cloud-zone` with the desired zone.


    ```c#
    using NWheels.DevOps.Model;
    using NWheels.DevOps.Adapters.Environments.Gke;

    namespace HelloWorld
    {
        class Main : SystemMain
        {
            // normally, deployment from local dev box is blocked
            [AllowDeployFromLocalDevBox] 
            GkeEnvironment Production => 
                new ProductionEnvironment().AsGkeEnvironment(
                    zone: "your-google-cloud-zone",
                    project: "your-google-project-id",
                    // normally, attempt to deploy from...
                    // ...a local dev box will be blocked
                );
        }
    }
    ```

2. Deploy the application by running:
    ```
    $ dotnet deploy production
    ```
    If everything goes right, the output should include this:
    ```
    Created cluster hello-world
    Deployed service hello-world-service
    --- ingress info ---
    NAME                 HOSTS   ADDRESS          PORTS   AGE
    hello-world-ingress  *       123.123.123.123  80      1m
    ------ DEPLOY: SUCCESS ------
    ```

3. View the page by opening browser and navigating to the IP address printed in your console. 

### Congratulations!

You've just completed a full development cycle and deployed your system to production!

To remove the deployment, delete the cluster "hello-world" on GKE.
