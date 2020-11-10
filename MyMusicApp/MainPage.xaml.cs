using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Pickers.Provider;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.Media.MediaProperties;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using MyMusicApp;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Globalization.DateTimeFormatting;
using System.Runtime.Serialization;
using Windows.ApplicationModel;
using Windows.UI.WindowManagement;
using Windows.ApplicationModel.Activation;



// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MyMusicApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {    

        #region Fields

        MediaPlayer mediaPlayer;

        MediaSource media;

        List<MediaSource> storage;

        TimeSpan currentTrackDuration;

        MediaTimelineController mediaTimeline;

        bool _isFullScreen = true;

        bool isSongsListRemoved = true;

        #endregion

        public MainPage()
        {

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500, 500));
            this.InitializeComponent();
           
            mediaPlayer = new MediaPlayer();

            storage = new List<MediaSource>();

            this.VolumeSlider.Value = 100;

            currentTrackDuration = new TimeSpan();

            mediaPlayer.MediaOpened += Media_Opened;

            mediaTimeline = new MediaTimelineController();

            mediaTimeline.PositionChanged += MediaTimelineController_PositionChanged;

            this.SizeChanged += Page_SizeChanged;

            ApplicationView.PreferredLaunchViewSize = new Size(930, 550);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            mySplitView.PaneOpened += PaneOpened;
            mySplitView.PaneClosed += PaneClosed;
        }

        #region WindowSizeChanges 

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width >= 930)
            {
                SoundProgressSlider.Width = 530;
                VolumeSlider.Width = 150;
                CurrentTrackTimeProgress.Margin = new Thickness(0, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 0, 0);
                CurrentSong.Margin = new Thickness(0, 0, 0, 5);
                StackVolumeSlider.Margin = new Thickness(0, 0, 30, 8);
                CurrentSong.Width = 500;
                PanelMediaButt.Margin = new Thickness(0, 0, 0, 0);
                SoundProgressSlider.Margin = new Thickness(0, 10, 0, 0);
                AppTitle.Margin = new Thickness(0, 0, 0, 0);
            }

            if (e.NewSize.Width < 930)
            {
                SoundProgressSlider.Width = 490;
                VolumeSlider.Width = 140;
                CurrentTrackTimeProgress.Margin = new Thickness(20, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 20, 0);
                CurrentSong.Margin = new Thickness(40, 0, 0, 5);
            }

            if (e.NewSize.Width < 895)
            {
                SoundProgressSlider.Width = 446;
                VolumeSlider.Width = 130;
                VolumeSlider.Margin = new Thickness(5, 0, 0, 5);
                CurrentTrackTimeProgress.Margin = new Thickness(42, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 42, 0);
                CurrentSong.Margin = new Thickness(80, 0, 0, 7);
                CurrentSong.Width = 500;
            }

            if (e.NewSize.Width < 842)
            {
                SoundProgressSlider.Width = 410;
                VolumeSlider.Width = 120;
                CurrentTrackTimeProgress.Margin = new Thickness(60, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 60, 0);
                CurrentSong.Margin = new Thickness(20, 0, 0, 5);
                StackVolumeSlider.Margin = new Thickness(0, 0, 40, 8);
                CurrentSong.Width = 400;
            }

            if (e.NewSize.Width < 820)
            {
                AppTitle.Margin = new Thickness(0, 0, 0, 0);
            }

                if (e.NewSize.Width < 780)
            {
                SoundProgressSlider.Width = 370;
                VolumeSlider.Width = 110;
                CurrentTrackTimeProgress.Margin = new Thickness(81, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 81, 0);
                CurrentSong.Margin = new Thickness(60, 0, 0, 5);
                StackVolumeSlider.Margin = new Thickness(0, 0, 60, 8);
                AppTitle.Margin = new Thickness(0, 0, 40, 0);
            }

            if (e.NewSize.Width < 765)
            {
                SoundProgressSlider.Width = 300;
                VolumeSlider.Width = 90;
                CurrentTrackTimeProgress.Margin = new Thickness(115, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 115, 0);
                CurrentSong.Margin = new Thickness(60, 0, 0, 5);
                StackVolumeSlider.Margin = new Thickness(0, 0, 70, 8);
                AppTitle.Margin = new Thickness(0, 0, 55, 0);
            }

            if (e.NewSize.Width < 755)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 80, 8);
                AppTitle.Margin = new Thickness(0, 0, 65, 0);
            }


            if (e.NewSize.Width < 745)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 90, 8);
                AppTitle.Margin = new Thickness(0, 0, 75, 0);
            }


            if (e.NewSize.Width < 735)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 100, 8);
                AppTitle.Margin = new Thickness(0, 0, 85, 0);
            }


            if (e.NewSize.Width < 725)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 110, 8);
                AppTitle.Margin = new Thickness(0, 0, 95, 0);
            }

            if (e.NewSize.Width < 715)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 120, 8);
                AppTitle.Margin = new Thickness(0, 0, 105, 0);
            }

            if (e.NewSize.Width < 705)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 130, 8);
                AppTitle.Margin = new Thickness(0, 0, 115, 0);
            }

            if (e.NewSize.Width < 695)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 140, 8);
                AppTitle.Margin = new Thickness(0, 0, 125, 0);
            }

            if (e.NewSize.Width < 685)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 150, 8);
                GridPlayManagePanel.Margin = new Thickness(0, 0, 0, 0);
                AppTitle.Margin = new Thickness(0, 0, 135, 0);
            }

            if (e.NewSize.Width < 675)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 155, 8);
                GridPlayManagePanel.Margin = new Thickness(0, 0, 5, 0);
                AppTitle.Margin = new Thickness(0, 0, 145, 0);
            }

            if (e.NewSize.Width < 667)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 165, 8);
                GridPlayManagePanel.Margin = new Thickness(0, 0, 15, 0);
                CurrentSong.Margin = new Thickness(60, 0, 0, 5);
                CurrentSong.Width = 400;
                AppTitle.Margin = new Thickness(0, 0, 155, 0);
            }

            if (e.NewSize.Width < 657)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 175, 8);
                GridPlayManagePanel.Margin = new Thickness(0, 0, 25, 0);
                CurrentSong.Margin = new Thickness(5, 0, 0, 5);
                CurrentSong.Width = 350;
                AppTitle.Margin = new Thickness(0, 0, 165, 0);
            }

            if (e.NewSize.Width < 649)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 185, 8);
                GridPlayManagePanel.Margin = new Thickness(0, 0, 30, 0);
                PanelMediaButt.Margin = new Thickness(0, 0, 0, 0);
                SoundProgressSlider.Margin = new Thickness(0, 10, 0, 0);
                CurrentTrackTimeProgress.Margin = new Thickness(115, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 115, 0);
                AppTitle.Margin = new Thickness(0, 0, 175, 0);
            }

            if (e.NewSize.Width < 640)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 195, 17);
                SoundProgressSlider.Margin = new Thickness(0, 10, 10, 0);
                PanelMediaButt.Margin = new Thickness(0, 0, 10, 0);
                CurrentTrackTimeProgress.Margin = new Thickness(110, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 120, 0);
                CurrentSong.Margin = new Thickness(0, 0, 5, 5);
                AppTitle.Margin = new Thickness(0, 0, 185, 0);
            }

            if (e.NewSize.Width < 630)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 205, 17);
                SoundProgressSlider.Margin = new Thickness(0, 10, 20, 0);
                PanelMediaButt.Margin = new Thickness(0, 0, 20, 0);
                CurrentTrackTimeProgress.Margin = new Thickness(105, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 125, 0);
                CurrentSong.Margin = new Thickness(0, 0, 15, 5);
                AppTitle.Margin = new Thickness(0, 0, 195, 0);
            }

            if (e.NewSize.Width < 620)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 215, 17);
                SoundProgressSlider.Margin = new Thickness(0, 10, 30, 0);
                PanelMediaButt.Margin = new Thickness(0, 0, 30, 0);
                CurrentTrackTimeProgress.Margin = new Thickness(100, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 130, 0);
                CurrentSong.Margin = new Thickness(0, 0, 25, 5);
                AppTitle.Margin = new Thickness(0, 0, 215, 0);
            }

            if (e.NewSize.Width < 610)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 225, 17);
                SoundProgressSlider.Margin = new Thickness(0, 10, 40, 0);
                PanelMediaButt.Margin = new Thickness(0, 0, 40, 0);
                CurrentTrackTimeProgress.Margin = new Thickness(95, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 135, 0);
                CurrentSong.Margin = new Thickness(0, 0, 35, 5);
                AppTitle.Margin = new Thickness(0, 0, 225, 0);
            }

            if (e.NewSize.Width < 600)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 235, 17);
                SoundProgressSlider.Margin = new Thickness(0, 10, 50, 0);
                PanelMediaButt.Margin = new Thickness(0, 0, 50, 0);
                CurrentTrackTimeProgress.Margin = new Thickness(90, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 140, 0);
                CurrentSong.Margin = new Thickness(0, 0, 45, 5);
                AppTitle.Margin = new Thickness(0, 0, 235, 0);
            }

            if (e.NewSize.Width < 590)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 245, 17);
                SoundProgressSlider.Margin = new Thickness(0, 10, 60, 0);
                PanelMediaButt.Margin = new Thickness(0, 0, 60, 0);
                CurrentTrackTimeProgress.Margin = new Thickness(85, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 145, 0);
                CurrentSong.Margin = new Thickness(0, 0, 55, 5);
                AppTitle.Margin = new Thickness(0, 0, 245, 0);
            }

            if (e.NewSize.Width < 580)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 255, 17);
                SoundProgressSlider.Margin = new Thickness(0, 10, 70, 0);
                PanelMediaButt.Margin = new Thickness(0, 0, 70, 0);
                CurrentTrackTimeProgress.Margin = new Thickness(80, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 150, 0);
                CurrentSong.Margin = new Thickness(0, 0, 65, 5);
                SoundProgressSlider.Width = 300;
                VolumeSlider.Width = 90;
                AppTitle.Margin = new Thickness(0, 0, 255, 0);
            }

            if (e.NewSize.Width < 570)
            {
                SoundProgressSlider.Width = 240;
                VolumeSlider.Width = 70;
                StackVolumeSlider.Margin = new Thickness(0, 0, 265, 17);
                SoundProgressSlider.Margin = new Thickness(0, 10, 80, 0);
                PanelMediaButt.Margin = new Thickness(0, 0, 80, 0);
                CurrentTrackTimeProgress.Margin = new Thickness(105, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 185, 0);
                CurrentSong.Margin = new Thickness(0, 0, 30, 5);
                AppTitle.Margin = new Thickness(0, 0, 265, 0);
            }

            if (e.NewSize.Width < 560)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 275, 17);
                SoundProgressSlider.Margin = new Thickness(0, 10, 90, 0);
                PanelMediaButt.Margin = new Thickness(0, 0, 90, 0);
                CurrentTrackTimeProgress.Margin = new Thickness(100, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 190, 0);
                CurrentSong.Margin = new Thickness(0, 0, 40, 5);
                AppTitle.Margin = new Thickness(0, 0, 275, 0);
            }

            if (e.NewSize.Width < 550)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 285, 17);
                SoundProgressSlider.Margin = new Thickness(0, 10, 100, 0);
                PanelMediaButt.Margin = new Thickness(0, 0, 100, 0);
                CurrentTrackTimeProgress.Margin = new Thickness(95, 17, 0, 0);
                TrackDuration.Margin = new Thickness(0, 17, 195, 0);
                CurrentSong.Margin = new Thickness(0, 0, 50, 5);
                AppTitle.Margin = new Thickness(0, 0, 300, 0);
                AppTitle.HorizontalAlignment = HorizontalAlignment.Center;
            }

            if (e.NewSize.Width < 540)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 295, 17);
                GridPlayManagePanel.Margin = new Thickness(0, 0, 30, 0);
                AppTitle.Margin = new Thickness(0, 0, 315, 0);
                AppTitle.HorizontalAlignment = HorizontalAlignment.Center;
            }

            if (e.NewSize.Width < 530)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 305, 17);
                GridPlayManagePanel.Margin = new Thickness(0, 0, 40, 0);
                AppTitle.HorizontalAlignment = HorizontalAlignment.Left;
                AppTitle.Margin = new Thickness(100, 0, 0, 0);
            }

            if (e.NewSize.Width < 520)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 315, 17);
                GridPlayManagePanel.Margin = new Thickness(0, 0, 50, 0);
                AppTitle.Margin = new Thickness(90, 0, 350, 0);
            }

            if (e.NewSize.Width < 510)
            {
                StackVolumeSlider.Margin = new Thickness(0, 0, 325, 17);
                GridPlayManagePanel.Margin = new Thickness(0, 0, 60, 0);
                AppTitle.Margin = new Thickness(80, 0, 360, 0);
            }
        }

        #endregion


        private async void AddTracs_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".flac");
            picker.FileTypeFilter.Add(".wav");
            picker.FileTypeFilter.Add(".aiff");
            picker.FileTypeFilter.Add(".m4a");
            picker.FileTypeFilter.Add(".ogg");
            picker.FileTypeFilter.Add(".alac");
            picker.FileTypeFilter.Add(".wma");

            var files = await picker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                this.CurrentSongsScroll.Visibility = 0;
                this.CurrentSongsScroll.Visibility = 0;
                this.SongsListTitle.Visibility = 0;
                this.PanelButtAddRemove.Visibility = 0;
                isSongsListRemoved = false;

                foreach (Windows.Storage.StorageFile file in files)
                {
                    this.CurrentPlaylist.Items.Add(file.Name.ToString());

                    media = MediaSource.CreateFromStorageFile(file);

                    storage.Add(media);
                }
                isSongsListRemoved = false;

                mediaPlayer.Source = storage[0];
                mediaPlayer.TimelineController = mediaTimeline;

                CurrentSongInitialize();
            }
        }

        public void CurrentSongInitialize()
        {
            this.CurrentSong.Text = this.CurrentPlaylist.Items[0].ToString();
            this.CurrentPlaylist.SelectedItem = this.CurrentPlaylist.Items[0];
            this.TrackDuration.Visibility = Visibility.Visible;
        }

        private void SelectedMusicListsItem(object sender, SelectionChangedEventArgs e)
        {
            if (isSongsListRemoved == false)
            {
                this.CurrentSong.Text = this.CurrentPlaylist.SelectedItem.ToString();

                mediaPlayer.Source = storage[this.CurrentPlaylist.SelectedIndex];


                if (mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
                {
                    mediaTimeline.Start();
                    mediaTimeline.Pause();
                }

                if (mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    mediaTimeline.Pause();

                    mediaPlayer.Source = storage[this.CurrentPlaylist.SelectedIndex];

                    mediaTimeline.Start();
                }               
            }       
        }

        #region MusicPlayManagePanel 

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (isSongsListRemoved == false)
            {
                if (this.CurrentPlaylist.SelectedIndex == default)
                {
                    mediaPlayer.Source = storage[0];
                }
                mediaTimeline.Resume();
            }        
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (isSongsListRemoved == false)
            {
                mediaTimeline.Pause();
            }              
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (isSongsListRemoved == false)
            {
                bool mediaPlayerCurrentState = false;

                if (mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    mediaPlayerCurrentState = true;
                }

                if (mediaPlayer.Source == storage[0])
                {
                    mediaPlayer.Source = storage[0];
                }
                else
                {
                    mediaPlayer.Source = storage[this.CurrentPlaylist.SelectedIndex - 1];
                    this.CurrentPlaylist.SelectedIndex = this.CurrentPlaylist.SelectedIndex - 1;
                    this.CurrentSong.Text = this.CurrentPlaylist.SelectedItem.ToString();
                }

                if (mediaPlayerCurrentState == true)
                {
                    mediaTimeline.Start();
                }
                else
                {
                    mediaTimeline.Start();
                    mediaTimeline.Pause();
                }
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (isSongsListRemoved == false)
            {
                bool mediaPlayerCurrentState = false;

                if (mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    mediaPlayerCurrentState = true;
                }

                if (mediaPlayer.Source == storage[storage.Count - 1])
                {
                    mediaPlayer.Source = storage[0];
                    this.CurrentPlaylist.SelectedItem = this.CurrentPlaylist.Items[0];
                    this.CurrentSong.Text = this.CurrentPlaylist.SelectedItem.ToString();
                }

                else
                {
                    mediaPlayer.Source = storage[this.CurrentPlaylist.SelectedIndex + 1];
                    this.CurrentPlaylist.SelectedIndex = this.CurrentPlaylist.SelectedIndex + 1;
                    this.CurrentSong.Text = this.CurrentPlaylist.SelectedItem.ToString();
                }

                if (mediaPlayerCurrentState == true)
                {
                    mediaTimeline.Start();
                }
                else
                {
                    mediaTimeline.Start();
                    mediaTimeline.Pause();
                }
            }            
        }

        #endregion

        private void VolumeValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.VolumeSlider.Value = e.NewValue;

            if (this.VolumeSlider.Value != null)
            {
                mediaPlayer.Volume = this.VolumeSlider.Value / 100;
            }
        }

        private async void Media_Opened(MediaPlayer sender, object e)
        {
            currentTrackDuration = sender.PlaybackSession.NaturalDuration;

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.SoundProgressSlider.Minimum = 0;
                this.SoundProgressSlider.Maximum = currentTrackDuration.TotalSeconds;
                this.SoundProgressSlider.StepFrequency = 1;
                this.TrackDuration.Text = currentTrackDuration.ToString(@"mm\:ss");
            });
        }

        private async void MediaTimelineController_PositionChanged(MediaTimelineController sender, object args)
        {

            if (currentTrackDuration != TimeSpan.Zero)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.SoundProgressSlider.Value = sender.Position.TotalSeconds;
                    this.CurrentTrackTimeProgress.Text = sender.Position.ToString(@"mm\:ss");
                });
            }
        }

        private void Track_Rewind(object sender, RangeBaseValueChangedEventArgs e)
        {
            mediaTimeline.Position = TimeSpan.FromSeconds(e.NewValue);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {         

            if (this.CurrentPlaylist.Items.Count > 1 && this.CurrentPlaylist.SelectedIndex != 0)
            {
                int removeIndex = this.CurrentPlaylist.SelectedIndex;              
                this.CurrentPlaylist.SelectedIndex = removeIndex - 1;
                this.CurrentPlaylist.Items.RemoveAt(removeIndex);                             
                storage.RemoveAt(removeIndex);
            }
            else if (this.CurrentPlaylist.SelectedIndex == 0 && this.CurrentPlaylist.Items.Count > 1)
            {
                int removeIndex = this.CurrentPlaylist.SelectedIndex;
                this.CurrentPlaylist.SelectedIndex = removeIndex + 1;
                this.CurrentPlaylist.Items.RemoveAt(removeIndex);                            
                storage.RemoveAt(removeIndex);
            }
            else
            {
                if (mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    mediaTimeline.Pause();
                    mediaPlayer.Source = null;
                }
                int removeIndex = this.CurrentPlaylist.SelectedIndex;
                this.CurrentSong.Text = " ";
                this.TrackDuration.Visibility = Visibility.Collapsed;
                this.CurrentTrackTimeProgress.Text = "00:00";
                this.SoundProgressSlider.Value = 0;
                this.CurrentSongsScroll.Visibility = Visibility.Collapsed;
                this.SongsListTitle.Visibility = Visibility.Collapsed;
                this.PanelButtAddRemove.Visibility = Visibility.Collapsed;
                isSongsListRemoved = true;
                this.CurrentPlaylist.Items.Clear();
                storage.Clear();                
            }                   
        }

        private void HamburgerMenu_Click(object sender, RoutedEventArgs e)
        {
            mySplitView.IsPaneOpen = !mySplitView.IsPaneOpen;
        }

        private void PaneOpened(SplitView sender, object e)
        {
                     
            if ( sender.IsPaneOpen == true)
            { 
                AddingTracs.Visibility = Visibility.Visible;
                SavingPlaylist.Visibility = Visibility.Visible; 
                mySplitView.Style = Application.Current.Resources["SplitViewOpenedStyle"] as Style;
                HamburgerMenu.Style = Application.Current.Resources["HamburgerButt2"] as Style;
            }     
               
        }
        private void PaneClosed(SplitView sender, object e)
        {
            if (sender.IsPaneOpen == false)
            {
                AddingTracs.Visibility = Visibility.Collapsed;
                SavingPlaylist.Visibility = Visibility.Collapsed;
                mySplitView.Style = Application.Current.Resources["SplitViewClosedStyle"] as Style;
                HamburgerMenu.Style = Application.Current.Resources["HamburgerButt1"] as Style;
            }
        }

    }
}

