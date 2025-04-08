namespace Presentation.Controllers.Http;

public class ErrorResponse
{
    public string Message { get; set; } = "An error occurred.";

    public string? Details { get; set; }

    public int StatusCode { get; set; }
}