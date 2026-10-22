
using API.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;

namespace API.IntegrationTests.Tests.CW1;

public sealed class CreateShipmentBookingTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateShipmentBooking_ShouldReturn200_WhenAdminSendRequest()
    {
        //Arrange

        //Act
        var response = await AdminClient.PostAsync("api/create-booking", null);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sref = await response.Content.ReadAsStringAsync();
        sref.Should().NotBeNull();
        sref.Should().StartWith("S");
    }

    [Fact]
    public async Task CreateShipmentBooking_ShouldReturn403_WhenRegularUserSendRequest()
    {
        //Arrange

        //Act
        var response = await RegularUserClient.PostAsync("api/create-booking", null);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
