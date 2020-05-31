using System;
using System.Collections.Generic;
using System.IO;
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

        public static readonly Random R = new Random();

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
        public uint Offset { get; set; } = 0;

        public bool IsStop;
        public bool DisLike;

        public delegate void ItemLiked(long currentItemLiked);
        public event ItemLiked ShowTotalPosts;
        public event ItemLiked ShowTotalPhotos;
        public event ItemLiked PostNotify;
        public event ItemLiked PhotoNotify;

        public string TxtPath;

        private ulong TotalPosts { get; set; }
        private ulong TotalPhotos { get; set; }

        private static readonly Random _rnd = new Random();


        //Получить информацию о количестве постов и фото пользователя
        public async Task GetTargetInfo()
        {
            long targetUserId = await GetTargetId();

            if (targetUserId == 0) return;

            TotalPosts = (await GetWallPosts(targetUserId, count: 1)).TotalCount;
            ShowTotalPosts?.Invoke((long)TotalPosts);

            TotalPhotos = (await GetPhotos(targetUserId, count: 1)).TotalCount;
            ShowTotalPhotos?.Invoke((long)TotalPhotos);
        }

        //Лайк постов
        public async Task LikePosts(int amount)
        {
            if (amount == 0) amount = 100000;
            long targetId = await GetTargetId();
            for (uint i = Offset; i < TotalPosts || i < Offset + amount; i += 100)
            {
                if (!IsStop)
                {
                    WallGetObject wallPosts = await GetWallPosts(targetId, offset: i);
                    for (int b = 0; b < (int)wallPosts.TotalCount; b++)
                    {
                        if (!IsStop)
                        {
                            long postId = wallPosts.WallPosts[b].Id.Value;

                            if (DisLike) await _api.Likes.DeleteAsync(LikeObjectType.Post, postId, targetId);
                            else await _api.Likes.AddAsync(new LikesAddParams { ItemId = postId, OwnerId = targetId, Type = LikeObjectType.Post, AccessKey = wallPosts.WallPosts[b].AccessKey });

                            PostNotify?.Invoke(i + b + 1);

                            //оффсет + текущий объект + 2(отсчет от нуля + следующая итерация) > количества требуемых лайков + оффсет
                            if ((i + b + 2) > amount + Offset) return;

                            await Task.Delay(R.Next(DelayMin, DelayMax));
                        }
                        else return;
                    }
                }
                else return;
            }
        }

        //Лайк фото
        public async Task LikePhotos(int amount)
        {
            if (amount == 0) amount = 100000;
            long targetId = await GetTargetId();

            for (uint i = Offset; i < TotalPhotos || i < Offset + amount; i += 200)
            {
                if (!IsStop)
                {
                    VkCollection<Photo> photos = await GetPhotos(targetId, i);

                    for (int b = 0; b < (int)photos.TotalCount; b++)
                    {
                        if (!IsStop)
                        {
                            long photoId = photos.ToList()[b].Id.Value;

                            if (DisLike) await _api.Likes.DeleteAsync(LikeObjectType.Photo, photoId, targetId);
                            else await _api.Likes.AddAsync(new LikesAddParams { ItemId = photoId, OwnerId = targetId, Type = LikeObjectType.Photo, AccessKey = photos[b].AccessKey });

                            PhotoNotify?.Invoke(i + b + 1);

                            //оффсет + текущий объект + 2(отсчет от нуля + следующая итерация) > количества требуемых лайков + оффсет
                            if ((i + b + 2) > amount + Offset) return;

                            await Task.Delay(R.Next(DelayMin, DelayMax));
                        }
                        else return;
                    }
                }
                else return;
            }
        }

        public async Task LikeList(int amount)
        {
            if (amount == 0) amount = 100000;

            List<string> links = File.ReadAllLines(TxtPath).ToList();

            for (int i = 0; i < links.Count && i < amount; i++)
            {
                //рандом для случайного выбора пользователя из списка
                int rndNum = _rnd.Next(links.Count);

                Target = links[rndNum].Split("/").LastOrDefault();
                long targetId = await GetTargetId();
                //TargetId==0 - значит, профиль закрыт и нет доступа.
                if (targetId!= 0)
                {
                    //рандом для случайного выбора поста/фото
                    int randNum;
                    //получить фото и лайкнуть 2 случайных, если они есть
                    var photos = await GetPhotos(targetId);
                    if (photos.Count > 0)
                    {
                        randNum = _rnd.Next(photos.Count-1);
                        await _api.Likes.AddAsync(new LikesAddParams { ItemId = photos[randNum].Id.Value, OwnerId = targetId, Type = LikeObjectType.Photo, AccessKey = photos[randNum].AccessKey });
                        randNum = _rnd.Next(photos.Count-1);
                        await _api.Likes.AddAsync(new LikesAddParams { ItemId = photos[randNum].Id.Value, OwnerId = targetId, Type = LikeObjectType.Photo, AccessKey = photos[randNum].AccessKey });
                    }
                    //получить посты и лайкнуть 2 случайных, если они есть
                    var posts = await GetWallPosts(targetId);
                    if (posts.TotalCount > 0)
                    {
                        randNum = _rnd.Next((int)posts.TotalCount-1);
                        await _api.Likes.AddAsync(new LikesAddParams { ItemId = posts.WallPosts[randNum].Id.Value, OwnerId = targetId, Type = LikeObjectType.Post, AccessKey = posts.WallPosts[randNum].AccessKey });
                        randNum = _rnd.Next((int)posts.TotalCount-1);
                        await _api.Likes.AddAsync(new LikesAddParams { ItemId = posts.WallPosts[randNum].Id.Value, OwnerId = targetId, Type = LikeObjectType.Post, AccessKey = posts.WallPosts[randNum].AccessKey });
                    }
                }

                links.RemoveAt(rndNum);

                await File.WriteAllLinesAsync(TxtPath, links.ToArray());
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
        private async Task<VkCollection<Photo>> GetPhotos(long targetId, uint offset = 0, ulong count = 200)
        {
            return await _api.Photo.GetAllAsync(new PhotoGetAllParams
            {
                OwnerId = targetId,
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
                var user = (await _api.Users.GetAsync(new List<string> { Target })).LastOrDefault();
                if ((bool)!user.IsClosed || (bool)user.CanAccessClosed) return user.Id;
                else return 0;
            }
        }
    }
}