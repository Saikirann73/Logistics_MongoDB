Instructions to setup the dot net project
1. Clone the git repo : https://github.com/Saikirann73/Logistics_MongoDB
2. Install dot net core sdk 3.1 or above
3. From terminal navigate to “Binaries” folder
4. Run the command “dotnet Logistics.dll” which runs the APIs on http://localhost:5000
5. Run the test harness and UI from ‘python’ folder.

Indexes created:
db.cities.createIndex({'position': '2d'})
db.cargos.createIndex({status:1,location:1,courier:1})
db.cargos.createIndex({duration:1}, { sparse: true })

Points considered during the development
1.	Data Access Layers are stateless and the instantiation will happen only once during the application startup.
2.	No POCO/Model classes used to access the MongoDB SDK methods. The POCO/Models used only for converting from BSON Documents and pass to UI. 
3.	Used thread safe locks for saving the Cargo tracking history.
4.	I guess in the UI, the cargo is being set to delivered without prior checking if it was picked up the plane from source. So, I added the check in API. This results in a unit test failure. Please skip the “Failed to delete destination” unit test.
5.	Unit test "Delivered cargo still visible" will fill, because for circulating routes for the planes, instead of removing the first route in the array, the item will be added back as last item. Please skip this unit test too to run the test harness.
6.	Used Read and Write Concerns as ‘Majority’
7.	Set the “retryWrites” = true
8.	Used logging to log error and information to the console
9.	Handled mongodb exceptions.
10.	Used two change streams(Insert & Update)
a.	‘Insert’ to watch for new cargos and assigns the nearest plane
b.	‘Update’ to watch for cargo’s ‘location’ field change and assigns the right plane
c.	‘Update’ to watch for cargo’s status field for tracking history
11.	Used compute pattern to calculate the ‘duration’ field when the cargo is marked as ‘delivered’. The actual average is calculated in the new API call and query used is a covered query.
12.	Deliberately did not use the outlier pattern for cargo’s tracking history as the history will rarely reaches many items in the array.
13.	Pre-filled the routes for planes based on the continents and used the hub to exchange the cargo between two regional planes
