using SDET_POC.API.Common.Models.CW1;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace SDET_POC.API.Services;

public interface ICW1Client
{
    Task<string> CreateShipmentBookingInCW1Async(CancellationToken cancellationToken);
}

public sealed class CW1Client : ICW1Client
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CW1Client> _logger;
    public CW1Client(HttpClient httpClient, ILogger<CW1Client> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    public async Task<string> CreateShipmentBookingInCW1Async(CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "eAdaptor");
            var path = Path.Combine(AppContext.BaseDirectory, "Features/CW1/TestData", "CreateShipmentBookingRequest.xml");
            var xmlString = File.ReadAllText(path);

            request.Content = new StringContent(xmlString);

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var responseContentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var responseSerializer = new XmlSerializer(typeof(UniversalResponseData));
            var universalResponse = responseSerializer.Deserialize(responseContentStream) as UniversalResponseData;

            var match = Regex.Match(universalResponse?.ProcessingLog ?? string.Empty, @"Successfully saved Shipment\s+(S\d+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            throw new InvalidOperationException($"Error occured while trying to retrieve shipment ref number for shipment booking");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Creating Shipment In CW1");
            throw;
        }
    }
}