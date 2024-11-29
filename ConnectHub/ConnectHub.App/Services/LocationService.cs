using Microsoft.Maui.Devices.Sensors;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ConnectHub.App.Services
{
    public class LocationService : ILocationService
    {
        private readonly IGeolocation _geolocation;
        private readonly IGeocoding _geocoding;

        public LocationService(IGeolocation geolocation, IGeocoding geocoding)
        {
            _geolocation = geolocation;
            _geocoding = geocoding;
        }

        public async Task<(double Latitude, double Longitude)> GetCurrentLocationAsync()
        {
            try
            {
                var location = await _geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(5)
                });

                if (location != null)
                {
                    return (location.Latitude, location.Longitude);
                }

                throw new Exception("Could not get current location.");
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get current location.", ex);
            }
        }

        public async Task<string> GetLocationNameAsync(double latitude, double longitude)
        {
            try
            {
                var placemarks = await _geocoding.GetPlacemarksAsync(latitude, longitude);
                var placemark = placemarks?.FirstOrDefault();

                if (placemark != null)
                {
                    var locationParts = new List<string>();

                    if (!string.IsNullOrEmpty(placemark.Locality))
                        locationParts.Add(placemark.Locality);

                    if (!string.IsNullOrEmpty(placemark.AdminArea))
                        locationParts.Add(placemark.AdminArea);

                    if (!string.IsNullOrEmpty(placemark.CountryName))
                        locationParts.Add(placemark.CountryName);

                    return string.Join(", ", locationParts);
                }

                return "Unknown Location";
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get location name.", ex);
            }
        }

        public async Task<bool> RequestLocationPermissionAsync()
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                return status == PermissionStatus.Granted;
            }
            catch (Exception ex)
            {
                throw new Exception("Error requesting location permission", ex);
            }
        }
    }
}
