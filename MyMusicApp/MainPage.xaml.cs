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
using Windows.Graphics.Display;
using Windows.UI.Core;



// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MyMusicApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {    

        #region Fields

        MediaPlayer _mediaPlayer;

        MediaSource _media;

        List<MediaSource> _storage;

        TimeSpan _currentTrackDuration;

        MediaTimelineController _mediaTimeline;

        bool _isSongsListRemoved = true;

        bool _isCompactSize = false;

        bool _isRandomPlayingMode = false;

        bool _isRepeatPlayingMode = false;

        Size _sizeBeforeCompactRezime;

        List<int> _randomIndexesList;

        #endregion

        public MainPage()
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500, 140));
            this.InitializeComponent();

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(UserLayout);  

            _mediaPlayer = new MediaPlayer();

            _mediaPlayer.MediaEnded += MediaEnded;

            _storage = new List<MediaSource>();

            this.VolumeSlider.Value = 100;

            _currentTrackDuration = new TimeSpan();

            _mediaPlayer.MediaOpened += Media_Opened;

            _mediaTimeline = new MediaTimelineController();

            _mediaTimeline.PositionChanged += MediaTimelineController_PositionChanged;

            this.SizeChanged += Page_SizeChanged;

            _sizeBeforeCompactRezime = new Size(930, 550);

            ApplicationView.PreferredLaunchViewSize = new Size(930, 550);

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            mySplitView.PaneOpened += PaneOpened;
            mySplitView.PaneClosed += PaneClosed;

            _randomIndexesList = new List<int>();
        }


        #region WindowSizeChanged

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 820)
            {
                double stackVolumeShift = 820 - e.NewSize.Width + 10;
                StackVolumeSlider.Margin = new Thickness(0, 0, stackVolumeShift, 16);             
            }

            if (e.NewSize.Width >= 930)
            {
               SoundProgressSlider.Width = 530;
                VolumeSlider.Width = 150;
                CurrentSong.Width = 600;          
                StackVolumeSlider.Margin = new Thickness(0, 0, 20, 16);
                AppTitle.Margin = new Thickness(0, 0, 0, 0);
            }

            if (e.NewSize.Width < 900 && e.NewSize.Width > 850)
            {
                SoundProgressSlider.Width = 470;
                VolumeSlider.Width = 140;
                CurrentSong.Width = 560;            
            }

            if (e.NewSize.Width < 850 && e.NewSize.Width > 820)
            {
                SoundProgressSlider.Width = 410;
                VolumeSlider.Width = 125;
                CurrentSong.Width = 500;
                StackVolumeSlider.Margin = new Thickness(0, 0, 20, 16);
            }          

            if (e.NewSize.Width < 800 && e.NewSize.Width > 750)
            {
                SoundProgressSlider.Width = 390;
                VolumeSlider.Width = 120;
                CurrentSong.Width = 500;
            }

            if (e.NewSize.Width < 750 && e.NewSize.Width > 610)
            {
                SoundProgressSlider.Width = 350;
                VolumeSlider.Width = 110;
            }

              if (e.NewSize.Width < 610 && e.NewSize.Width > 565)
            {
                SoundProgressSlider.Width = 310;
                VolumeSlider.Width = 100;
            }

            if (e.NewSize.Width < 565)
            {
                SoundProgressSlider.Width = 270;
                VolumeSlider.Width = 90;
                CurrentSong.Width = 500;
            }

            if (ApplicationView.GetForCurrentView().IsFullScreen == true)
            {
                CompactSymbol.Symbol = Symbol.BackToWindow;
                _isCompactSize = false;
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
                _isSongsListRemoved = false;

                foreach (Windows.Storage.StorageFile file in files)
                {
                    this.CurrentPlaylist.Items.Add(file.Name.ToString());

                    _media = MediaSource.CreateFromStorageFile(file);
               
                    _storage.Add(_media);          
                }

                if (_isRandomPlayingMode == true && _storage.Count > 2)
                {
                    _randomIndexesList.Clear();
                    GetRandomIndexes();
                }

                _isSongsListRemoved = false;

                _mediaPlayer.Source = _storage[0];
                _mediaPlayer.TimelineController = _mediaTimeline;

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
            if (_isSongsListRemoved == false)
            {
                this.CurrentSong.Text = this.CurrentPlaylist.SelectedItem.ToString();

                _mediaPlayer.Source = _storage[this.CurrentPlaylist.SelectedIndex];


                if (_mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
                {
                    _mediaTimeline.Start();
                    _mediaTimeline.Pause();
                }

                if (_mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    _mediaTimeline.Pause();

                    _mediaPlayer.Source = _storage[this.CurrentPlaylist.SelectedIndex];

                    _mediaTimeline.Start();
                }               
            }       
        }

        #region MusicPlayManagePanel 

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (_isSongsListRemoved == false)
            {
                if (this.CurrentPlaylist.SelectedIndex == default)
                {
                    _mediaPlayer.Source = _storage[0];
                }
                _mediaTimeline.Resume();
            }        
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (_isSongsListRemoved == false)
            {
                _mediaTimeline.Pause();
            }              
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (_isSongsListRemoved == false)
            {
                bool mediaPlayerCurrentState = false;

                if (_mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    mediaPlayerCurrentState = true;
                }

                if (_isRandomPlayingMode == false)
                {
                    if (_mediaPlayer.Source == _storage[0])
                    {
                        _mediaPlayer.Source = _storage[0];
                    }

                    else
                    {
                        _mediaPlayer.Source = _storage[this.CurrentPlaylist.SelectedIndex - 1];
                        this.CurrentPlaylist.SelectedIndex = this.CurrentPlaylist.SelectedIndex - 1;
                        this.CurrentSong.Text = this.CurrentPlaylist.SelectedItem.ToString();
                    }
                }
               
                else
                {                   
                     int previousTrack = _randomIndexesList.IndexOf(CurrentPlaylist.SelectedIndex);
                     _mediaPlayer.Source = _storage[previousTrack];
                     CurrentPlaylist.SelectedItem = CurrentPlaylist.Items[previousTrack];
                     this.CurrentSong.Text = this.CurrentPlaylist.SelectedItem.ToString();                  
                }

                if (mediaPlayerCurrentState == true)
                {
                    _mediaTimeline.Start();
                }
                else
                {
                    _mediaTimeline.Start();
                    _mediaTimeline.Pause();
                }
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (_isSongsListRemoved == false)
            {
                bool mediaPlayerCurrentState = false;

                if (_mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    mediaPlayerCurrentState = true;
                }

                if (_isRandomPlayingMode == false)
                {
                    if (_mediaPlayer.Source == _storage[_storage.Count - 1])
                    {
                        _mediaPlayer.Source = _storage[0];
                        this.CurrentPlaylist.SelectedItem = this.CurrentPlaylist.Items[0];
                        this.CurrentSong.Text = this.CurrentPlaylist.SelectedItem.ToString();
                    }
                    else
                    {
                        _mediaPlayer.Source = _storage[this.CurrentPlaylist.SelectedIndex + 1];
                        this.CurrentPlaylist.SelectedIndex = this.CurrentPlaylist.SelectedIndex + 1;
                        this.CurrentSong.Text = this.CurrentPlaylist.SelectedItem.ToString();
                    }
                }
               
                else
                {   
                      _mediaPlayer.Source = _storage[_randomIndexesList[CurrentPlaylist.SelectedIndex]];
                      this.CurrentPlaylist.SelectedIndex = _randomIndexesList[CurrentPlaylist.SelectedIndex];
                       this.CurrentSong.Text = this.CurrentPlaylist.SelectedItem.ToString();                                                                            
                }

                if (mediaPlayerCurrentState == true)
                {
                    _mediaTimeline.Start();
                }
                else
                {
                    _mediaTimeline.Start();
                    _mediaTimeline.Pause();
                }
            }            
        }

        #endregion

        private void VolumeValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.VolumeSlider.Value = e.NewValue;

                _mediaPlayer.Volume = this.VolumeSlider.Value / 100;       
        }

        private async void Media_Opened(MediaPlayer sender, object e)
        {
            _currentTrackDuration = sender.PlaybackSession.NaturalDuration;

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.SoundProgressSlider.Minimum = 0;
                this.SoundProgressSlider.Maximum = _currentTrackDuration.TotalSeconds;
                this.SoundProgressSlider.StepFrequency = 1;
                this.TrackDuration.Text = _currentTrackDuration.ToString(@"mm\:ss");
            });
        }

        private async void MediaTimelineController_PositionChanged(MediaTimelineController sender, object args)
        {

            if (_currentTrackDuration != TimeSpan.Zero)
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
            _mediaTimeline.Position = TimeSpan.FromSeconds(e.NewValue);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {         
            if (this.CurrentPlaylist.Items.Count > 1 && this.CurrentPlaylist.SelectedIndex != 0)
            {         
                    int removeIndex = this.CurrentPlaylist.SelectedIndex;
                    this.CurrentPlaylist.SelectedIndex = removeIndex - 1;
                    this.CurrentPlaylist.Items.RemoveAt(removeIndex);
                    _storage.RemoveAt(removeIndex);

                if (_isRandomPlayingMode == true)
                {
                    _randomIndexesList.Clear();
                    GetRandomIndexes();
                }
            }
            else if (this.CurrentPlaylist.SelectedIndex == 0 && this.CurrentPlaylist.Items.Count > 1)
            {
                int removeIndex = this.CurrentPlaylist.SelectedIndex;
                    this.CurrentPlaylist.SelectedIndex = removeIndex + 1;
                    this.CurrentPlaylist.Items.RemoveAt(removeIndex);
                    _storage.RemoveAt(removeIndex);

                if (_isRandomPlayingMode == true)
                {
                    _randomIndexesList.Clear();
                    GetRandomIndexes();
                }                           
            }
            else
            {
                if (_mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    _mediaTimeline.Pause();
                    _mediaPlayer.Source = null;
                }
                this.CurrentSong.Text = " ";
                this.TrackDuration.Visibility = Visibility.Collapsed;
                this.CurrentTrackTimeProgress.Text = "00:00";
                this.SoundProgressSlider.Value = 0;
                this.CurrentSongsScroll.Visibility = Visibility.Collapsed;
                this.SongsListTitle.Visibility = Visibility.Collapsed;
                this.PanelButtAddRemove.Visibility = Visibility.Collapsed;
                _isSongsListRemoved = true;
                this.CurrentPlaylist.Items.Clear();
                if (_randomIndexesList.Count > 0)
                {
                    _randomIndexesList.Clear();
                }
                _storage.Clear();                
            }                   
        }

        #region SplitViewSection

        private void HamburgerMenu_Click(object sender, RoutedEventArgs e)
        {
            mySplitView.IsPaneOpen = !mySplitView.IsPaneOpen;
        }

        private void PaneOpened(SplitView sender, object e)
        {
                     
            if ( sender.IsPaneOpen == true)
            { 
                AddingTracs.Visibility = Visibility.Visible;
                //SavingPlaylist.Visibility = Visibility.Visible; 
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
        #endregion

        private void CompactSizeButt_Click(object sender, RoutedEventArgs e)
        {
            if (_isCompactSize == false)
            {
                ApplicationView.GetForCurrentView().TryResizeView(new Size(500, 140));
                CompactSymbol.Symbol = Symbol.FullScreen;
                _isCompactSize = true;                        
            }
            else
            {
                ApplicationView.GetForCurrentView().TryResizeView(_sizeBeforeCompactRezime);
                CompactSymbol.Symbol = Symbol.BackToWindow;
                _isCompactSize = false;
            }           
        }

        private void RandomTrackButt_Click(object sender, RoutedEventArgs e)
        {
            if (_isRandomPlayingMode == false)
            {
                RandomTrackButt.Style = Application.Current.Resources["RandomTrackButtStyle2"] as Style;

                if(_storage.Count > 2)
                {
                    GetRandomIndexes();
                }

                _isRandomPlayingMode = true;
            }

            else
            {
                RandomTrackButt.Style = Application.Current.Resources["RandomTrackButtStyle1"] as Style;
                _isRandomPlayingMode = false;
            }
        }  
   
        private void GetRandomIndexes()
        {
            var arrayRandom = Enumerable.Range(0, _storage.Count).ToArray();

            var rand = new Random();
            for (int i = arrayRandom.Length - 1; i >= 0; i--)
            {
                int j = rand.Next(i);
                var temp = arrayRandom[i];
                arrayRandom[i] = arrayRandom[j];
                arrayRandom[j] = temp;
            }
        
            for (int i = 0; i < arrayRandom.Length; i++)
                _randomIndexesList.Add(arrayRandom[i]);
        }

        private async void MediaEnded(MediaPlayer sender, object e)
        {          
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                SongEnded();
            });
        }
        
        private void SongEnded()
        {
            if (_isRepeatPlayingMode == true)
            {
                _mediaTimeline.Start();
                return;
            }
                RoutedEventArgs e = new RoutedEventArgs();
                object notRandom = new object();
                Next_Click(notRandom, e);
               _mediaTimeline.Start();
        }

        private void RepeatButt_Click(object sender, RoutedEventArgs e)
        {
            if (_isRepeatPlayingMode == false)
            {
                RepeatButt.Style = Application.Current.Resources["RepeatButtStyle2"] as Style;

                _isRepeatPlayingMode = true;
            }

            else
            {
                RepeatButt.Style = Application.Current.Resources["RepeatButtStyle1"] as Style;

                _isRepeatPlayingMode = false;
            }
        }
    }
}

