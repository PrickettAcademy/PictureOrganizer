using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PictureOrganizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Organizer organizer = new Organizer();

        public MainWindow()
        {
            InitializeComponent();
            organizer.Initialize(@"c:\pictureOrganizer\", @"c:\users\jpricket\pictures");
            if (!organizer.FilesLoaded())
            {
                organizer.LoadFiles();
            }
            NextPicture();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            organizer.Save();
            base.OnClosing(e);
        }

        private void NextPicture()
        {
            while (true)
            {
                string filepath = organizer.GetNextPictureFile();
                if (filepath == null)
                {
                    filepathLabel.Content = "";
                    ShowStatusMessage("Done.");
                    return;
                }

                try
                {
                    if (!organizer.CheckForDuplicateFile())
                    {
                        LoadPicture(filepath);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ShowStatusMessage("Unable to load file. (skip it)");
                    return;
                }
            }
        }

        private void ShowStatusMessage(string message)
        {
            imageControl.Source = null;
            statusText.Content = message;
        }


        private void LoadPicture(string filepath)
        {
            filepathLabel.Content = filepath;
            BitmapImage bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.CacheOption = BitmapCacheOption.OnLoad;
            bmi.UriSource = new Uri(filepath, UriKind.Absolute);
            bmi.EndInit();
            imageControl.Source = bmi;
        }

        private void Keep_Click(object sender, RoutedEventArgs e)
        {
            organizer.KeepPictureFile();
            NextPicture();
        }

        private void Archive_Click(object sender, RoutedEventArgs e)
        {
            organizer.ArchivePictureFile();
            NextPicture();
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            organizer.SkipPictureFile();
            NextPicture();
        }
    }
}
