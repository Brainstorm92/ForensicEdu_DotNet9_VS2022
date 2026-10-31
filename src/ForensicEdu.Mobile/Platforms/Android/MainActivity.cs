using Android.App;
using Android.Content.PM;
using Android.OS;
namespace ForensicEdu.Mobile;

[Activity(Theme="@style/Maui.SplashTheme",MainLauncher=true,LaunchMode=LaunchMode.SingleTop,ConfigurationChanges=ConfigChanges.ScreenSize|ConfigChanges.Orientation|ConfigChanges.UiMode|ConfigChanges.ScreenLayout|ConfigChanges.SmallestScreenSize|ConfigChanges.Density)]
public class MainActivity:MauiAppCompatActivity
{
 protected override void OnCreate(Bundle? savedInstanceState)
 {
  base.OnCreate(savedInstanceState);
  if(Window is {} window){window.SetBackgroundDrawable(new Android.Graphics.Drawables.ColorDrawable(Android.Graphics.Color.Rgb(14,19,33)));var controller=AndroidX.Core.View.WindowCompat.GetInsetsController(window,window.DecorView);if(controller!=null){controller.AppearanceLightStatusBars=false;controller.AppearanceLightNavigationBars=false;}}
 }
}
