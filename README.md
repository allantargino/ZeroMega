# Zero Mega

Zero Mega is a project from [ThingsExpert](https://github.com/ThingsExpert/) implemented in partner with Microsoft. In this repository you can find all solutions and scripts related to the project.

There are 3 folders:
- **Bash Scripts:** Here are the scripts in charge to send data from Linux Servers to Azure Event Hub.
- **Stream Analytics Scripts:** Here are the scripts for processing incoming data in Azure Stream Analytics.
- **ZeroMega**: The Visual Studio solution with the Zero Mega ASP.NET Web API.
  - GraphConsoleApp: Console Application in charge of set extended properties in Azure AD
  - SASGenerator: Console Application in order to get SAS tokens for isolated tests.
  - ZeroMegaAPI: Main project from this solution, which is an ASP.NET Web API.
  
## Background

Things Expert wanted to create a car tracking solution that was more efficient than the ones already on the market. The target market is car rental companies, as they usually implement car tracking devices to obtain significant discounts from insurance companies. However, existing solutions on the market are based on installing GPS devices on cars, which has two drawbacks: high battery consumption and high usage of data connection (incurring mobile operator costs).  Given this scenario, the project team established 4 barriers to be overcome:
-	Battery consumption
-	High data usage
-	Need of scalability to support an initial estimate of 200,000 cars
-	Message Integrity, meaning none of the messages sent to the cloud would be lost

In order to achieve that the following architecture for the solution was suggested.

## Architecture
### Part 1: Data Insertion 

Things Expert had already developed a framework encompassing mobile infrastructure and messaging through their network, but it was not connected to the Cloud. Once we identified their server (Signaling Central) already could send HTTP messages to any destination, we started to draw the cloud connection part of the solution.

A typical message sent from cars is pretty much like this:

```json
{
  "anum":"21960101935",
	"bnum":"008102980000000180014",
  "cgi":"724030041128843",
  "id_thing":"724180050340589",
	"lat":"-23.5813055556",
	"long":"-46.62425"
}
```

The final archicture for the insertion part is showed right beyond:

<p align="center">
 <img src="/Images/architecture_1.png" width="700">
</p>



### Part 2: Data Query

For the data query part, the final structure is the following one. We discuss it along this article.

<p align="center">
 <img src="/Images/architecture_2.png" width="500">
</p>

## Running the solution:

### Prerequisites:
- [Azure Account](https://azure.microsoft.com/en-us/free/)
- [Azure Active Directory Tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/)
- [Microsoft Azure Storage Emulator](https://github.com/Azure/azure-content/blob/master/articles/storage/storage-use-emulator.md)
- [Visual Studio](https://www.visualstudio.com/downloads/)

### Step 1.	Use Azure AD to store the account number as an Extension Property
1. Create an Web Application in Azure AD and called it ZeroGraph.
2. Get the Tenand Name, Tenant ID, Client ID and Client Secret.
3. Open GraphConsoleApp project and  insert your values in [Constants.cs](/ZeroMega/GraphConsoleApp/Constants.cs) file:
 ```cs
 public const string TenantName = "<Your Tenant Name>.onmicrosoft.com";
 public const string TenantId = "<Your Tenant ID>";
 public const string ClientId = "<Your Client ID>";
 public const string ClientSecret = "<Your Client Secret>";
 public const string AuthString = "https://login.windows.net/" + TenantName;
 public const string ResourceUrl = "https://graph.windows.net";

 ```
4. Run the project with F5.
5. Type and set the desired users’ account.

### Step 2.	Setup the IoT/Data Insertion Enviroment
1.	Create a Service Bus Namespace
2.	Create an Event Hub
3.	Create a Storage Account with General Purpose
4.	Use Microsoft Azure Storage Emulator and create:
  * BLOB Container called idthingsrd
  * Tables xDRTable and xDR2LogsTable
5. Upload the file Stream Analytics Scripts\2016-08-09-03-22-ref.csv into the idthingsrd container.
6. Create a Stream Analytics and set it this way:
  * Input 1: Event Hub
  * Input 2: Reference data idthingsrd
  * Output 1: Table xDRTable
  * Output 2: Table xDR2LogsTable
  * Query: Copy and paste the following content: (Also available in [Stream Analytics Scripts\query.txt](/Stream Analytics Scripts\/query.txt):

<!-- -->

 ```sql
 --Processing
 WITH ProcessedInput AS (
     SELECT
         CASE
             WHEN LEN(id_thing) = 17 THEN CONCAT('90', id_thing)
             WHEN LEN(id_thing) = 15 THEN CONCAT('8000', id_thing)
             ELSE CONCAT('X', id_thing)
         END AS id_thing, System.TimeStamp AS datetime_event, lat AS latitude, long AS longitude, dts AS date_event, tts AS time_event, anum AS numA, bnum AS numB, cgi AS CGI        
     FROM
         Labcom01in
 )

 --Output: xDR
 SELECT
     PI.id_thing, PI.datetime_event, Ref.account, PI.latitude, PI.longitude, PI.date_event, PI.time_event, PI.numA, PI.numB, PI.CGI
 INTO
     xDRout
 FROM
     ProcessedInput PI
 JOIN
     Idthingrdin Ref
 ON
     PI.id_thing = Ref.id_thing

 ```

> IMEI, IMSI, NumberA, NumberB and CGI are tipically values extracted from a GSM call.

<!-- -->
> Tipically, IMEI numbers has 17 digits, while IMSI 15 digits. The partner responsible for the design of the solution decided to make a standard 19 digits unique identifier, using the prefix 90 in case an incoming IMEI message, or 8000 if it is an IMSI one. Besides that, for log purpuses, if is not an IMEI or an IMSI case, an X prefix is added.

Your Stream Analytics topology should be similar to this one:
<p align="center">
 <img src="/Images/stream_analytics_topology.png" width="500">
</p>


### Step 3.	Setup the Data Query Enviroment
1. Create an Web Application in Azure AD and called it ZeroAPI.
2. Set the permission to the application could read the directory.
3. Get the Tenand Name, APP ID URI, Client ID and Client Secret.
4. Create an general purpuse storage account in Azure. Get the connection string.
5. Open ZeroMega solution and ZeroMegaAPI project. Inside the [Web.config](/ZeroMega/ZeroMegaAPI/Web.config) file, replace the following values:

 ```xml
  <appSettings>
    <add key="ClientValidationEnabled" value="true" />
    <add key="UnobtrusiveJavaScriptEnabled" value="true" />
    <!--Main App Settings:-->
    <add key="ida:Tenant" value="<Your Tenant Name>.onmicrosoft.com"; />
    <add key="ida:Audience" value="<Your APP ID URI>" />
    <add key="ida:ClientID" value="<Your Client ID>" />
    <add key="ida:Password" value="<Your Client Secret>" />
    <add key="StorageConnectionString" value="<Your Storage Connection String>" />
  </appSettings>
 ```

6. Copy the ZeroGraph [constants settings previously setup](/ZeroMega/GraphConsoleApp/Constants.cs) for the file [Constants.cs](/ZeroMega/ZeroMegaAPI/Constants.cs) in ZeroMegaAPI project.
> We decided use a constants file instead of using Web.config file in order to separate the roles for quering Active Directory and for the Web API.
7. Run the project with F5 and check if everything is working fine.
8. Publish the Web API as an Azure Web App and exposes it for your web services/partners.
 
> The main goal with the project is to make a commercial usage for the API straight to developers. To achieve that, we decided to use Azure API Management to provide an easy interface to control the usage and analytics info. This step is optional if you wouldn't like to add this commercial/control layer.

<!-- -->
> Basically, you need to: 
- Create an API Management environment
- Use your Azure AD tenant as an OAuth authorization server
- Add a reference to Azure API App using Swagger doc file

<!-- -->
> [Steve Danielson](https://github.com/steved0x) has wroten an amazing article showing how to to protect a Web API backend with Azure Active Directory and API Management. You can find it here: https://azure.microsoft.com/en-us/documentation/articles/api-management-howto-protect-backend-with-aad/

## Last Words About the API

As you can see in the IPosition interface, you can query the cars (or other things) positions by ThingId, AccountID and DateTime):
```cs
    interface IPosition
    {
        Task<ThingPosition> GetThingPosition(int accountId, string thingId, string datetime);

        Task<IEnumerable<ThingPosition>> GetThingPositions(int accountId, string thingId);

        Task<IEnumerable<ThingPosition>> GetThingPositions(int accountId, string thingId, DateTime lowerLimit);

        Task<IEnumerable<ThingPosition>> GetThingPositions(int accountId, string thingId, DateTime lowerLimit, DateTime upperLimit);

        Task<IEnumerable<ThingPosition>> GetAllThingsPositions(int accountId);
    }
```

Besides it shows AccountID as parameter (related to the cars owner), the user cannot queries others cars which don't belongs to him/her. As soon we protected the API using Azure AD and extended its schema for support the AccountID property, we can do the following: Query AD for the AccountID from the Authenticated User and returns just his/her cars.

```cs
    public class PositionController : ApiController
    {
        private int _account;
        private PositionRepository _repository = new PositionRepository();

        //api/Position/
        [Route("api/Position")]
        public async Task<IEnumerable<ThingPosition>> Get()
        {
            var user = getUPN();
            _account = GetAccountId(user);

            return await _repository.GetAllThingsPositions(_account);
        }
	
     [...]
```
