
using System;
using System.IO;
using System.Windows;

namespace WebNinja
{
    public partial class QuickFavorites : Window
    {
        String _url;
        String _favoritesPath;

        public QuickFavorites( String title, String url )
        {
            InitializeComponent();

            _url = url; 
            _favoritesPath = IeBookmarksManager.Instance.GetFavoritesFolder();

            TitleTextBox.Text = title;
            FolderTextBox.Text = Path.GetFileName( _favoritesPath );
        }

        private void RemoveButton_MouseUp( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            this.Close();
        }

        private void DoneButton_Click( object sender, RoutedEventArgs e )
        {
            IeBookmarksManager.Instance.CreateBookmark( TitleTextBox.Text, _url, _favoritesPath );

            this.Close();
        }

        private void EditButton_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        private void FolderTextBoxChooser_Click( object sender, RoutedEventArgs e )
        {
            var favoritesFolderBrowser = new IeBookmarksManager.FavoritesFolderBrowser();

            if( (bool) favoritesFolderBrowser.ShowDialog() )
            {
                _favoritesPath = favoritesFolderBrowser.SelectedFavoritePath;

                FolderTextBox.Text = Path.GetFileName( _favoritesPath );
            }
        }
    }
}
