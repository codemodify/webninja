
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WebNinja.Controls
{
    public partial class BrowserControl : UserControl
    {
        WpfWebBrowserExtender _wpfWebBrowserHelper;

        public class TitleChangeEventArgs : EventArgs
        {
            public String Title;
        }

        public class NewWindowRequestEventArgs : EventArgs
        {
            public String Url;
        }

        public event EventHandler<TitleChangeEventArgs> TitleChange = delegate { };

        public event EventHandler<NewWindowRequestEventArgs> NewWindowRequest = delegate { };

        public String Url
        {
            get
            {
                return AddressBox.Text;
            }

            set
            {
                if( !String.IsNullOrEmpty( value ) )
                {
                    AddressBox.Text = value;
                    GoButton_Click( null, null );
                }
            }
        }

        public BrowserControl() :
            base()
        {
            InitializeComponent();

            BackButton.IsEnabled = IE.CanGoBack;
            ForwardButton.IsEnabled = IE.CanGoForward;

            _wpfWebBrowserHelper = new WpfWebBrowserExtender( IE );
            _wpfWebBrowserHelper.HookForEvents
            (
                new WpfWebBrowserExtender.HookEventType[] 
                {
                    WpfWebBrowserExtender.HookEventType.DownloadBegin,
                    WpfWebBrowserExtender.HookEventType.ProgressChange,
                    WpfWebBrowserExtender.HookEventType.DownloadComplete,
                    WpfWebBrowserExtender.HookEventType.NavigateError,
                    WpfWebBrowserExtender.HookEventType.NavigateComplete,
                    WpfWebBrowserExtender.HookEventType.NewWindow
                }
            );

            _wpfWebBrowserHelper.DownloadBegin += WpfWebBrowserHelper_DownloadBegin;
            _wpfWebBrowserHelper.ProgressChange += WpfWebBrowserHelper_ProgressChange;
            _wpfWebBrowserHelper.DownloadComplete += WpfWebBrowserHelper_DownloadComplete;
            _wpfWebBrowserHelper.NavigateError += WpfWebBrowserHelper_NavigateError;
            _wpfWebBrowserHelper.NavigateComplete += WpfWebBrowserHelper_NavigateComplete;
            _wpfWebBrowserHelper.NewWindow += WpfWebBrowserHelper_NewWindow;

            AddressBox.Focus();
            AddressBox.SelectAll();
        }

        #region IE delegates

        void NavigationHelperOpenBingQuery( String stringToQuery )
        {
            var bingQueryString = String.Format( "http://www.bing.com/search?q={0}", stringToQuery );

            _wpfWebBrowserHelper.Navigate( bingQueryString );
        }

        void WpfWebBrowserHelper_DownloadBegin( object sender, WpfWebBrowserExtender.DownloadBeginEventArgs e )
        {
            StopButton.Visibility = Visibility.Visible;
            RefreshButton.Visibility = Visibility.Hidden;

            StatusText.Visibility = Visibility.Visible;
        }

        void WpfWebBrowserHelper_ProgressChange( object sender, WpfWebBrowserExtender.ProgressChangeEventArgs e )
        {
            double percentDownloaded = e.ProgressMax != 0 ? e.Progress * 100 / e.ProgressMax : 100;

            StatusText.Width = AddressBox.ActualWidth * percentDownloaded / 100;
        }

        void WpfWebBrowserHelper_DownloadComplete( object sender, WpfWebBrowserExtender.DownloadCompleteEventArgs e )
        {
            StopButton.Visibility = Visibility.Hidden;
            RefreshButton.Visibility = Visibility.Visible;

            BackButton.IsEnabled = IE.CanGoBack;
            ForwardButton.IsEnabled = IE.CanGoForward;

            StatusText.Visibility = Visibility.Hidden;

            if( !String.IsNullOrEmpty( e.PageTitle ) )
            {
                const int maxTitleLength = 20;
                String pageTitle = e.PageTitle;
                if( pageTitle.Length > maxTitleLength )
                {
                    pageTitle = pageTitle.Substring( 0, maxTitleLength - 3 ) + "...";
                }

                var evetArgs = new TitleChangeEventArgs();
                    evetArgs.Title = pageTitle;

                TitleChange( this, evetArgs );
            }
        }

        void WpfWebBrowserHelper_NavigateError( object sender, WpfWebBrowserExtender.NavigateErrorEventArgs e )
        {
            NavigationHelperOpenBingQuery( e.Uri );
        }

        void WpfWebBrowserHelper_NavigateComplete( object sender, WpfWebBrowserExtender.NavigateCompleteEventArgs e )
        {
            AddressBox.Text = e.Uri;
        }

        void WpfWebBrowserHelper_NewWindow( object sender, WpfWebBrowserExtender.NewWindowEventArgs e )
        {
            var eventArgs = new NewWindowRequestEventArgs();
                eventArgs.Url = e.url;

            NewWindowRequest( this, eventArgs );
        }

        #endregion

        #region GoButton_Click()

        private void GoButton_Click( object sender, System.Windows.RoutedEventArgs e )
        {
            String address = AddressBox.Text;

            if( String.IsNullOrEmpty( address ) )
                return;

            try
            {
                if( !address.StartsWith( "http" ) && ( address.Split( new char[]{' '} ).Length > 1 ) )
                {
                    NavigationHelperOpenBingQuery( address );
                }
                else if( !address.StartsWith( "http" ) )
                {
                    address = String.Format( "http://{0}", address );
                }

                _wpfWebBrowserHelper.Navigate( address );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.ToString() );
            }
        }
        
        #endregion

        #region AddressBox_KeyUp()

        private void AddressBox_KeyUp( object sender, System.Windows.Input.KeyEventArgs e )
        {
            if( e.Key == System.Windows.Input.Key.Return )
                GoButton_Click( null, null );
        }

        #endregion

        #region RefreshButton_Click()

        private void RefreshButton_Click( object sender, RoutedEventArgs e )
        {
            _wpfWebBrowserHelper.Refresh();
        }

        #endregion

        #region StopButton_Click()

        private void StopButton_Click( object sender, RoutedEventArgs e )
        {
            _wpfWebBrowserHelper.Stop();
        }

        #endregion

        #region BackButton_Click()

        private void BackButton_Click( object sender, RoutedEventArgs e )
        {
            _wpfWebBrowserHelper.GoBack();
        }

        #endregion

        #region ForwardButton_Click()

        private void ForwardButton_Click( object sender, RoutedEventArgs e )
        {
            _wpfWebBrowserHelper.GoForward();
        }

        #endregion

        #region NewTabButton_Click

        private void NewTabButton_Click( object sender, RoutedEventArgs e )
        {
            var eventArgs = new NewWindowRequestEventArgs();

            NewWindowRequest( this, eventArgs );
        }

        #endregion

        private void BookmarksButton_Click( object sender, RoutedEventArgs e )
        {
            var button = sender as Button;

            Point buttonPosition = new Point( 0, 0 );
            Point buttonPositionInScreenCoordinates = button.PointToScreen( buttonPosition );
            
            var quickFavorites = new QuickFavorites( _wpfWebBrowserHelper.Title, _wpfWebBrowserHelper.Url );
                quickFavorites.Left = buttonPositionInScreenCoordinates.X - 17;
                quickFavorites.Top = buttonPositionInScreenCoordinates.Y + 20;
                quickFavorites.ShowDialog();
        }

        private void SettingsButton_Click( object sender, RoutedEventArgs e )
        {

        }

        private MenuItem MenuBuilderHelper1( String urlFilePath )
        {
            var urlIcon = new Image();
                urlIcon.Source = new BitmapImage( new Uri( "pack://application:,,,/WebNinja;component/star.png" ) );

            var menuItem = new MenuItem();
                menuItem.Header = Path.GetFileNameWithoutExtension( urlFilePath );
                menuItem.Tag = urlFilePath;
                menuItem.Click += new RoutedEventHandler( BookmarkClicked );
                menuItem.Icon = urlIcon;

            return menuItem;
        }
        private MenuItem MenuBuilderHelper2( String folderPath )
        {
            var folderIcon = new Image();
                folderIcon.Source = new BitmapImage( new Uri( "pack://application:,,,/WebNinja;component/folder.png" ) );

            var rootMenuItem = new MenuItem();
                rootMenuItem.Header = Path.GetFileNameWithoutExtension( folderPath );
                rootMenuItem.Tag = folderPath;
                rootMenuItem.Icon = folderIcon;

            Dictionary<String,bool> bookmarks = IeBookmarksManager.Instance.GetBookmarksForFolder( folderPath );

            foreach( String bookmark in bookmarks.Keys )
            {
                MenuItem menuItem = null;

                if( bookmarks[ bookmark ] ) // folder
                {
                    menuItem = MenuBuilderHelper2( bookmark );
                }
                else
                {
                    menuItem = MenuBuilderHelper1( bookmark );
                }

                rootMenuItem.Items.Add( menuItem );
            }

            return rootMenuItem;
        }

        private void OpenBookmarksButton_Click( object sender, RoutedEventArgs e )
        {
            Dictionary<String,bool> bookmarks = IeBookmarksManager.Instance.GetBookmarksForFolder( IeBookmarksManager.Instance.GetFavoritesFolder() );

            var menu = new ContextMenu();

            foreach( String bookmark in bookmarks.Keys )
            {
                MenuItem menuItem = null;

                if( bookmarks[ bookmark ] ) // folder
                {
                    menuItem = MenuBuilderHelper2( bookmark );
                }
                else
                {
                    menuItem = MenuBuilderHelper1( bookmark );
                }

                menu.Items.Add( menuItem );
            }

            menu.PlacementTarget = sender as Button;
            menu.IsOpen = true;
        }

        private void BookmarkClicked( object sender, RoutedEventArgs e )
        {
            var menuItem = sender as MenuItem;
                menuItem.Click -= new RoutedEventHandler( BookmarkClicked );

            string urlFile = menuItem.Tag as String;
            if( File.Exists( urlFile ) )
            {
                this.Url = IeBookmarksManager.Instance.ReadUrlFromBookmark( urlFile );
            }
        }
    }
}
