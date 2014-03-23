
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#region Some magics with COM interfaces that will allow to better hook into the IE's engine

[ComImport, InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
[Guid( "6d5140c1-7436-11ce-8034-00aa006009fa" )]

internal interface IServiceProvider
{
    [return: MarshalAs( UnmanagedType.IUnknown )]
    object QueryService( ref Guid guidService, ref Guid riid );
}

#endregion

#region WpfWebBrowserExtenderException

public class WpfWebBrowserExtenderComBindException : Exception
{
    public WpfWebBrowserExtenderComBindException( String message ) :
        base( message )
    {}

    public WpfWebBrowserExtenderComBindException( String message, Exception ex ) :
        base( message, ex )
    { }
}

#endregion

#region WpfWebBrowserExtender

public class WpfWebBrowserExtender
{
    #region House keeping

    System.Windows.Controls.WebBrowser _wpfWebBrowser;
    SHDocVw.IWebBrowser2 _webBrowser2;
    SHDocVw.DWebBrowserEvents_Event _webBrowserEvents;
    SHDocVw.DWebBrowserEvents2_Event _webBrowserEvents2;
    HookEventType[] _delegateTypes;
    private delegate void HookForEventDelegate( SHDocVw.DWebBrowserEvents_Event webBrowserEvents, SHDocVw.DWebBrowserEvents2_Event webBrowserEvents2 );

    #endregion

    public WpfWebBrowserExtender( System.Windows.Controls.WebBrowser wpfWebBrowser )
    {
        _wpfWebBrowser = wpfWebBrowser;
        _wpfWebBrowser.Navigate( new Uri( "about:blank" ) );
    }

    ~WpfWebBrowserExtender()
    {
        Dictionary<HookEventType,HookForEventDelegate> 
            unHookForEvent = new Dictionary<HookEventType, HookForEventDelegate>();
            unHookForEvent.Add( HookEventType.DownloadBegin     , UnHookForEventDownloadBegin           );
            unHookForEvent.Add( HookEventType.ProgressChange    , UnHookForEventPageLoadProgressUpdate  );
            unHookForEvent.Add( HookEventType.DownloadComplete  , UnHookForEventDownloadComplete        );
            unHookForEvent.Add( HookEventType.NavigateError     , UnHookForEventNavigateError           );
            unHookForEvent.Add( HookEventType.NavigateComplete  , UnHookForEventNavigateComplete2       );
            unHookForEvent.Add( HookEventType.NewWindow         , UnHookForEventNewWindow               );

        foreach( HookEventType dt in _delegateTypes )
        {
            unHookForEvent[ dt ]( _webBrowserEvents, _webBrowserEvents2 );
        }
    }

    #region Events to catch from IE

    public enum HookEventType
    {
        DownloadBegin,
        ProgressChange,
        DownloadComplete,
        NavigateError,
        NavigateComplete,
        NewWindow
    }

    #region EventArgs

    public class DownloadBeginEventArgs : EventArgs
    {
        public String Uri;
    }

    public class ProgressChangeEventArgs : EventArgs
    {
        public String Uri;
        public int Progress;
        public int ProgressMax;
    }

    public class DownloadCompleteEventArgs : EventArgs
    {
        public String Uri;
        public String PageTitle;
    }

    public class NavigateErrorEventArgs : EventArgs
    {
        public String Uri;
    }

    public class NavigateCompleteEventArgs : EventArgs
    {
        public String Uri;
    }

    public class NewWindowEventArgs : EventArgs
    {
        public String url;
    }

    #endregion

    public event EventHandler<DownloadBeginEventArgs> DownloadBegin = delegate { };
    public event EventHandler<ProgressChangeEventArgs> ProgressChange = delegate { };
    public event EventHandler<DownloadCompleteEventArgs> DownloadComplete = delegate { };
    public event EventHandler<NavigateErrorEventArgs> NavigateError = delegate { };
    public event EventHandler<NavigateCompleteEventArgs> NavigateComplete = delegate { };
    public event EventHandler<NewWindowEventArgs> NewWindow = delegate { };

    #endregion

    #region HookForEvents

    public void HookForEvents( HookEventType[] delegateTypes )
    {
        _delegateTypes = delegateTypes;

        try
        {
            Guid ieServiceIdentifier = new Guid( "0002DF05-0000-0000-C000-000000000046" );
            Guid ieInterfaceIdentifier = typeof( SHDocVw.IWebBrowser2 ).GUID;

            IServiceProvider serviceProvider = (IServiceProvider) _wpfWebBrowser.Document;

            _webBrowser2 = (SHDocVw.IWebBrowser2) serviceProvider.QueryService( ref ieServiceIdentifier, ref ieInterfaceIdentifier );

            _webBrowserEvents = (SHDocVw.DWebBrowserEvents_Event) _webBrowser2;
            _webBrowserEvents2 = (SHDocVw.DWebBrowserEvents2_Event) _webBrowser2;
        }
        catch( Exception ex )
        {
            throw new WpfWebBrowserExtenderComBindException( String.Empty, ex );
        }

        Dictionary<HookEventType,HookForEventDelegate> 
            hookForEvent = new Dictionary<HookEventType, HookForEventDelegate>();
            hookForEvent.Add( HookEventType.DownloadBegin       , HookForEventDownloadBegin             );
            hookForEvent.Add( HookEventType.ProgressChange      , HookForEventPageLoadProgressUpdate    );
            hookForEvent.Add( HookEventType.DownloadComplete    , HookForEventDownloadComplete          );
            hookForEvent.Add( HookEventType.NavigateError       , HookForEventNavigateError             );
            hookForEvent.Add( HookEventType.NavigateComplete    , HookForEventNavigateComplete2         );
            hookForEvent.Add( HookEventType.NewWindow           , HookForEventNewWindow                 );
        
        foreach( HookEventType dt in _delegateTypes )
        {
            hookForEvent[ dt ]( _webBrowserEvents, _webBrowserEvents2 );
        }
    }

    #endregion

    #region IE hooks + unhooks

    #region hooks

    private void HookForEventDownloadBegin( SHDocVw.DWebBrowserEvents_Event webBrowserEvents, SHDocVw.DWebBrowserEvents2_Event webBrowserEvents2 )
    {
        webBrowserEvents2.DownloadBegin += new SHDocVw.DWebBrowserEvents2_DownloadBeginEventHandler( webBrowserEvents2_DownloadBegin );
    }

    private void HookForEventPageLoadProgressUpdate( SHDocVw.DWebBrowserEvents_Event webBrowserEvents, SHDocVw.DWebBrowserEvents2_Event webBrowserEvents2 )
    {
        webBrowserEvents2.ProgressChange += new SHDocVw.DWebBrowserEvents2_ProgressChangeEventHandler( webBrowserEvents2_ProgressChange );
    }

    private void HookForEventDownloadComplete( SHDocVw.DWebBrowserEvents_Event webBrowserEvents, SHDocVw.DWebBrowserEvents2_Event webBrowserEvents2 )
    {
        webBrowserEvents2.DownloadComplete += new SHDocVw.DWebBrowserEvents2_DownloadCompleteEventHandler( webBrowserEvents2_DownloadComplete );
    }

    private void HookForEventNavigateError( SHDocVw.DWebBrowserEvents_Event webBrowserEvents, SHDocVw.DWebBrowserEvents2_Event webBrowserEvents2 )
    {
        webBrowserEvents2.NavigateError += new SHDocVw.DWebBrowserEvents2_NavigateErrorEventHandler( webBrowserEvents2_NavigateError );
    }

    private void HookForEventNavigateComplete2( SHDocVw.DWebBrowserEvents_Event webBrowserEvents, SHDocVw.DWebBrowserEvents2_Event webBrowserEvents2 )
    {
        _wpfWebBrowser.Navigated += new System.Windows.Navigation.NavigatedEventHandler( wpfWebBrowser_Navigated );
    }

    private void HookForEventNewWindow( SHDocVw.DWebBrowserEvents_Event webBrowserEvents, SHDocVw.DWebBrowserEvents2_Event webBrowserEvents2 )
    {
        webBrowserEvents2.NewWindow3 += new SHDocVw.DWebBrowserEvents2_NewWindow3EventHandler( webBrowserEvents2_NewWindow3 );
    }

    #endregion

    #region unhooks

    private void UnHookForEventDownloadBegin( SHDocVw.DWebBrowserEvents_Event webBrowserEvents, SHDocVw.DWebBrowserEvents2_Event webBrowserEvents2 )
    {
        webBrowserEvents2.DownloadBegin -= new SHDocVw.DWebBrowserEvents2_DownloadBeginEventHandler( webBrowserEvents2_DownloadBegin );
    }

    private void UnHookForEventPageLoadProgressUpdate( SHDocVw.DWebBrowserEvents_Event webBrowserEvents, SHDocVw.DWebBrowserEvents2_Event webBrowserEvents2 )
    {
        webBrowserEvents2.ProgressChange -= new SHDocVw.DWebBrowserEvents2_ProgressChangeEventHandler( webBrowserEvents2_ProgressChange );
    }

    private void UnHookForEventDownloadComplete( SHDocVw.DWebBrowserEvents_Event webBrowserEvents, SHDocVw.DWebBrowserEvents2_Event webBrowserEvents2 )
    {
        webBrowserEvents2.DownloadComplete -= new SHDocVw.DWebBrowserEvents2_DownloadCompleteEventHandler( webBrowserEvents2_DownloadComplete );
    }

    private void UnHookForEventNavigateError( SHDocVw.DWebBrowserEvents_Event webBrowserEvents, SHDocVw.DWebBrowserEvents2_Event webBrowserEvents2 )
    {
        webBrowserEvents2.NavigateError -= new SHDocVw.DWebBrowserEvents2_NavigateErrorEventHandler( webBrowserEvents2_NavigateError );
    }

    private void UnHookForEventNavigateComplete2( SHDocVw.DWebBrowserEvents_Event webBrowserEvents, SHDocVw.DWebBrowserEvents2_Event webBrowserEvents2 )
    {
        _wpfWebBrowser.Navigated -= new System.Windows.Navigation.NavigatedEventHandler( wpfWebBrowser_Navigated );
    }

    private void UnHookForEventNewWindow( SHDocVw.DWebBrowserEvents_Event webBrowserEvents, SHDocVw.DWebBrowserEvents2_Event webBrowserEvents2 )
    {
        webBrowserEvents2.NewWindow3 -= new SHDocVw.DWebBrowserEvents2_NewWindow3EventHandler( webBrowserEvents2_NewWindow3 );
    }

    #endregion

    void webBrowserEvents2_DownloadBegin()
    {
        var eventArgs = new DownloadBeginEventArgs();
            eventArgs.Uri = _webBrowser2.LocationURL;

        DownloadBegin( this, eventArgs );
    }

    void webBrowserEvents2_ProgressChange( int Progress, int ProgressMax )
    {
        var eventArgs = new ProgressChangeEventArgs();
            eventArgs.Progress = Progress;
            eventArgs.ProgressMax = ProgressMax;
            eventArgs.Uri = _webBrowser2.LocationURL;

        ProgressChange( this, eventArgs );
    }

    void webBrowserEvents2_DownloadComplete()
    {
        var htmlDocument = _webBrowser2.Document as mshtml.HTMLDocument;

        var eventArgs = new DownloadCompleteEventArgs();
            eventArgs.Uri = _webBrowser2.LocationURL;
            eventArgs.PageTitle = htmlDocument.title;
                    
        DownloadComplete( this, eventArgs );
    }

    void webBrowserEvents2_NavigateError( object pDisp, ref object URL, ref object Frame, ref object StatusCode, ref bool Cancel )
    {
        var eventArgs = new NavigateErrorEventArgs();
            eventArgs.Uri = URL as String;

        NavigateError( this, eventArgs );
    }

    void wpfWebBrowser_Navigated( object sender, System.Windows.Navigation.NavigationEventArgs e )
    {
        var eventArgs = new NavigateCompleteEventArgs();
            eventArgs.Uri = e.Uri.ToString();

        NavigateComplete( this, eventArgs );
    }

    void webBrowserEvents2_NewWindow3( ref object ppDisp, ref bool Cancel, uint dwFlags, string bstrUrlContext, string bstrUrl )
    {
        Cancel = true;

        var eventArgs = new NewWindowEventArgs();
            eventArgs.url = bstrUrl;
        
        NewWindow( this, eventArgs );
    }

    #endregion

    #region Methods to call on IE

    public String Title
    {
        get
        {
            var htmlDocument = _webBrowser2.Document as mshtml.HTMLDocument;

            return htmlDocument.title;
        }

        private set{}
    }

    public String Url
    {
        get
        {
            var htmlDocument = _webBrowser2.Document as mshtml.HTMLDocument;

            return htmlDocument.location.href;
        }

        private set{}
    }

    public void Navigate( String url )
    {
        _webBrowser2.Navigate( url );
    }

    public void Refresh()
    {
        _webBrowser2.Refresh();
    }

    public void Stop()
    {
        _webBrowser2.Stop();
    }

    public void GoBack()
    {
        _webBrowser2.GoBack();
    }

    public void GoForward()
    {
        _webBrowser2.GoForward();
    }

    #endregion
}

#endregion
