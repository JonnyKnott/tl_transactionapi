# TrueLayer.TransactionData

## Application Overview

The TrueLayer.TransactionData project is designed to allow users to utilise the TrueLayer Data API to view their transactions across all of their bank accounts.

- Firstly, a user connects their account by navigating to the following url:
    ```
    https://auth.truelayer.com/?response_type=code&client_id=j0nn7st3stap1-fc947a&scope=info%20accounts%20balance%20cards%20transactions%20direct_debits%20standing_orders%20offline_access&redirect_uri=http://localhost:5000/api/v1/callback/12345&providers=uk-ob-all%20uk-oauth-all
    ```
    - NOTE: The client_id and redirect_uri are subject to change depending on setup and environment. 
    - The redirect_uri must be in the structure: 
        ```
        {Active api Url}/api/v1/Callback/{User Id}
        ```
    - Make a note of the user id used in the redirect_uri, because it will be needed to access your data later.

- The application will set up access behind the scenes and store your active accounts in a permanent cache.

- Calls made to the Transaction endpoint `{Active api Url}/api/v1/transaction/{User Id}` will return a complete list of all transactions across all connected bank accounts.
    - The first call may take a while. Results will then be cached for 10 minute periods.
  
- Calls made to the Transaction Summary endpoint `{Active api Url}/api/v1/transaction/{User Id}/summary` will return a categorised summary of transaction data, split into Debit and Credit transactions.
    - Passing the query parameter `detailed=true` will return a more specific list of categories. For example, `Electronics & Software` could be a category instead of `Shopping`.

## Pre-requisites

- The application uses .NET Core 3.1.
- The application requires access to a redis cache endpoint. This can be set up below using Docker or pointed to an external service.
- It is recommended to build and run the application using the docker-compose file.

## Running the application

### Configuration

Configuration is stored in the launchSettings.json file found at `src/TrueLayer.TransactionData.WebApi/Properties/launchSettings.json`, or passed as environment variables in `docker-compose.yml`.

The application requires a TrueLayer ClientId and ClientSecret in order to function. Update the `TrueLayer__Request__ClientId` setting in the environment variables:
e.g. 
```
"TrueLayer__Request__ClientId": "j0nn7st3stap1-fc947a"
```
 
#### Client Secret

The `ClientSecret` for development and testing purposes must be added through custom configuration. The key used to navigate to the secret is `TrueLayer:Request:ClientSecret`.

- The recommended way (which will also provide the secret when running in docker) is to add a new json file named `appsettings.Development.json` to the `src/TrueLayer.TransactionData.WebApi` project folder and add the following content:

    ```json
    {
      "TrueLayer": {
        "Request": {
          "ClientSecret": "{ClientSecretValue}"
        }
      }
    }
    ```

- Ensure the following is set in `TrueLayer.TransactionData.WebApi.csproj`:
    ```xml
    <ItemGroup>
        <Content Update="appsettings.Development.json">
          <CopyToOutputDirectory>Always</CopyToOutputDirectory>
        </Content>
      </ItemGroup>
    ``` 
  
 - NOTE: <b>Using an appsettings.Development.json file is the only currently supported method of providing a user secret to the application when running inside a docker container.</b>
 
 #### Redis Endpoint
 
 The configuration value `Connections__RedisEndpoint` is set to 127.0.0.1 by default. If an external Redis endpoint is provided then this value should be updated. 
 
 ## Running application in IDE
 
 - If you want to run the application via your IDE but do not have a Redis service available, run the `docker-compose.test.yml` file as follows:
 
     ```shell
    docker-compose -f docker-compose.test.yml up
    ``` 
 
 - Once the configuration values are set up correctly, the application can be built and run via your IDE, running `TrueLayer.TransactionData.WebApi` project.
 
 ## Running application in Docker
 
 - In order to run the application in Docker, navigate to the repository root in a terminal.
 
 - Build the WebApi image
 
     ```shell
    docker-compose build webapi
    ```
 - Bring up the docker container
    ```shell
    docker-compose up
    ``` 
 
 - When running in the container the API endpoint is exposed as `http://localhost:9001/` 
 
 
 ## Postman Collection
 
 The postman collection and environment provided in the repository has example calls for the API following a successful Callback request triggered from the Auth Link.
 
 The collection can be found at `postman/TrueLayer.TransactionData.postman_collection.json` and the environment at `postman/TrueLayer Local.postman_environment.json`