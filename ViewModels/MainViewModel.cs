using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VkLikerMVVM.Commands;
using VkNet;
using VkNet.AudioBypassService.Exceptions;
using VkNet.Exception;
using VkNet.Model;

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
        private ulong _totalPosts;
        private long _postsLiked;
        private ulong _totalPhotos;
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

        #endregion

        //Конструктор
        public MainViewModel()
        {
            #region Commands

            AuthorizeCommand = new RelayCommand(obj => Authorize(obj as PasswordBox));
            GetInfoCommand = new RelayCommand(async obj => await GetInfo());
            LikePostsCommand = new RelayCommand(async obj => await LikePosts());
            LikePhotosCommand = new RelayCommand(obj => LikePhotos());

            #endregion

            Settings.Instance.Load();

            if (Settings.Instance.Token != null)
            {
                Authorize(null);
            }
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

        public ulong TotalPosts
        {
            get => _totalPosts;
            set => SetProperty(ref _totalPosts, value);
        }

        public long PostsLiked
        {
            get => _postsLiked;
            set => SetProperty(ref _postsLiked, value);
        }

        public ulong TotalPhotos
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
                SetProperty(ref _delayMax, value);
                _likesFunctions.DelayMax = value;
            }
        }

        public int LikesCount
        {
            get => _likesCount;
            set => _likesFunctions.Amount = _likesCount = value;
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

        #region Commands

        public RelayCommand AuthorizeCommand { get; }
        public RelayCommand GetInfoCommand { get; }
        public RelayCommand LikePostsCommand { get; }
        public RelayCommand LikePhotosCommand { get; }

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
                throw new NotImplementedException(); // сам уберешь это.
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
                    var progressTotalPosts = new Progress<ulong>(report => TotalPosts = report);
                    var progressTotalPhotos = new Progress<ulong>(report => TotalPhotos = report);
                    await _likesFunctions.GetTargetInfo(progressTotalPosts, progressTotalPhotos);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task LikePosts()
        {
            if (string.IsNullOrEmpty(LikesTarget))
                MessageBox.Show("Введите цель", "Error");

            else
            {
                await GetInfo();
                var itemLiked = new Progress<long>(report => PostsLiked = report);
                IsUiUnLocked = false;
                await _likesFunctions.LikePosts(itemLiked);
                MessageBox.Show("Готово!", "Done");
                IsUiUnLocked = true;
            }
        }

        private async void LikePhotos()
        {
            if (string.IsNullOrEmpty(LikesTarget))
                MessageBox.Show("Введите цель", "Error");

            else
            {
                await GetInfo();
                var itemLiked = new Progress<long>(report => PhotosLiked = report);
                IsUiUnLocked = false;
                await _likesFunctions.LikePhotos(itemLiked);
                MessageBox.Show("Готово!", "Done");
                IsUiUnLocked = true;
            }
        }

        #endregion
    }
}