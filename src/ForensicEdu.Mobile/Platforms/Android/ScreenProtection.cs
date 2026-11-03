using Android.Views;
namespace ForensicEdu.Mobile.Services;
public sealed class ScreenProtection:IScreenProtection{public void SetProtected(bool enabled){var window=Platform.CurrentActivity?.Window;if(enabled)window?.AddFlags(WindowManagerFlags.Secure);else window?.ClearFlags(WindowManagerFlags.Secure);}}
