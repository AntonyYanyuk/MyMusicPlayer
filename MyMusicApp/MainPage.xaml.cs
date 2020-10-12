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

        bool isSongsListRemoved = false;

        #endregion


        public MainPage()
        {
            this.InitializeComponent();

            mediaPlayer = new MediaPlayer();

            storage = new List<MediaSource>();

            this.VolumeSlider.Value = 100;

            currentTrackDuration = new TimeSpan();

            mediaPlayer.MediaOpened += Media_Opened;

            mediaTimeline = new MediaTimelineController();

            mediaTimeline.PositionChanged += MediaTimelineController_PositionChanged;

            ApplicationView.PreferredLaunchViewSize = new Size(1500, 1000);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            
        }

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
                isSongsListRemoved = false;

                foreach (Windows.Storage.StorageFile file in files)
                {
                    this.CurrentPlaylist.Items.Add(file.Name.ToString());

                    media = MediaSource.CreateFromStorageFile(file);

                    storage.Add(media);
                }

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
                    mediaPlayer.Source = storage[storage.Count - 1];
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
                isSongsListRemoved = true;
                this.CurrentPlaylist.Items.Clear();
                storage.Clear();                
            }                   
        }
    }
}

