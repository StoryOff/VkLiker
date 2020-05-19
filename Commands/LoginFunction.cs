using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using VkNet;
using VkNet.AudioBypassService.Extensions;
using VkNet.AudioBypassService.Models;
using VkNet.Model;

namespace VkLikerMVVM
{
    class LoginFunction
    {
        //Логин по токену
        public static async Task Login(string token, VkApi api)
        {
            await api.AuthorizeAsync(new ApiAuthParams
            {
                AccessToken = token
            });

            GetCurrentUser(api);
        }

        //Логин по логину/номеру телефона и паролю
        public static async Task Login(string login, string password, VkApi api)
        {
            await api.AuthorizeAsync(new ApiAuthParams
            {
                Login = login,
                Password = password
            });
            GetCurrentUser(api);
        }

        //Логин по логину и паролю с двухфакторной аутентификацией
        public static async Task Login(string login, string password, string twoAuth, VkApi api)
        {
            await api.AuthorizeAsync(new ApiAuthParams
            {
                Login = login,
                Password = password,
                TwoFactorAuthorization = () => { return twoAuth; }
            });
            GetCurrentUser(api);
        }

        private static void GetCurrentUser(VkApi api)
        {
            MainVM.CurrentUser = api.Users.Get(new List<long>(), VkNet.Enums.Filters.ProfileFields.ScreenName).FirstOrDefault();
        }
    }
}
