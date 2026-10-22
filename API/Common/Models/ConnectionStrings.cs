using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace SDET_POC.API.Common.Models;

public sealed class ConnectionStrings
{
    [Required]
    public string DefaultConnection { get; set; } = string.Empty;
    public string ExternalDbConnection { get; set; } = string.Empty;

    public bool Validate(string cs)
    {
        try
        {
            using var connection = new SqlConnection(cs);
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
