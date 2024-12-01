using System.Diagnostics;

namespace ConnectHub.App.Services
{
    public class NavigationService : INavigationService
    {
        public async Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null)
        {
            try
            {
                Debug.WriteLine($"Attempting to navigate to: {route}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (parameters != null)
                    {
                        await Shell.Current.GoToAsync(route, true, parameters);
                    }
                    else
                    {
                        await Shell.Current.GoToAsync(route, true);
                    }
                });
                Debug.WriteLine($"Successfully navigated to: {route}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation failed to {route}: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task GoBackAsync()
        {
            try
            {
                Debug.WriteLine("Attempting to navigate back");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync("..", true);
                });
                Debug.WriteLine("Successfully navigated back");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation back failed: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task NavigateToRootAsync()
        {
            try
            {
                Debug.WriteLine("Attempting to navigate to root");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync("//main", true);
                });
                Debug.WriteLine("Successfully navigated to root");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation to root failed: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
