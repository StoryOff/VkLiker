﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VkNet;
using VkNet.AudioBypassService.Extensions;
using VkNet.Model;

namespace VkLikerMVVM.Commands
{
    class LoginFunctions
    {
         private static readonly VkApi Api = new VkApi(new ServiceCollection().AddAudioBypass());

        //Логин по токену
        public static async Task<VkApi> Login(string token)
        {
            await Api.AuthorizeAsync(new ApiAuthParams
            {
                AccessToken = token
            });

            return Api;
        }

        //Логин по логину/номеру телефона и паролю
        public static async Task<VkApi> Login(string login, string password)
        {
            await Api.AuthorizeAsync(new ApiAuthParams
            {
                Login = login,
                Password = password,
                ForceSms = true
            });

            return Api;
        }

        //Логин по логину и паролю с двухфакторной аутентификацией
        public static async Task<VkApi> Login(string login, string password, string twoAuth)
        {
            await Api.AuthorizeAsync(new ApiAuthParams
            {
                Login = login,
                Password = password,
                TwoFactorAuthorization = () => twoAuth
            });

            return Api;
        }

        public static User GetCurrentUser()
        {
            return Api.Users.Get(new List<long>(), VkNet.Enums.Filters.ProfileFields.ScreenName).FirstOrDefault();
        }
    }
}
