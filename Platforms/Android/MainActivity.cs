using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;

namespace Telegram.Bot
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize
            | ConfigChanges.Orientation
            | ConfigChanges.UiMode
            | ConfigChanges.ScreenLayout
            | ConfigChanges.SmallestScreenSize
            | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            ConfigureWindowInsetsListener();
        }

        private void ConfigureWindowInsetsListener()
        {
            var insetsListener = new SystemBarsInsetsListener();
            ViewCompat.SetOnApplyWindowInsetsListener(Window!.DecorView, insetsListener);
        }

        private class SystemBarsInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
        {
            public WindowInsetsCompat OnApplyWindowInsets(global::Android.Views.View view, WindowInsetsCompat insets)
            {
                var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());

                view.SetPadding(
                    left: 0,
                    top: 0,
                    right: 0,
                    bottom: systemBars.Bottom);

                return insets;
            }
        }
    }
}
