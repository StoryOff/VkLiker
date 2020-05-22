using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace VkLikerMVVM.Commands
{
    class LikesFunctions
    {
        private readonly VkApi _api;
        //конструктор
        public LikesFunctions(VkApi api)
        {
            _api = api;
        }

        public static readonly Random r = new Random();

        private int _delayMin = 10000;
        private int _delayMax = 15000;
        public int DelayMin
        {
            get => _delayMin;
            set => _delayMin = (value * 1000);
        }
        public int DelayMax
        {
            get => _delayMax;
            set => _delayMax = (value * 1000);
        }

        public string Target { get; set; }
        public int Amount { get; set; } = 0;
        public uint Offset { get; set; } = 0;

        private ulong TotalPosts { get; set; }
        private ulong TotalPhotos { get; set; }

        //Получить информацию о количестве постов и фото пользователя
        public async Task GetTargetInfo(IProgress<string> progressPosts = null, IProgress<string> progressPhotos = null)
        {
            long targetUserId = await GetTargetId();

            if (targetUserId == 0) return;

            TotalPosts = (await GetWallPosts(targetUserId, count: 1)).TotalCount;
            progressPosts.Report(TotalPosts.ToString());

            TotalPhotos = (await GetPhotos(targetUserId, count: 1)).TotalCount;
            progressPhotos.Report(TotalPhotos.ToString());
        }

        //Лайк постов
        public async Task LikePosts(IProgress<string> progressCounter = null)
        {
            if (Amount == 0) Amount = 100000;
            long targetUserId = await GetTargetId();
            for (uint i = Offset; i < TotalPosts || i < Offset + Amount; i += 100)
            {
                WallGetObject wallPosts = await GetWallPosts(targetUserId, offset: i);

                for (int b = 0; b < (int)wallPosts.TotalCount; b++)
                {
                    long postId = wallPosts.WallPosts[b].Id.Value;

                    await _api.Likes.AddAsync(new LikesAddParams { ItemId = postId, OwnerId = targetUserId, Type = LikeObjectType.Post, AccessKey = wallPosts.WallPosts[b].AccessKey });

                    progressCounter.Report((i + b + 1).ToString());
                    
                    //оффсет + текущий объект + 2(отсчет от нуля + следующая итерация) > количества требуемых лайков + оффсет
                    if ((i + b + 2) > Amount + Offset) return;

                    await Task.Delay(r.Next(DelayMin, DelayMax));
                }
            }
        }

        //Лайк фото
        public async Task LikePhotos(IProgress<string> progressCounter = null)
        {
                if (Amount == 0) Amount = 100000;
                long targetUserId = await GetTargetId();

            for (uint i = Offset; i < TotalPhotos || i < Offset + Amount; i += 200)
            {
                VkCollection<Photo> photos = await GetPhotos(targetUserId, offset: i);
                
                for (int b = 0; b < (int)photos.TotalCount; b++)
                {
                    long photoId = photos.ToList()[b].Id.Value;

                    await _api.Likes.AddAsync(new LikesAddParams { ItemId = photoId, OwnerId = targetUserId, Type = LikeObjectType.Photo, AccessKey = photos[b].AccessKey });

                    progressCounter.Report((i + b + 1).ToString());

                    //оффсет + текущий объект + 2(отсчет от нуля + следующая итерация) > количества требуемых лайков + оффсет
                    if ((i + b + 2) > Amount + Offset) return;
                    
                    await Task.Delay(r.Next(DelayMin, DelayMax));
                }
            }
        }

        //Получить посты пользователя
        private async Task<WallGetObject> GetWallPosts(long targetid, uint offset = 0, ulong count = 100)
        {
            return await _api.Wall.GetAsync(new WallGetParams
            {
                OwnerId = targetid,
                Offset = offset,
                Count = count
            });
        }

        //Получить фото пользователя
        private async Task<VkCollection<Photo>> GetPhotos(long targetid, uint offset = 0, ulong count = 200)
        {
            return await _api.Photo.GetAllAsync(new PhotoGetAllParams
            {
                OwnerId = targetid,
                Offset = offset,
                Count = count
            });
        }

        //Получить id пользователя по скрин нейму
        private async Task<long> GetTargetId()
        {
            if (long.TryParse(Target, out long targetId)) return targetId;

            else
            {
                return (await _api.Users.GetAsync(new List<string> { Target })).LastOrDefault().Id;
            }
        }
    }
}