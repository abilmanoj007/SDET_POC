namespace API.IntegrationTests.Infrastructure;

[CollectionDefinition("SdetTestCollection")]
public class SharedTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>
{
}
