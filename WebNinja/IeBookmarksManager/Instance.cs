
using System;
using System.Collections.Generic;
using System.IO;

namespace IeBookmarksManager
{
    public class Instance
    {
        public static String GetFavoritesFolder()
        {
            return System.Environment.GetFolderPath( Environment.SpecialFolder.Favorites );
        }

        public static void CreateBookmark( String title, String url, String subFolder )
        {
            var favoritesFolder = System.Environment.GetFolderPath( Environment.SpecialFolder.Favorites );

            var urlFileName = String.Format( "{0}.url", title );
            var fullUrlFileName = Path.Combine( favoritesFolder, subFolder );
                fullUrlFileName = Path.Combine( fullUrlFileName, urlFileName );

            using( var streamWritter = System.IO.File.CreateText( fullUrlFileName ) )
            {
                streamWritter.WriteLine( "[InternetShortcut]" );
                streamWritter.WriteLine( "URL=" + url );
            }
        }

        public static String ReadUrlFromBookmark( String bookmark )
        {
            string url = string.Empty;

            using( var streamReader = System.IO.File.OpenText( bookmark ) )
            {
                while( !streamReader.EndOfStream )
                {
                    String line = streamReader.ReadLine();
                    if( line.StartsWith( "URL" ) )
                    {
                        url = line.Replace( "URL=", String.Empty );
                        break;
                    }
                }
            }

            return url;
        }

        public static Dictionary<String,bool> GetBookmarksForFolder( String folder )
        {
            var bookmarks = new Dictionary<String, bool>();

            var favoritesFolder = System.Environment.GetFolderPath( Environment.SpecialFolder.Favorites );

            if( !String.IsNullOrEmpty( folder ) )
            {
                favoritesFolder = Path.Combine( favoritesFolder, folder );
            }

            //foreach( String file in Directory.GetFiles( favoritesFolder ) ) //, String.Empty, SearchOption.TopDirectoryOnly ) )
            //{
            //    bookmarks.Add( file, File.Exists( Path.Combine(favoritesFolder,file) ) );
            //}

            var directoryInfo = new DirectoryInfo( favoritesFolder );
            foreach( FileSystemInfo fileSystemInfo  in directoryInfo.GetFileSystemInfos() )
            {
                bookmarks.Add( fileSystemInfo.FullName, !(fileSystemInfo is FileInfo) );
            }

            return bookmarks;
        }
    }
}
