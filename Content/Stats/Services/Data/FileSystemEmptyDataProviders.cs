﻿using Google.Protobuf;
using Google.Protobuf.Reflection;
using Microsoft.Extensions.Options;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IT.WebServices.Fragments.Content.Stats;
using IT.WebServices.Content.Stats.Services.Subscriptions;

namespace IT.WebServices.Content.Stats.Services.Data
{
    public class FileSystemLikeDataProvider : GenericFileSystemEmptyDataProvider, ILikeDataProvider
    {
        public FileSystemLikeDataProvider(IOptions<AppSettings> settings, SubscriptionList subList) : base("likes", settings, subList) { }

        public async Task Like(Guid userId, Guid contentId)
        {
            await Touch(userId, contentId);
            await subList.ContentChanges.Writer.WriteAsync(contentId);
            await subList.UserChanges.Writer.WriteAsync(userId);
        }

        public async Task Unlike(Guid userId, Guid contentId)
        {
            await Delete(userId, contentId);
            await subList.ContentChanges.Writer.WriteAsync(contentId);
            await subList.UserChanges.Writer.WriteAsync(userId);
        }
    }

    public class FileSystemSaveDataProvider : GenericFileSystemEmptyDataProvider, ISaveDataProvider
    {
        public FileSystemSaveDataProvider(IOptions<AppSettings> settings, SubscriptionList subList) : base("saves", settings, subList) { }

        public async Task Save(Guid userId, Guid contentId)
        {
            await Touch(userId, contentId);
            await subList.ContentChanges.Writer.WriteAsync(contentId);
            await subList.UserChanges.Writer.WriteAsync(userId);
        }

        public async Task Unsave(Guid userId, Guid contentId)
        {
            await Delete(userId, contentId);
            await subList.ContentChanges.Writer.WriteAsync(contentId);
            await subList.UserChanges.Writer.WriteAsync(userId);
        }
    }

    public class GenericFileSystemEmptyDataProvider
    {
        protected readonly SubscriptionList subList;

        private readonly DirectoryInfo contentDataDir;
        private readonly DirectoryInfo userDataDir;
        private readonly string subDirName;

        public GenericFileSystemEmptyDataProvider(string subDirName, IOptions<AppSettings> settings, SubscriptionList subList)
        {
            this.subList = subList;

            this.subDirName = subDirName;

            var root = new DirectoryInfo(settings.Value.DataStore);
            root.Create();
            contentDataDir = root.CreateSubdirectory("stats").CreateSubdirectory("content");
            userDataDir = root.CreateSubdirectory("stats").CreateSubdirectory("users");
        }

        public Task Delete(Guid userId, Guid contentId)
        {
            var fContent = GetContentFilePath(contentId, userId);
            var fUser = GetUserFilePath(userId, contentId);

            fContent.Delete();
            fUser.Delete();

            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<Guid> GetAllForContent(Guid contentId)
        {
            foreach (var file in GetContentDir(contentId).GetFiles())
                yield return await Task.FromResult(file.Name.ToGuid());
        }

        public async IAsyncEnumerable<Guid> GetAllForUser(Guid userId)
        {
            foreach (var file in GetUserDir(userId).GetFiles())
                yield return await Task.FromResult(file.Name.ToGuid());
        }

        public Task<DateTime> GetLastChangeForContent(Guid contentId)
        {
            return Task.FromResult(GetContentDir(contentId).LastWriteTimeUtc);
        }

        public Task<DateTime> GetLastChangeForUser(Guid userId)
        {
            return Task.FromResult(GetUserDir(userId).LastWriteTimeUtc);
        }

        public async Task Touch(Guid userId, Guid contentId)
        {
            var fContent = GetContentFilePath(contentId, userId);
            var fUser = GetUserFilePath(userId, contentId);

            await Touch(fContent);
            await Touch(fUser);
        }

        private ValueTask Touch(FileInfo fi)
        {
            fi.Delete();
            return fi.Create().DisposeAsync();
        }

        private DirectoryInfo GetContentDir(Guid contentId)
        {
            var dir = contentDataDir.CreateSubdirectory(contentId.ToString()).CreateSubdirectory(subDirName);
            return dir;
        }

        private DirectoryInfo GetUserDir(Guid userId)
        {
            var dir = userDataDir.CreateSubdirectory(userId.ToString()).CreateSubdirectory(subDirName);
            return dir;
        }

        private FileInfo GetContentFilePath(Guid contentId, Guid userId)
        {
            var dir = GetContentDir(contentId);
            var name = userId.ToString();
            return new FileInfo(dir.FullName + "/" + name);
        }

        private FileInfo GetUserFilePath(Guid userId, Guid contentId)
        {
            var dir = GetUserDir(userId);
            var name = contentId.ToString();
            return new FileInfo(dir.FullName + "/" + name);
        }
    }
}