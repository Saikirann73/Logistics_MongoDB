# Instructions to setup the dot net project <br />
```sh
1. Clone the git repo : https://github.com/Saikirann73/Logistics_MongoDB
2. Install dot net core sdk 3.1 or above
3. From terminal navigate to “Binaries” folder
4. Set the mongodb connection string in the 'Logistics/appsettings.json' file
5. Restore the mongodump from the folder "Binaries/MongoDump" to the mongodb cluster with the database name 'logistics'.
6. Run the command “dotnet Logistics.dll” which runs the APIs and UI
7. Access the url https://localhost:5001/static/index.html to see the UI 
8. Run the test harness from ‘python’ folder.
```
# Indexes created <br />
The following indexes are already part of the provided mongodump
```sh
db.cities.createIndex({'position': '2d'})
db.cargos.createIndex({status:1,location:1})
db.cargos.createIndex({duration:1}, { sparse: true })
```
Note: 
Unit test 'Delivered cargo still visible' will fail, because i am not marking a cargo as delivered unless it is the final destination. Please skip this UT to proceed with the test harness

# Points considered during the development <br />
1.	Data Access Layers are stateless and the instantiation will happen only once during the application startup.
2.	No POCO/Model classes used to access the MongoDB SDK methods. The POCO/Models used only for converting from BSON Documents and pass to UI. 
3.	Used thread safe locks for saving the Cargo tracking history.
5.	Used Read and Write Concerns as ‘Majority’
6.	Set the “retryWrites” = true
7.	Used logging to log error and information to the console
8.	Handled mongodb exceptions.
9.	Used two change streams(Insert & Update)
a.	‘Insert’ to watch for new cargos and assigns the nearest plane
b.	‘Update’ to watch for cargo’s ‘location’ field change and assigns the right plane and routes to the plane.
c.	‘Update’ to watch for cargo’s status field for tracking history
10.	Used compute pattern to calculate the ‘duration’ field when the cargo is marked as ‘delivered’. The actual average is calculated in the new API call and query used is a covered query.
11.	Deliberately did not use the outlier pattern for cargo’s tracking history as the history will rarely reaches many items in the array.
12.	Pre-filled the eligible routes for planes based on the continents and used the hub to exchange the cargo between two regional planes.
13. For courier tracking history, the history entries are added to the document in the change stream events rather than in the API.
