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
using Windows.System;



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

        List<List<string>> _playlistsSongNameCollection;

        List<List<MediaSource>> _playlistsStorageFileCollection;

        TimeSpan _currentTrackDuration;

        MediaTimelineController _mediaTimeline;

        bool _isNewPlaylistAdd = false;

        bool _isSongsListRemoved = true;

        bool _isCompactSize = false;

        bool _isRandomPlayingMode = false;

        bool _isRepeatPlayingMode = false;

        Size _sizeBeforeCompactRezime;

        List<int> _randomIndexesList;

        int _currentTrackIndex;

        #endregion

        public MainPage()
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500, 140));
            this.InitializeComponent();

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(UserLayout);

            _mediaPlayer = new MediaPlayer();

            PlaylistsList.SelectionChanged += PlaylistsSelectionChanged;                

            _mediaPlayer.MediaEnded += MediaEnded;

            _playlistsSongNameCollection = new List<List<string>>();

            _playlistsStorageFileCollection = new List<List<MediaSource>>();

            _storage = new List<MediaSource>();

            VolumeSlider.Value = 100;

            _currentTrackDuration = new TimeSpan();

            _mediaPlayer.MediaOpened += Media_Opened;

            _mediaTimeline = new MediaTimelineController();

            _mediaTimeline.PositionChanged += MediaTimelineController_PositionChanged;

            this.SizeChanged += Page_SizeChanged;

            _sizeBeforeCompactRezime = new Size(930, 550);

            ApplicationView.PreferredLaunchViewSize = new Size(930, 550);

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            MySplitView.PaneOpened += PaneOpened;
            MySplitView.PaneClosed += PaneClosed;

            _randomIndexesList = new List<int>();
        }     

        #region WindowSizeChanged

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height < 1038 && e.NewSize.Height > 320)
            {
                PLaylistsScroll.Height = 720 - (1038 - e.NewSize.Height);
            }
            else
            {
                PLaylistsScroll.Height = 720;
            }

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
                CurrentSong.Width = 470;
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

            if (files.Count > 0 && _isNewPlaylistAdd == false)
            {
                CurrentSongsScroll.Visibility = 0;
                SongsListTitle.Visibility = 0;
                PanelButtAddRemove.Visibility = 0;
                _isSongsListRemoved = false;

                foreach (Windows.Storage.StorageFile file in files)
                {
                    CurrentPlaylist.Items.Add(file.Name.ToString());

                    _media = MediaSource.CreateFromStorageFile(file);

                    _storage.Add(_media);
                }

                if (SongsListTitle.Text != "Tracks")
                {
                    PlaylistToUpdate(files);
                }

                BeforeSongInitializeValidation();

                CurrentSongInitialize();
            }
            else
            {
                if (files.Count > 0)
                {
                    PlaylistCollectionToFill(files);
                }
                _isNewPlaylistAdd = false;
            }
        }

        private void PlaylistCollectionToFill(IReadOnlyList<StorageFile> files)
        {
            foreach (Windows.Storage.StorageFile file in files)
            {                 
                _playlistsSongNameCollection[PlaylistsList.SelectedIndex].Add(file.Name.ToString());

                _media = MediaSource.CreateFromStorageFile(file);

                _playlistsStorageFileCollection[PlaylistsList.SelectedIndex].Add(_media);
            }
        }

        private void PlaylistToUpdate(IReadOnlyList<StorageFile> files)
        {
            int indexPlaylist = PlaylistsList.Items.IndexOf(SongsListTitle.Text);

            foreach (Windows.Storage.StorageFile file in files)
            {
                _playlistsSongNameCollection[indexPlaylist].Add(file.Name.ToString());
            }
        }

        private void BeforeSongInitializeValidation()
        {
            if (_isRandomPlayingMode == true && _storage.Count > 2)
            {
                _randomIndexesList.Clear();
                GetRandomIndexes();
                _mediaTimeline.Pause();
            }

            _isSongsListRemoved = false;

            _currentTrackIndex = 0;
            _mediaPlayer.Source = _storage[0];
            _mediaPlayer.TimelineController = _mediaTimeline;
        }

        private void CurrentSongInitialize()
        {
            CurrentSong.Text = CurrentPlaylist.Items[0].ToString();
            if (CurrentPlaylist.SelectedItems.Count <= 1)
            {
                CurrentPlaylist.SelectedItem = CurrentPlaylist.Items[0];
            }
            TrackDuration.Visibility = Visibility.Visible;
        }

        private void SelectedMusicListsItem(object sender, SelectionChangedEventArgs e)
        {
            if (_isSongsListRemoved == false && CurrentPlaylist.SelectedItems.Count == 1)
            {
                _mediaPlayer.Source = _storage[CurrentPlaylist.SelectedIndex];
                _mediaTimeline.Position = TimeSpan.FromSeconds(0);
                CurrentSong.Text = CurrentPlaylist.SelectedItem.ToString();

                if (_isRandomPlayingMode == false)
                {
                    _currentTrackIndex = CurrentPlaylist.SelectedIndex;
                }

                if (_mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
                {
                    _mediaTimeline.Start();
                    _mediaTimeline.Pause();
                }

                if (_mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    _mediaTimeline.Pause();

                    _mediaPlayer.Source = _storage[CurrentPlaylist.SelectedIndex];

                    _mediaTimeline.Start();
                }
            }
        }

        #region MusicPlayManagePanel 

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (_isSongsListRemoved == false)
            {
                if (CurrentPlaylist.SelectedIndex == default)
                {
                    _mediaPlayer.Source = _storage[0];
                    _currentTrackIndex = 0;
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
                    if (_currentTrackIndex == 0)
                    {
                        _currentTrackIndex = 0;

                        if (CurrentPlaylist.SelectedItems.Count <= 1)
                        {
                            CurrentPlaylist.SelectedItem = CurrentPlaylist.Items[_currentTrackIndex];
                        }

                        else
                        {
                            _mediaPlayer.Source = _storage[_currentTrackIndex];
                            CurrentSong.Text = CurrentPlaylist.Items[_currentTrackIndex].ToString();
                        }
                    }

                    else
                    {
                        _currentTrackIndex--;

                        if (CurrentPlaylist.SelectedItems.Count <= 1)
                        {
                            CurrentPlaylist.SelectedItem = CurrentPlaylist.Items[_currentTrackIndex];
                        }
                        else
                        {
                            _mediaPlayer.Source = _storage[_currentTrackIndex];
                            CurrentSong.Text = CurrentPlaylist.Items[_currentTrackIndex].ToString();
                        }
                    }
                }

                else
                {
                    if (_currentTrackIndex - 1 == -1)
                    {
                        _currentTrackIndex = _storage.Count - 1;

                        if (CurrentPlaylist.SelectedItems.Count <= 1)
                        {
                            CurrentPlaylist.SelectedItem = CurrentPlaylist.Items[_randomIndexesList[_currentTrackIndex]];
                        }
                        else
                        {
                            _mediaPlayer.Source = _storage[_randomIndexesList[_currentTrackIndex]];
                            CurrentSong.Text = CurrentPlaylist.Items[_randomIndexesList[_currentTrackIndex]].ToString();
                        }
                    }

                    else
                    {
                        _currentTrackIndex--;

                        if (CurrentPlaylist.SelectedItems.Count <= 1)
                        {
                            CurrentPlaylist.SelectedItem = CurrentPlaylist.Items[_randomIndexesList[_currentTrackIndex]];
                        }
                        else
                        {
                            _mediaPlayer.Source = _storage[_randomIndexesList[_currentTrackIndex]];
                            CurrentSong.Text = CurrentPlaylist.Items[_randomIndexesList[_currentTrackIndex]].ToString();
                        }
                    }
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
                    if (_currentTrackIndex + 1 == _storage.Count)
                    {
                        _currentTrackIndex = 0;

                        if (CurrentPlaylist.SelectedItems.Count <= 1)
                        {
                            CurrentPlaylist.SelectedItem = CurrentPlaylist.Items[0];
                        }
                        else
                        {
                            _mediaPlayer.Source = _storage[0];
                            CurrentSong.Text = CurrentPlaylist.Items[0].ToString();
                        }
                    }
                    else
                    {
                        _currentTrackIndex++;

                        if (CurrentPlaylist.SelectedItems.Count <= 1)
                        {
                            CurrentPlaylist.SelectedIndex = _currentTrackIndex;
                        }
                        else
                        {
                            _mediaPlayer.Source = _storage[_currentTrackIndex];
                            CurrentSong.Text = CurrentPlaylist.Items[_currentTrackIndex].ToString();
                        }
                    }
                }

                else
                {
                    if (_currentTrackIndex + 1 == _storage.Count)
                    {
                        _currentTrackIndex = 0;

                        if (CurrentPlaylist.SelectedItems.Count <= 1)
                        {
                            CurrentPlaylist.SelectedIndex = _randomIndexesList[_currentTrackIndex];
                        }

                        else
                        {
                            _mediaPlayer.Source = _storage[_randomIndexesList[_currentTrackIndex]];
                            CurrentSong.Text = CurrentPlaylist.Items[_randomIndexesList[_currentTrackIndex]].ToString();
                        }
                    }

                    else
                    {
                        _currentTrackIndex++;

                        if (CurrentPlaylist.SelectedItems.Count <= 1)
                        {
                            CurrentPlaylist.SelectedIndex = _randomIndexesList[_currentTrackIndex];
                        }
                        else
                        {
                            _mediaPlayer.Source = _storage[_randomIndexesList[_currentTrackIndex]];
                            CurrentSong.Text = CurrentPlaylist.Items[_randomIndexesList[_currentTrackIndex]].ToString();
                        }
                    }
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
            VolumeSlider.Value = e.NewValue;

            _mediaPlayer.Volume = VolumeSlider.Value / 100;
        }

        private async void Media_Opened(MediaPlayer sender, object e)
        {
            _currentTrackDuration = sender.PlaybackSession.NaturalDuration;

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                SoundProgressSlider.Minimum = 0;
                SoundProgressSlider.Maximum = _currentTrackDuration.TotalSeconds;
                SoundProgressSlider.StepFrequency = 1;
                TrackDuration.Text = _currentTrackDuration.ToString(@"mm\:ss");
            });
        }

        private async void MediaTimelineController_PositionChanged(MediaTimelineController sender, object args)
        {

            if (_currentTrackDuration != TimeSpan.Zero)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    SoundProgressSlider.Value = sender.Position.TotalSeconds;
                    CurrentTrackTimeProgress.Text = sender.Position.ToString(@"mm\:ss");
                });
            }
        }

        private void Track_Rewind(object sender, RangeBaseValueChangedEventArgs e)
        {
            _mediaTimeline.Position = TimeSpan.FromSeconds(e.NewValue);
        }

        #region Remove_Function
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPlaylist.SelectedItems.Count <= 1 && CurrentPlaylist.Items.Count > 1)
            {
                if (SongsListTitle.Text != "Tracks")
                {
                    int indexPlaylist = PlaylistsList.Items.IndexOf(SongsListTitle.Text);
                    _playlistsSongNameCollection[indexPlaylist].RemoveAt(CurrentPlaylist.SelectedIndex);                    
                }

                    if (_currentTrackIndex == 0)
                {
                    CurrentPlaylist.SelectedIndex = _currentTrackIndex + 1;
                    CurrentPlaylist.Items.RemoveAt(0);
                    _storage.RemoveAt(0);
                    _currentTrackIndex--;

                    if (_isRandomPlayingMode == true)
                    {
                        _randomIndexesList.Clear();
                        GetRandomIndexes();
                    }
                }

                else if (_currentTrackIndex != _storage.Count - 1)
                {
                    CurrentPlaylist.SelectedIndex = _currentTrackIndex + 1;
                    CurrentPlaylist.Items.RemoveAt(_currentTrackIndex - 1);
                    _storage.RemoveAt(_currentTrackIndex - 1);
                    _currentTrackIndex--;

                    if (_isRandomPlayingMode == true)
                    {
                        _randomIndexesList.Clear();
                        GetRandomIndexes();
                    }
                }
                else
                {
                    CurrentPlaylist.SelectedIndex = _currentTrackIndex - 1;
                    CurrentPlaylist.Items.RemoveAt(_currentTrackIndex + 1);
                    _storage.RemoveAt(_currentTrackIndex + 1);

                    if (_isRandomPlayingMode == true)
                    {
                        _randomIndexesList.Clear();
                        GetRandomIndexes();
                    }
                }
            }
            else
            {
                if (CurrentPlaylist.SelectedItems.Count > 1 && _storage.Count - CurrentPlaylist.SelectedItems.Count != 0)
                {
                    if (SongsListTitle.Text != "Tracks")
                    {
                        int indexPlaylist = PlaylistsList.Items.IndexOf(SongsListTitle.Text);

                        for (int i = _storage.Count - 1; i > -1; i--)
                        {
                            if (CurrentPlaylist.SelectedItems.Contains(CurrentPlaylist.Items[i]))
                            {
                                _playlistsSongNameCollection[indexPlaylist].RemoveAt(i);
                            }
                        }                       
                    }

                    for (int i = _storage.Count - 1; i > -1; i--)
                    {
                        if (CurrentPlaylist.SelectedItems.Contains(CurrentPlaylist.Items[i]))
                        {
                            _storage.RemoveAt(i);
                            CurrentPlaylist.Items.RemoveAt(i);
                        }
                    }

                    if (_storage.Count > 1)
                    {
                        CurrentPlaylist.SelectedItem = CurrentPlaylist.Items[_currentTrackIndex];
                    }
                    else
                    {
                        CurrentPlaylist.SelectedItem = CurrentPlaylist.Items[0];
                    }

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
                    CurrentSong.Text = " ";
                    TrackDuration.Visibility = Visibility.Collapsed;
                    CurrentTrackTimeProgress.Text = "00:00";
                    SoundProgressSlider.Value = 0;
                    CurrentSongsScroll.Visibility = Visibility.Collapsed;
                    SongsListTitle.Visibility = Visibility.Collapsed;
                    PanelButtAddRemove.Visibility = Visibility.Collapsed;
                    _isSongsListRemoved = true;
                    CurrentPlaylist.Items.Clear();
                    if (_randomIndexesList.Count > 0)
                    {
                        _randomIndexesList.Clear();
                    }
                    _storage.Clear();
                }
            }
        }
        #endregion

        #region SplitViewSection

        private void HamburgerMenu_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void PaneOpened(SplitView sender, object e)
        {

            if (sender.IsPaneOpen == true)
            {
                AddingTracs.Visibility = Visibility.Visible;
                TextBoxCreateNewPlaylist.Visibility = Visibility.Visible;
                ErrorTextNewPlaylist.Visibility = Visibility.Visible;
                if (PlaylistsList.Items.Count > 0)
                {
                    PlaylistsText.Visibility = Visibility.Visible;
                    PLaylistsScroll.Visibility = Visibility.Visible;
                }
                MySplitView.Style = Application.Current.Resources["SplitViewOpenedStyle"] as Style;
                HamburgerMenu.Style = Application.Current.Resources["HamburgerButt2"] as Style;
            }

        }
        private void PaneClosed(SplitView sender, object e)
        {
            if (sender.IsPaneOpen == false)
            {
                TextBoxCreateNewPlaylist.Visibility = Visibility.Collapsed;
                AddingTracs.Visibility = Visibility.Collapsed;
                ErrorTextNewPlaylist.Visibility = Visibility.Collapsed;
                PlaylistsText.Visibility = Visibility.Collapsed;
                PLaylistsScroll.Visibility = Visibility.Collapsed;
                PlaylistsList.SelectedIndex = -1;
                TextBoxCreateNewPlaylist.Password = "";
                ErrorTextNewPlaylist.Text = "";

                MySplitView.Style = Application.Current.Resources["SplitViewClosedStyle"] as Style;
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

        #region RandomPlayingModeSection

        private void RandomTrackButt_Click(object sender, RoutedEventArgs e)
        {
            if (_isRandomPlayingMode == false)
            {
                RandomTrackButt.Style = Application.Current.Resources["RandomTrackButtStyle2"] as Style;

                if (_storage.Count > 2)
                {
                    _randomIndexesList.Clear();
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
        #endregion

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

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            CurrentPlaylist.SelectAll();
        }

        private void CancelSelection_Click(object sender, RoutedEventArgs e)
        {
            CurrentPlaylist.SelectedItems.Clear();
            TimeSpan returnTrackPosition = _mediaTimeline.Position;
            CurrentPlaylist.SelectedItem = CurrentPlaylist.Items[_currentTrackIndex];
            _mediaTimeline.Position = returnTrackPosition;
        }

        private void PasswordBoxCreateNewPlaylist_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                bool isNameBlankOrEmpty = IsEmptyOrWhitespace(TextBoxCreateNewPlaylist.Password);

                if (isNameBlankOrEmpty == true)
                {
                    ErrorTextNewPlaylist.Text = "The field cannot be empty";
                    return;
                }

                if (PlaylistsList.Items.Contains(TextBoxCreateNewPlaylist.Password.ToString()) == true)
                {
                    ErrorTextNewPlaylist.Text = "Playlist already exists";
                    return;
                }

                if (TextBoxCreateNewPlaylist.Password.ToString() == "Tracks")
                {
                    ErrorTextNewPlaylist.Text = "Use another name";
                    return;
                }

                ErrorTextNewPlaylist.Text = "";
                PlaylistsText.Visibility = Visibility.Visible;
                PLaylistsScroll.Visibility = Visibility.Visible;           
                PlaylistsList.Items.Add(TextBoxCreateNewPlaylist.Password.ToString());
                TextBoxCreateNewPlaylist.Password = string.Empty;

                List<string> listStringNew = new List<string>();
                List<MediaSource> listMediaNew = new List<MediaSource>();
                _playlistsSongNameCollection.Add(listStringNew);
                _playlistsStorageFileCollection.Add(listMediaNew);
            }
        }

        private void PlaylistsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (PlaylistsList.SelectedIndex != -1)
            {
                OpenPlaylist.Visibility = 0;
                AddFiles.Visibility = 0;
                RemoveList.Visibility = 0;
            }
            else
            {
                OpenPlaylist.Visibility = Visibility.Collapsed;
                AddFiles.Visibility = Visibility.Collapsed;
                RemoveList.Visibility = Visibility.Collapsed;
            }
        }

        private bool IsEmptyOrWhitespace(string s)
        {
            if (s == null || s.Length == 0) return true;
            for (int i = 0; i < s.Length; i++)
            {
                if (!char.IsWhiteSpace(s[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private void OpenPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (_playlistsSongNameCollection[PlaylistsList.SelectedIndex].Count > 0)
            {
                CurrentPlaylist.Items.Clear();

                foreach (string songName in _playlistsSongNameCollection[PlaylistsList.SelectedIndex])
                {
                    CurrentPlaylist.Items.Add(songName);
                }

                _storage = _playlistsStorageFileCollection[PlaylistsList.SelectedIndex];

                SongsListTitle.Text = PlaylistsList.Items[PlaylistsList.SelectedIndex].ToString();

                CurrentSongsScroll.Visibility = 0;
                SongsListTitle.Visibility = 0;
                PanelButtAddRemove.Visibility = 0;

                BeforeSongInitializeValidation();

                CurrentSongInitialize();                
            }
            else
            {
                ErrorTextNewPlaylist.Text = "Playlist is empty!";
            }
        }

        private void AddFilesToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            _isNewPlaylistAdd = true;
            AddTracs_Click(sender, e);            
        }

        private void RemoveList_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistsList.SelectedItems.Count == 1)
            {
                _playlistsSongNameCollection.RemoveAt(PlaylistsList.SelectedIndex);
                _playlistsStorageFileCollection.RemoveAt(PlaylistsList.SelectedIndex);
                PlaylistsList.Items.RemoveAt(PlaylistsList.SelectedIndex);
            }
            else 
            {
                for (int i = PlaylistsList.Items.Count - 1; i > -1; i--)
                {
                    if (PlaylistsList.SelectedItems.Contains(PlaylistsList.Items[i]))
                    {
                        _playlistsSongNameCollection.RemoveAt(i);
                        _playlistsStorageFileCollection.RemoveAt(i);
                        PlaylistsList.Items.RemoveAt(PlaylistsList.SelectedIndex);
                    }
                }
            }
        }
    }
}




            



