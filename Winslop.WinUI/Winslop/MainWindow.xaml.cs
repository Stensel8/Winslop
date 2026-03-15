using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using Winslop.Helpers;
using Winslop.Services;
using Winslop.Views;

namespace Winslop
{
    public sealed partial class MainWindow : Window
    {
        private readonly Dictionary<string, Type> _pages = new()
        {
            { "Home",     typeof(FeaturesPage) },
            { "Apps",     typeof(AppsPage) },
            { "Install",  typeof(InstallPage) },
            { "Tools",    typeof(ToolsPage) },
            { "Settings", typeof(SettingsPage) }
        };

        // Services initialized in constructor
        private NavigationService _nav = null!;

        private MenuActionRouter _router = null!;
        private LoggerDisplay _loggerDisplay = null!;
        private LoggerActions? _logActions;

        private bool _closeHandled;

        public MainWindow()
        {
            InitializeComponent();

            // Title bar
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            // Migration dialog deferred until XAML root is ready
            this.Activated += OnFirstActivated;

            // Version info tooltip on Home button
            ToolTipService.SetToolTip(navBtnFeatures, WindowsVersion.GetDisplayString());

            // -- Services -----------------------------------------
            var navButtons = new[] { navBtnFeatures, navBtnApps, navBtnInstall, navBtnTools, navBtnSettings };

            _nav = new NavigationService(
                ContentFrame, navButtons, _pages, (FrameworkElement)Content);
            _router = new MenuActionRouter(ContentFrame);
            _loggerDisplay = new LoggerDisplay(txtLogger, scrollLogger, DispatcherQueue);
            _logActions = new LoggerActions();

            // Wire log actions to FeaturesPage tree after each navigation
            ContentFrame.Navigated += OnContentFrameNavigated;

            // Donation dialog on close (unless opted out)
            Closed += MainWindow_Closed;

            // Navigate to the default page after everything is set up
            DispatcherQueue.TryEnqueue(
                Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                () => _nav.NavigateToDefault("Home"));
        }

        private async void OnFirstActivated(object sender, WindowActivatedEventArgs args)
        {
            this.Activated -= OnFirstActivated;

            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, async () =>
            {
                await MigrationHelper.ShowIfNeededAsync(this.Content.XamlRoot);
            });
        }

        // -- Navigation -------------------------------------------

        // Handle nav button clicks and navigate and update visual state
        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
                _nav.NavigateTo(tag);
        }

        // Called after every page navigation. Wires up the LogActions
        // provider and adjusts UI state (menus, buttons, visibility) per page.
        private void OnContentFrameNavigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            var page = ContentFrame.Content;

            // Give LogActions access to the feature tree when FeaturesPage is active
            if (page is FeaturesPage fp)
                _logActions?.SetFeaturesItemsProvider(() => fp.RootItems);

            // -- Button/menu state per page ---------------------------
            bool isFeatures = page is FeaturesPage;
            bool isTools = page is ToolsPage;
            bool isSettings = page is SettingsPage;
            bool hasActions = page is IMainActions && !isTools && !isSettings;

            // Menu items only relevant on FeaturesPage
            MenuUndo.IsEnabled = isFeatures;
            MenuExport.IsEnabled = isFeatures;
            MenuImport.IsEnabled = isFeatures;
            MenuToggle.IsEnabled = !isTools && !isSettings;

            // Analyze/Apply buttons: hidden on Settings + Tools
            bottomButtons.Visibility = (isSettings || isTools)
                ? Visibility.Collapsed : Visibility.Visible;

            // Search bar, logger: hidden on Settings only
            searchBar.Visibility = isSettings ? Visibility.Collapsed : Visibility.Visible;
            logSeparator.Visibility = isSettings ? Visibility.Collapsed : Visibility.Visible;
            scrollLogger.Visibility = isSettings ? Visibility.Collapsed : Visibility.Visible;
        }

        // -- Button handlers --------------------------------------

        private async void btnAnalyze_Click(object sender, RoutedEventArgs e)
        {
            var actions = _router.CurrentActions();
            if (actions == null) return;

            btnAnalyze.IsEnabled = btnFix.IsEnabled = false;
            try { await actions.AnalyzeAsync(); }
            finally { btnAnalyze.IsEnabled = btnFix.IsEnabled = true; }
        }

        private async void btnFix_Click(object sender, RoutedEventArgs e)
        {
            var actions = _router.CurrentActions();
            if (actions == null) return;

            btnAnalyze.IsEnabled = btnFix.IsEnabled = false;
            try { await actions.FixAsync(); }
            finally { btnAnalyze.IsEnabled = btnFix.IsEnabled = true; }
        }

        // -- Search -----------------------------------------------

        private void textSearch_TextChanged(object sender, TextChangedEventArgs e)
            => _router.CurrentSearchable()?.ApplySearch(textSearch.Text);

        private void textSearch_GotFocus(object sender, RoutedEventArgs e)
            => textSearch.Text = string.Empty;

        // -- More options menu ---------------------------------------

        private void MenuToggleAll_Click(object sender, RoutedEventArgs e)
            => _router.ToggleAll();

        private void MenuUndo_Click(object sender, RoutedEventArgs e)
            => _router.Undo();

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
            => _router.Refresh();

        private void MenuExport_Click(object sender, RoutedEventArgs e)
            => _router.Export(NativeMethods.ShowSaveDialog());

        private void MenuImport_Click(object sender, RoutedEventArgs e)
            => _router.Import(NativeMethods.ShowOpenDialog());

        // -- Log actions ------------------------------------------
        private void MenuInspectLog_Click(object sender, RoutedEventArgs e)
            => _logActions?.AnalyzeOnline(ExternalLinks.LogInspectorUrl);

        private void MenuCopyLog_Click(object sender, RoutedEventArgs e)
            => _logActions?.CopyToClipboard();

        private void MenuClearLog_Click(object sender, RoutedEventArgs e)
            => _logActions?.Clear();

        private void MenuLogChecked_Click(object sender, RoutedEventArgs e)
            => _logActions?.LogCheckedFeatures();

        private void MenuLogUnchecked_Click(object sender, RoutedEventArgs e)
            => _logActions?.LogUncheckedLeafFeatures();

        private void MenuLogSummary_Click(object sender, RoutedEventArgs e)
            => _logActions?.LogFeatureSummary();

        // -- Support links (Ko-fi / PayPal flyout) ------------------

        private void MenuKofi_Click(object sender, RoutedEventArgs e)
            => ExternalLinks.OpenKofi();

        private void MenuPaypal_Click(object sender, RoutedEventArgs e)
            => ExternalLinks.OpenPaypal();

        // -- Closing ----------------------------------------------

        private async void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            if (_closeHandled || DonationHelper.HasDonated())
                return;

            args.Handled = true;
            await DonationHelper.ShowDonationDialogAsync(Content.XamlRoot);
            _closeHandled = true;
            Close();
        }
    }
}