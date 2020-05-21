using System;
using System.Threading.Tasks;
using System.Windows;
using VkNet;
using VkNet.Model;

namespace VkLikerMVVM
{
    class MainVm : BaseVm
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
        private string _totalPosts = "0";
        private string _postLiked = "0";
        private string _totalPhotos = "0";
        private string _photoLiked = "0";
        private string _likesTarget;
        private string _delayMin = "10";
        private string _delayMax = "15";
        private string _likesAmount = "0";
        private string _likesOffset = "0";
        //Команды
        private RelayCommand _loginCommand;
        private RelayCommand _getInfo;
        private RelayCommand _likePosts;
        private RelayCommand _likePhotos;
        //Инстансы
        private LikesFunctions _likesFunctions;

        //isHitTestVisible
        private bool _isUiUnLocked = true;
        #endregion

        //Конструктор
        public MainVm()
        {
            Settings.Instance.Load();

            if (Settings.Instance.Token != null)
            {
                LoginFunc();
            }
        }

        #region VkNet
        public VkApi Api
        {
            get => _api;
            set
            {
                _api = value;
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
                _currentUser = value;
                ScreenName = CurrentUser.ScreenName;
            }
        }
        #endregion

        #region Visibility
        public bool IsMainInterfaceVisible
        {
            get => _isMainInterfaceVisible;
            set
            {
                _isMainInterfaceVisible = value;
                RaisePropertyChanged();
            }
        }

        public bool IsLoginPanelVisible
        {
            get => _isLoginPanelVisible;
            set
            {
                _isLoginPanelVisible = value;
                RaisePropertyChanged();
            }
        }

        public bool IsLogPassVisible
        {
            get => _isLogPassVisible;
            set
            {
                _isLogPassVisible = value;
                RaisePropertyChanged();
            }
        }

        public bool IsTwoAuthVisible
        {
            get => _isTwoAuthVisible;
            set
            {
                _isTwoAuthVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TextBox
        public string TotalPosts
        {
            get => _totalPosts;
            set
            {
                _totalPosts = value;
                RaisePropertyChanged();
            }
        }

        public string PostLiked
        {
            get => _postLiked;
            set
            {
                _postLiked = value;
                RaisePropertyChanged();
            }
        }

        public string TotalPhotos
        {
            get => _totalPhotos;
            set
            {
                _totalPhotos = value;
                RaisePropertyChanged();
            }
        }

        public string PhotoLiked
        {
            get => _photoLiked;
            set
            {
                _photoLiked = value;
                RaisePropertyChanged();
            }
        }

        public string DelayMin
        {
            get => _delayMin;
            set
            {
                _delayMin = value;
                _likesFunctions.DelayMin = Convert.ToInt32(value);
            }
        }

        public string DelayMax
        {
            get => _delayMax;
            set
            {
                _delayMax = value;
                _likesFunctions.DelayMax = Convert.ToInt32(value);
            }
        }

        public string LikesAmount
        {
            get => _likesAmount;
            set
            {
                if (int.TryParse(value, out int intAmount))
                {
                    _likesAmount = value;
                    _likesFunctions.Amount = intAmount;
                }
            }
        }

        public string LikesOffset
        {
            get => _likesOffset;
            set
            {
                if (uint.TryParse(value, out uint ulongOffset))
                {
                    _likesOffset = value;
                    _likesFunctions.Offset = ulongOffset;
                }
            }
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
            set
            {
                _screenName = value;
                RaisePropertyChanged();
            }
        }

        public string LoginTextBox { get; set; }
        public string PasswordBox { private get; set; }
        public string TwoAuthTextBox { get; set; }
        #endregion

        #region IsHitTestVisible
        public bool IsUiUnLocked
        {
            get => _isUiUnLocked;
            set
            {
                _isUiUnLocked = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Commands
        public RelayCommand LoginCommand
        {
            get
            {
                return _loginCommand ??
                  (_loginCommand = new RelayCommand(obj => LoginFunc()));
            }
        }

        public RelayCommand GetInfo
        {
            get
            {
                return _getInfo ??
                  (_getInfo = new RelayCommand(obj => GetInfoFunc()));
            }
        }

        public RelayCommand LikePosts
        {
            get
            {
                return _likePosts ??
                  (_likePosts = new RelayCommand(async(obj) => await LikePostsFunc()));
            }
        }

        public RelayCommand LikePhotos
        {
            get
            {
                return _likePhotos ??
                  (_likePhotos = new RelayCommand(obj => LikePhotosFunc()));
            }
        }
        #endregion

        #region CommandFunc
        private async void LoginFunc()
        {
            IsLoginPanelVisible = false;
            try
            {
                //логин по токену, если есть
                if (Settings.Instance.Token != null)
                {
                    Api = await LoginFunctions.Login(Settings.Instance.Token);
                    IsMainInterfaceVisible = true;
                    return;
                }
                //логин с двухфакторной авторизацей
                else if (IsTwoAuthVisible == true)
                {
                    if (!string.IsNullOrWhiteSpace(TwoAuthTextBox) && TwoAuthTextBox.Length > 3)
                    {
                        Api = await LoginFunctions.Login(LoginTextBox, PasswordBox, TwoAuthTextBox);
                    }
                    else MessageBox.Show("Введите код Двойной Аутентификации");
                }
                //логин по логину и паролю
                else
                {
                    if (!string.IsNullOrWhiteSpace(LoginTextBox) && !string.IsNullOrWhiteSpace(PasswordBox) && LoginTextBox.Length > 4 && PasswordBox.Length > 5)
                    {
                        Api = await LoginFunctions.Login(LoginTextBox, PasswordBox);
                    }
                    else MessageBox.Show("Введите Логин и Пароль");
                }

                if (Api.IsAuthorized)
                {
                    Settings.Instance.Token = Api.Token;
                    IsMainInterfaceVisible = true;
                    Settings.Instance.Save();
                }
                else IsLoginPanelVisible = true;
            }
            catch (VkNet.AudioBypassService.Exceptions.VkAuthException)
            {
                if (IsTwoAuthVisible) MessageBox.Show("Неправильно введен код Двойной Аутентификации", "Error");
                else MessageBox.Show("Неправильно введен Логин или Пароль", "Error");
                IsLoginPanelVisible = true;
                return;
            }
            catch (InvalidOperationException)
            {
                IsLogPassVisible = false;
                IsLoginPanelVisible = true;
                IsTwoAuthVisible = true;
                return;
            }
        }

        private async Task GetInfoFunc()
        {
            try
            {
                if (string.IsNullOrEmpty(LikesTarget)) MessageBox.Show("Введите цель", "Error");
                else
                {
                    var progressTotalPosts = new Progress<string>(report => TotalPosts = report);
                    var progressTotalPhotos = new Progress<string>(report => TotalPhotos = report);
                    await _likesFunctions.GetTargetInfo(progressTotalPosts, progressTotalPhotos);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); 
            }
        }

        private async Task LikePostsFunc()
        {
            if (string.IsNullOrEmpty(LikesTarget)) MessageBox.Show("Введите цель", "Error");

            else
            {
                await GetInfoFunc();
                Progress<string> itemLiked = new Progress<string>(report => PostLiked = report);
                IsUiUnLocked = false;
                await _likesFunctions.LikePosts(itemLiked);
                MessageBox.Show("Готово!", "Done");
                IsUiUnLocked = true;
            }
        }

        private async void LikePhotosFunc()
        {
            if (string.IsNullOrEmpty(LikesTarget)) MessageBox.Show("Введите цель", "Error");

            else
            {
                await GetInfoFunc();
                Progress<string> itemLiked = new Progress<string>(report => PhotoLiked = report);
                IsUiUnLocked = false;
                await _likesFunctions.LikePhotos(itemLiked);
                MessageBox.Show("Готово!", "Done");
                IsUiUnLocked = true;
            }
        }
        #endregion
    }
}
