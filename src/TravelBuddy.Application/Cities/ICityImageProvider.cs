using System.Threading;
using System.Threading.Tasks;

namespace TravelBuddy.Cities;

public interface ICityImageProvider
{
    Task<string?> GetImageUrlAsync(
        string cityName,
        string? wikiDataId,
        CancellationToken cancellationToken = default);
}
