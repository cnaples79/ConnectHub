using System.Windows.Input;
using ConnectHub.App.Services;
using ConnectHub.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Maui.Storage;

namespace ConnectHub.App.ViewModels
{
    public partial class NewPostViewModel : ObservableObject
    {
        private readonly IApiService _apiService;
        private readonly ILogger<NewPostViewModel> _logger;
        private readonly IConnectivity _connectivity;
        private readonly IFilePicker _filePicker;
        private readonly IGeolocation _geolocation;
        private readonly ILocationService _locationService;
        private const int MaxContentLength = 500;
        private const int MaxImageSize = 10 * 1024 * 1024; // 10MB

        [ObservableProperty]
        private string _content = string.Empty;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private string _selectedImagePath;

        [ObservableProperty]
        private string _locationName;

        [ObservableProperty]
        private bool _isPosting;

        [ObservableProperty]
        private bool _isLocationEnabled;

        [ObservableProperty]
        private double? _latitude;

        [ObservableProperty]
        private double? _longitude;

        public NewPostViewModel(
            IApiService apiService,
            ILogger<NewPostViewModel> logger,
            IConnectivity connectivity,
            IFilePicker filePicker,
            IGeolocation geolocation,
            ILocationService locationService)
        {
            _apiService = apiService;
            _logger = logger;
            _connectivity = connectivity;
            _filePicker = filePicker;
            _geolocation = geolocation;
            _locationService = locationService;
        }

        [RelayCommand]
        private async Task PickImage()
        {
            if (IsPosting)
                return;

            try
            {
                var options = new PickOptions
                {
                    PickerTitle = "Select a photo",
                    FileTypes = FilePickerFileType.Images
                };

                var result = await _filePicker.PickAsync(options);
                if (result != null)
                {
                    var fileInfo = new FileInfo(result.FullPath);
                    if (fileInfo.Length > MaxImageSize)
                    {
                        StatusMessage = "Selected image is too large. Maximum size is 10MB.";
                        return;
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(result.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Any(x => x == extension))
                    {
                        StatusMessage = "Invalid image format. Allowed formats: JPG, PNG, GIF";
                        return;
                    }

                    SelectedImagePath = result.FullPath;
                    StatusMessage = "Image selected successfully";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error picking image");
                StatusMessage = "Failed to select image. Please try again.";
            }
        }

        [RelayCommand]
        private void ClearImage()
        {
            if (IsPosting)
                return;

            SelectedImagePath = null;
            StatusMessage = "Image cleared";
        }

        [RelayCommand]
        private async Task ToggleLocation()
        {
            if (IsPosting)
                return;

            try
            {
                if (!IsLocationEnabled)
                {
                    var hasPermission = await _locationService.RequestLocationPermissionAsync();
                    if (!hasPermission)
                    {
                        StatusMessage = "Location permission is required to add location to your post";
                        return;
                    }

                    var (lat, lon) = await _locationService.GetCurrentLocationAsync();
                    Latitude = lat;
                    Longitude = lon;
                    LocationName = await _locationService.GetLocationNameAsync(lat, lon);
                    IsLocationEnabled = true;
                    StatusMessage = "Location added successfully";
                }
                else
                {
                    Latitude = null;
                    Longitude = null;
                    LocationName = null;
                    IsLocationEnabled = false;
                    StatusMessage = "Location removed";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling location");
                StatusMessage = "Failed to get location. Please try again.";
                IsLocationEnabled = false;
            }
        }

        [RelayCommand]
        private async Task CreatePost()
        {
            if (IsPosting)
                return;

            if (string.IsNullOrWhiteSpace(Content))
            {
                StatusMessage = "Please enter some content for your post";
                return;
            }

            if (Content.Length > MaxContentLength)
            {
                StatusMessage = $"Content exceeds maximum length of {MaxContentLength} characters";
                return;
            }

            if (!_connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
            {
                StatusMessage = "No internet connection. Please check your network settings.";
                return;
            }

            try
            {
                IsPosting = true;
                StatusMessage = "Creating post...";

                Stream? imageStream = null;
                string? fileName = null;

                if (!string.IsNullOrEmpty(SelectedImagePath))
                {
                    imageStream = File.OpenRead(SelectedImagePath);
                    fileName = Path.GetFileName(SelectedImagePath);
                }

                using (imageStream)
                {
                    var post = await _apiService.CreatePostAsync(Content, imageStream, fileName, Latitude, Longitude);
                    if (post != null)
                    {
                        Content = string.Empty;
                        SelectedImagePath = null;
                        Latitude = null;
                        Longitude = null;
                        LocationName = null;
                        IsLocationEnabled = false;
                        StatusMessage = "Post created successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                StatusMessage = "Failed to create post. Please try again.";
            }
            finally
            {
                IsPosting = false;
            }
        }
    }
}
