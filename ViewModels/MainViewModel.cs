using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VkLikerMVVM.Commands;
using VkNet;
using VkNet.AudioBypassService.Exceptions;
using VkNet.Exception;
using VkNet.Model;
using MahApps.Metro.Controls.Dialogs;

namespace VkLikerMVVM.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        #region Storage

        //VkNet
        private User _currentUser = new User();
        private VkApi _api;

        //Визибилити
        private bool _isMainInterfaceVisible;
        private bool _isLoginPanelVisible = true;
        private bool _isLogPassVisible = true;
        private bool _isTwoAuthVisible;

        //Текст боксы
        private string _screenName = "Screen Name";
        private long _totalPosts;
        private long _postsLiked;
        private long _totalPhotos;
        private long _photosLiked;
        private string _likesTarget;
        private int _delayMin = 10;
        private int _delayMax = 15;
        private int _likesCount;

        private uint _likesOffset;

        //Инстансы
        private LikesFunctions _likesFunctions;

        //isHitTestVisible
        private bool _isUiUnLocked = true;

        //MahApps.Metro
        private readonly IDialogCoordinator dialogCoordinator;

        //Булы(баттоны/тоглбаттны)
        private bool _isStop = true;
        private bool _disLike;

        //Путь
        private string _txtPath;

        #endregion

        //Конструктор
        public MainViewModel(IDialogCoordinator instance)
        {
            #region Commands

            AuthorizeCommand = new RelayCommand(obj => Authorize(obj as PasswordBox));
            GetInfoCommand = new RelayCommand(async obj => await GetInfo());
            LikePostsCommand = new RelayCommand(obj => LikePosts());
            LikePhotosCommand = new RelayCommand(obj => LikePhotos());
            StopCommand = new RelayCommand(obj => { isStop = true; MessageBox.Show("Прогресс остановлен"); });
            ChooseTxtPathCommand = new RelayCommand(obj => ChooseTxtPath());
            LikeListCommand = new RelayCommand(obj => LikeList());

            #endregion

            #region Events

            #endregion

            Settings.Instance.Load();

            if (Settings.Instance.Token != null)
            {
                Authorize(null);
            }

            dialogCoordinator = instance;
        }

        #region VkNet

        public VkApi Api
        {
            get => _api;
            set
            {
                SetProperty(ref _api, value);

                if (Api.IsAuthorized)
                {
                    CurrentUser = LoginFunctions.GetCurrentUser();
                    _likesFunctions = new LikesFunctions(value);

                    //подписка на эвенты
                    _likesFunctions.ShowTotalPosts += (ShowTotalPosts) => TotalPosts = ShowTotalPosts;
                    _likesFunctions.ShowTotalPhotos += ShowTotalPhotos;
                    _likesFunctions.PostNotify += PostProgress;
                    _likesFunctions.PhotoNotify += PhotoProgress;
                }
            }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                SetProperty(ref _currentUser, value);
                ScreenName = CurrentUser.ScreenName;
            }
        }

        #endregion

        #region Visibility

        public bool IsMainInterfaceVisible
        {
            get => _isMainInterfaceVisible;
            set => SetProperty(ref _isMainInterfaceVisible, value);
        }

        public bool IsLoginPanelVisible
        {
            get => _isLoginPanelVisible;
            set => SetProperty(ref _isLoginPanelVisible, value);
        }

        public bool IsLogPassVisible
        {
            get => _isLogPassVisible;
            set => SetProperty(ref _isLogPassVisible, value);
        }

        public bool IsTwoAuthVisible
        {
            get => _isTwoAuthVisible;
            set => SetProperty(ref _isTwoAuthVisible, value);
        }

        #endregion

        #region TextBox

        public long TotalPosts
        {
            get => _totalPosts;
            set => SetProperty(ref _totalPosts, value);
        }

        public long PostsLiked
        {
            get => _postsLiked;
            set => SetProperty(ref _postsLiked, value);
        }

        public long TotalPhotos
        {
            get => _totalPhotos;
            set => SetProperty(ref _totalPhotos, value);
        }

        public long PhotosLiked
        {
            get => _photosLiked;
            set => SetProperty(ref _photosLiked, value);
        }

        public int DelayMin
        {
            get => _delayMin;
            set
            {
                SetProperty(ref _delayMin, value);
                _likesFunctions.DelayMin = value;
            }
        }

        public int DelayMax
        {
            get => _delayMax;
            set
            {
                if (value > DelayMin)
                {
                    SetProperty(ref _delayMax, value);
                    _likesFunctions.DelayMax = value;
                }
                else dialogCoordinator.ShowModalMessageExternal(this, "Error", "Значение \"До\" должно быть больше, чем значение \"От\"");
            }
        }

        public int LikesCount
        {
            get => _likesCount;
            set => _likesCount = value;
        }

        public uint LikesOffset
        {
            get => _likesOffset;
            set => _likesFunctions.Offset = _likesOffset = value;
        }

        public string LikesTarget
        {
            get => _likesTarget;
            set
            {
                _likesTarget = value;
                _likesFunctions.Target = _likesTarget;
            }
        }

        public string ScreenName
        {
            get => _screenName;
            set => SetProperty(ref _screenName, value);
        }

        public string Login { get; set; }
        public string TwoAuthCode { get; set; }

        #endregion

        #region IsHitTestVisible

        public bool IsUiUnLocked
        {
            get => _isUiUnLocked;
            set => SetProperty(ref _isUiUnLocked, value);
        }

        #endregion

        #region bools
        public bool isStop
        {
            get => _isStop;
            set
            {
                SetProperty(ref _isStop, value);
                _likesFunctions.IsStop = value;
            }
        }

        public bool DisLike
        {
            get => _disLike;
            set
            {
                SetProperty(ref _disLike, value);
                _likesFunctions.DisLike = value;
            }
        }
        #endregion

        #region Путь
        public string TxtPath
        {
            get => _txtPath;
            set
            {
                SetProperty(ref _txtPath, value);
                _likesFunctions.TxtPath = value;
            }
        }
        #endregion

        #region Commands

        public RelayCommand AuthorizeCommand { get; }
        public RelayCommand GetInfoCommand { get; }
        public RelayCommand LikePostsCommand { get; }
        public RelayCommand LikePhotosCommand { get; }
        public RelayCommand StopCommand { get; }
        public RelayCommand ChooseTxtPathCommand { get; }
        public RelayCommand LikeListCommand { get; }

        #endregion

        #region CommandFunc

        private async void Authorize(PasswordBox passwordBox)
        {
            string password = passwordBox?.Password;

            IsLoginPanelVisible = false;

            try
            {
                // логин по токену, если есть
                if (Settings.Instance.Token != null)
                {
                    Api = await LoginFunctions.Login(Settings.Instance.Token);
                    IsMainInterfaceVisible = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(password) || Login.Length <= 4 || password.Length <= 5)
                {
                    MessageBox.Show("Введите Логин и Пароль");
                    return;
                }

                // логин с двухфакторной авторизацей
                if (IsTwoAuthVisible)
                {
                    if (string.IsNullOrEmpty(TwoAuthCode) || TwoAuthCode.Length <= 3)
                    {
                        MessageBox.Show("Введите код Двойной Аутентификации");
                        return;
                    }

                    Api = await LoginFunctions.Login(Login, password, TwoAuthCode);
                }
                else
                {
                    Api = await LoginFunctions.Login(Login, password);
                }
            }
            catch (VkAuthException)
            {
                MessageBox.Show(IsTwoAuthVisible ? "Неправильно введен код Двойной Аутентификации" : "Неправильно введен Логин или Пароль",
                    "Error");

                IsLoginPanelVisible = true;
            }
            catch (CaptchaNeededException)
            {
                MessageBox.Show("Требуется ввести капчу");
            }
            catch (InvalidOperationException)
            {
                IsLogPassVisible = false;
                IsLoginPanelVisible = true;
                IsTwoAuthVisible = true;
            }
            finally
            {
                if (Api != null && Api.IsAuthorized)
                {
                    Settings.Instance.Token = Api.Token;
                    IsMainInterfaceVisible = true;

                    Settings.Instance.Save();
                }
                else
                {
                    IsLoginPanelVisible = true;
                }
            }
        }

        private async Task GetInfo()
        {
            try
            {
                if (string.IsNullOrEmpty(LikesTarget))
                    MessageBox.Show("Введите цель", "Error");
                else
                {
                    await _likesFunctions.GetTargetInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void LikePosts()
        {
            if (string.IsNullOrEmpty(LikesTarget))
                MessageBox.Show("Введите цель", "Error");

            else
            {
                try
                {
                    isStop = false;
                    IsUiUnLocked = false;
                    await GetInfo();
                    await _likesFunctions.LikePosts(LikesCount);
                    if (!isStop) MessageBox.Show("Готово!", "Done");
                    IsUiUnLocked = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                    IsUiUnLocked = true;
                }
            }
        }

        private async void LikePhotos()
        {
            if (string.IsNullOrEmpty(LikesTarget))
                MessageBox.Show("Введите цель", "Error");

            else
            {
                try
                {
                    isStop = false;
                    IsUiUnLocked = false;
                    await GetInfo();
                    await _likesFunctions.LikePhotos(LikesCount);
                    if (!isStop) MessageBox.Show("Готово!", "Done");
                    IsUiUnLocked = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                    IsUiUnLocked = true;
                }
            }
        }

        private void ChooseTxtPath()
        {
            var dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.InitialDirectory = "C:\\";
            dlg.DefaultExt = "txt";
            dlg.Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) TxtPath = dlg.FileName;
        }

        private async void LikeList()
        {
            if (string.IsNullOrEmpty(TxtPath))
                MessageBox.Show("Выберите Txt файл со списком ссылок", "Error");
            else
            {
                try
                {
                    isStop = false;
                    DisLike = false;
                    LikesTarget = string.Empty;
                    IsUiUnLocked = false;

                    await _likesFunctions.LikeList(LikesCount);
                    if (!isStop) MessageBox.Show("Готово!", "Done");
                    IsUiUnLocked = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                    IsUiUnLocked = true;
                }
            }
        }


        private void ShowTotalPhotos(long Total)
        {
            TotalPhotos = Total;
        }

        private void PostProgress(long Progress)
        {
            PostsLiked = Progress;
        }

        private void PhotoProgress(long Progress)
        {
            PhotosLiked = Progress;
        }
        #endregion
    }
}