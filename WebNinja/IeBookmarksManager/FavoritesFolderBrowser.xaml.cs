
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace IeBookmarksManager
{
    public partial class FavoritesFolderBrowser : Window
    {
        private readonly object _dummyNode = null;

        public String SelectedFavoritePath
        {
            get;
            set;
        }

        public FavoritesFolderBrowser()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            SelectedFavoritePath = Instance.GetFavoritesFolder();

            if( !(bool) RootCheckbox.IsChecked && FavoritesTree.SelectedItem != null )
            {
                SelectedFavoritePath = ( FavoritesTree.SelectedItem as TreeViewItem ).Tag as String;
            }

            DialogResult = true;
        }

        private void Cancel_Click( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
        }

        private void Window_Loaded( object sender, RoutedEventArgs e )
        {
            foreach( string folder in Directory.GetDirectories( Instance.GetFavoritesFolder() ) )
            {
                var item = new TreeViewItem();
                    item.Header = Path.GetFileName( folder );
                    item.Tag = folder;
                    item.Items.Add( _dummyNode );
                    item.Expanded += Folder_Expanded;

                FavoritesTree.Items.Add( item );
            }
        }

        void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == _dummyNode)
            {
                item.Items.Clear();
                try
                {					
                    foreach (string dir in Directory.GetDirectories(item.Tag as string))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = new DirectoryInfo(dir).Name;
                        subitem.Tag = dir;
                        subitem.Items.Add(_dummyNode);
                        subitem.Expanded += Folder_Expanded;
                        item.Items.Add(subitem);
                    }
                }
                catch( Exception )
                {}
            }
        }
    }
}
