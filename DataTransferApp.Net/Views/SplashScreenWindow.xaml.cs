using System;
using System.Windows;
using System.Windows.Threading;

namespace DataTransferApp.Net.Views;

/// <summary>
/// Interaction logic for SplashScreenWindow.xaml.
/// </summary>
public partial class SplashScreenWindow : Window
{
    private readonly Action _onSplashComplete;
    private readonly DispatcherTimer _timer;

    public SplashScreenWindow(Action onSplashComplete)
    {
        InitializeComponent();
        _onSplashComplete = onSplashComplete ?? throw new ArgumentNullException(nameof(onSplashComplete));

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _timer.Tick += Timer_Tick;
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _timer.Stop();
        _onSplashComplete();
        Close();
    }
}