# SDET Proof Of Concept

This is a POC project to demostrate how we can implement SDET based integration testing to our web api's.
To demostrate this, we have built a simple HRM system having 2 entities - employees and departments. There are various endpoints for adding, updating and fetching data related to these entities.

The test we have chosen here is WebApplicationFactory based, where using HttpClient we call the different endpoints we have and check whether they provide the excpected results for a particular input or logged in user type.

Integrating database is the main pain point when it comes to integration tests. In this setup, we use TestContainers which help spin up MS SQL server instance in a docker container and run tests against it. This way, we get to test with a close to real database, but won't have to worry about data being built up in our dev server. The SQL Server instance is created during the start of a test operation and is killed after all tests finish execution.

The integration tests will run during all PR's raised as well as when branch is merged.

For each test run, we can seed data into the database with data required to run that test. After each test run, we use respawn to reset the database so that tests can run in complete isolation and one test won't affect the other.

Usually for our apps, we get auth tokens passed from frontend, gets the token for a user from Azure AD. So in order to pass authentication and authorization checks in the code, we construct Claims to mimick the type of user logged into the backend. We create httpclients making using of these claims for calling the API's to test different scenarios.

For the purpose of running tests, secrets like api keys, company guid's are to be stored in the secrets section of github actions.

The github workflow files have also been updated to generate test reports as well as code coverage reports

### For running the tests locally, you will have to install docker locally on your machine.
