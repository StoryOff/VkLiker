using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using VkNet;
using VkNet.AudioBypassService.Extensions;
using VkNet.Model;

namespace VkLikerMVVM
{
    class MainVM : BaseVM
    {
        #region Storage
        private bool _isMainInterfaceVisible;
        private bool _isLoginPanelVisible = true;
        private bool _isLogPassVisible = true;
        private bool _isTwoAuthVisible = false;
        private RelayCommand _loginCommand;
        private static User _currentUser = new User();
        public static string _screenName = "Screen Name";
        #endregion

        public VkApi Api = new VkApi(new ServiceCollection().AddAudioBypass());

        public static string ScreenName
        {
            get => _screenName;
            set
            {
                _screenName = value;
            }
        }

        public static User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                ScreenName = CurrentUser.ScreenName;
            }
        }

        public bool IsMainInterfaceVisible
        {
            get => _isMainInterfaceVisible;
            set
            {
                _isMainInterfaceVisible = value;
                RaisePropertyChanged();
            }
        }

        public MainVM()
        {
            if (Settings.Instance.Token != null)
            {
                LoginFunc();
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

        public string LoginTextBox { get; set; } = "";
        public string PasswordBox { private get; set; } = "";
        public string TwoAuthTextBox { get; set; } = "";

        public RelayCommand LoginCommand
        {
            get
            {
                return _loginCommand ??
                  (_loginCommand = new RelayCommand(obj => LoginFunc()));
            }
        }

        private async void LoginFunc()
        {
            IsLoginPanelVisible = false;
            try
            {
                //логин по токену, если есть
                if (Settings.Instance.Token != null)
                {
                    await LoginFunction.Login(Settings.Instance.Token, Api);
                    IsMainInterfaceVisible = true;
                    MessageBox.Show(CurrentUser.ScreenName);
                    return;
                }
                //логин с двухфакторной авторизацей
                else if (IsTwoAuthVisible == true)
                {
                    if (TwoAuthTextBox.Length > 3)
                    {
                        await LoginFunction.Login(LoginTextBox, PasswordBox, TwoAuthTextBox, Api);
                    }
                    else MessageBox.Show("Введите код Двойной Аутентификации");
                }
                //логин по логину и паролю
                else
                {
                    if (LoginTextBox.Length > 4 && PasswordBox.Length > 5)
                    {
                        await LoginFunction.Login(LoginTextBox, PasswordBox, Api);
                    }
                    else MessageBox.Show("Введите Логин и Пароль");
                }

                if (Api.IsAuthorized)
                {
                    Settings.Instance.Token = Api.Token;
                    IsMainInterfaceVisible = true;
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
    }
}
