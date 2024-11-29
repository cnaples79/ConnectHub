using System.Threading.Tasks;

namespace ConnectHub.App.Services
{
    public interface ILocationService
    {
        Task<(double Latitude, double Longitude)> GetCurrentLocationAsync();
        Task<string> GetLocationNameAsync(double latitude, double longitude);
        Task<bool> RequestLocationPermissionAsync();
    }
}
