namespace Infrastructure.Configurations;

public class PagedConfigurationResponse
{
    public IEnumerable<ConfigurationItem>? Items { get; set; }

    public string? PageToken { get; set; }
}
