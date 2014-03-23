using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CustomWindow;

namespace WebNinja
{
    public partial class MainWindow : StandardWindow
    {
        public static RoutedCommand CtrlW = new RoutedCommand();
        public static RoutedCommand CtrlT = new RoutedCommand();
        public static RoutedCommand CtrlD = new RoutedCommand();
        public static RoutedCommand CtrlB = new RoutedCommand();
        //public static RoutedCommand CtrlClick = new RoutedCommand();

        public MainWindow()
        {
            CtrlW.InputGestures.Add( new KeyGesture( Key.W, ModifierKeys.Control ) );
            CtrlT.InputGestures.Add( new KeyGesture( Key.T, ModifierKeys.Control ) );
            CtrlT.InputGestures.Add( new KeyGesture( Key.D, ModifierKeys.Control ) );
            CtrlT.InputGestures.Add( new KeyGesture( Key.B, ModifierKeys.Control ) );
            //CtrlClick.InputGestures.Add( new MouseGesture( MouseAction.LeftClick, ModifierKeys.Control ) );

            InitializeComponent();

            tabControl.Items.Clear();

            TabNew_MouseUp( null, null );
        }

        private void CtrlWDelegate( object sender, ExecutedRoutedEventArgs e )
        {
            var tabItem = tabControl.SelectedItem as TabItem;

            tabControl.Items.RemoveAt( tabControl.Items.IndexOf( tabItem ) );

            if( tabControl.Items.Count == 0 )
            {
                TabNew_MouseUp( null, null );
            }

            tabControl.Focus();
        }

        private void CtrlTDelegate( object sender, ExecutedRoutedEventArgs e )
        {
            TabNew_MouseUp( null, null );
        }

        private void CtrlDDelegate( object sender, ExecutedRoutedEventArgs e )
        {

        }

        private void CtrlBDelegate( object sender, ExecutedRoutedEventArgs e )
        {

        }

        private void Tab_Close( object sender, RoutedEventArgs e )
        {
            var menuItem = sender as MenuItem;
                menuItem.Click -= new RoutedEventHandler( Tab_Close );

            var contextMenu = menuItem.Parent as ContextMenu;
            var tabItem = contextMenu.PlacementTarget as TabItem;

            tabControl.Items.RemoveAt( tabControl.Items.IndexOf( tabItem ) );

            if( tabControl.Items.Count == 0 )
            {
                TabNew_MouseUp( null, null );
            }
        }
        
        private void Tab_CloseOthers( object sender, RoutedEventArgs e )
        {
            var menuItem = sender as MenuItem;
                menuItem.Click -= new RoutedEventHandler( Tab_CloseOthers );

            var contextMenu = menuItem.Parent as ContextMenu;
            var currentTabItem = contextMenu.PlacementTarget as TabItem;

            var tabsToRemove = new List<TabItem>();
            foreach( TabItem tabItem in tabControl.Items )
            {
                if( tabItem != currentTabItem ) //&& tabItem != NewTabItem )
                {
                    tabsToRemove.Add( tabItem );
                }
            }

            foreach( TabItem tabItem in tabsToRemove )
            {
                tabControl.Items.Remove( tabItem );
            }
        }
        
        private void Tab_Bookmark( object sender, RoutedEventArgs e )
        {
            var menuItem = sender as MenuItem;
                menuItem.Click -= new RoutedEventHandler( Tab_Bookmark );

            //var currentTabItem = sender as TabItem;

            // var browserControl = tabItem.Content as Controls.BrowserControl;
        }

        private void Tab_BookmarkAll( object sender, RoutedEventArgs e )
        {
            var menuItem = sender as MenuItem;
                menuItem.Click -= new RoutedEventHandler( Tab_BookmarkAll );

            //var currentTabItem = sender as TabItem;

            // var browserControl = tabItem.Content as Controls.BrowserControl;
        }

        private void TabNew_MouseUp( object sender, RoutedEventArgs e )
        {
            var closeMenuItem = new MenuItem();
                closeMenuItem.Header = "Close";
                closeMenuItem.Click += new RoutedEventHandler( Tab_Close );
            
            var closeOthersMenuItem = new MenuItem();
                closeOthersMenuItem.Header = "Close Others";
                closeOthersMenuItem.Click += new RoutedEventHandler( Tab_CloseOthers );

            var bookmarkMenuItem = new MenuItem();
                bookmarkMenuItem.Header = "Bookmark";
                bookmarkMenuItem.Click += new RoutedEventHandler( Tab_Bookmark );

            var bookmarkAllMenuItem = new MenuItem();
                bookmarkAllMenuItem.Header = "Bookmark All";
                bookmarkAllMenuItem.Click += new RoutedEventHandler( Tab_BookmarkAll );
            
            var contextMenu = new ContextMenu();
                contextMenu.Items.Add( closeMenuItem );
                contextMenu.Items.Add( closeOthersMenuItem );
                contextMenu.Items.Add( bookmarkMenuItem );
                contextMenu.Items.Add( bookmarkAllMenuItem );

            var browserControl = new Controls.BrowserControl();
                browserControl.TitleChange += BrowserControl_TitleChange;
                browserControl.NewWindowRequest += BrowserControl_NewWindowRequest;

            var tabItem = new TabItem();
                tabItem.Header = "New Page";
                tabItem.ContextMenu = contextMenu;
                tabItem.Content = browserControl;

            tabControl.Items.Add( tabItem );
            tabControl.SelectedIndex = tabControl.Items.IndexOf( tabItem );
            
            browserControl.Url = "http://www.bing.com";
        }

        void BrowserControl_TitleChange( object sender, Controls.BrowserControl.TitleChangeEventArgs e )
        {
            var browserControl = sender as Controls.BrowserControl;

            foreach( TabItem tabItem in tabControl.Items )
            {
                if( tabItem.Content as Controls.BrowserControl == browserControl )
                {
                    tabItem.Header = e.Title;
                    break;
                }
            }
        }

        void BrowserControl_NewWindowRequest( object sender, Controls.BrowserControl.NewWindowRequestEventArgs e )
        {
            TabNew_MouseUp( null, null );

            var tabItem = tabControl.SelectedItem as TabItem;
            
            var browserControl = tabItem.Content as Controls.BrowserControl;
                browserControl.Url = e.Url;
        }

        private void StandardWindow_MouseDown( object sender, MouseButtonEventArgs e )
        {
            if( e.LeftButton == MouseButtonState.Pressed )
                this.DragMove();
        }
    }
}
