# Running instructions

The easiest way to get the project running, is to open the sln file in the top directory, build the solution and run the VCC.ProductPricingApiTest.Api and VCC.ProductPricingApiTest.Web files from within the solution. This should launch both the api and web solutions in your default browser. The API will default to a swagger view so you can play just with the API endpoints, and the web view will default to the home page from which you can access the Products page from the left hand nav bar. This method avoids any issues with certificates.

An alternative, is to open two powershell windows, browse to the same dir as the solution and run the following commands (so one powershell instance for each app):

`dotnet run --project .\VCC.ProductPricingApiTest.Api\VCC.ProductPricingApiTest.Api.csproj`
`dotnet run --project .\VCC.ProductPricingApiTest.Web\VCC.ProductPricingApiTest.Web.csproj`

then browse to:

`http://localhost:5213/Swagger`  for the API Swagger view
`[http://localhost:5213/Swagge](http://localhost:5217/)`  for the web UI

# DAL switch

Inside VCC.ProductPricingApiTest.Api/program.cs there is an enum option to switch between to Data Access Layers, EF or a static version using singleton (a third option is in the enum but not fully implemented as it requires an actual DB).

# Other notes

There is a Notes.txt file in the top directory with some of my design thoughts, considerations and ideas for future improvements.

I also included an example SQL create script, in the top directory, named Tables.sql.
