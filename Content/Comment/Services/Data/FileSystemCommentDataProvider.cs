using Google.Protobuf;
using Microsoft.Extensions.Options;
using IT.WebServices.Fragments.Comment;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using IT.WebServices.Helpers;
using System.ComponentModel.Design;

namespace IT.WebServices.Content.Comment.Services.Data
{
    public class FileSystemCommentDataProvider : ICommentDataProvider
    {
        private readonly DirectoryInfo commentDir;
        private readonly DirectoryInfo contentIndexDir;
        private readonly DirectoryInfo parentIndexDir;
        private readonly byte[] touch = new byte[0];

        public FileSystemCommentDataProvider(IOptions<AppSettings> settings)
        {
            var root = new DirectoryInfo(settings.Value.DataStore);
            root.Create();
            var dir = root.CreateSubdirectory("comment");
            commentDir = dir.CreateSubdirectory("comment");
            contentIndexDir = dir.CreateSubdirectory("content");
            parentIndexDir = dir.CreateSubdirectory("parent");
        }


        public async Task CreateIndexes(CommentRecord record)
        {
            var fdContent = GetContentIndexFilePath(record);
            await File.WriteAllBytesAsync(fdContent.FullName, touch);

            if (record.Public.ParentCommentID.ToGuid() != Guid.Empty)
            {
                var fdParent = GetParentIndexFilePath(record);
                await File.WriteAllBytesAsync(fdParent.FullName, touch);
            }
        }

        public async Task<bool> Delete(CommentRecord record)
        {
            var fd = GetCommentFilePath(record.Public.CommentID);
            var res = fd.Exists;
            if (res)
                fd.Delete();

            await DeleteIndexes(record);

            return res;
        }

        public Task<bool> Delete(Guid commentId)
        {
            var fd = GetCommentFilePath(commentId.ToString());
            var res = fd.Exists;
            fd.Delete();
            return Task.FromResult(res);
        }

        public Task DeleteIndexes(CommentRecord record)
        {
            var fdContent = GetContentIndexFilePath(record);
            File.Delete(fdContent.FullName);

            if (record.Public.ParentCommentID.ToGuid() != Guid.Empty)
            {
                var fdParent = GetParentIndexFilePath(record);
                File.Delete(fdParent.FullName);
            }

            return Task.CompletedTask;
        }

        public Task<bool> Exists(Guid commentId)
        {
            var fd = GetCommentFilePath(commentId.ToString());
            return Task.FromResult(fd.Exists);
        }

        public Task<CommentRecord> Get(Guid commentId)
        {
            if (commentId == Guid.Empty)
                return null;

            return Get(commentId.ToString());
        }

        private async Task<CommentRecord> Get(string commentId)
        {
            var fd = GetCommentFilePath(commentId.ToString());
            if (!fd.Exists)
                return null;

            return CommentRecord.Parser.ParseFrom(await File.ReadAllBytesAsync(fd.FullName));
        }

        public async IAsyncEnumerable<CommentRecord> GetAll()
        {
            foreach (var file in commentDir.GetFiles("*", SearchOption.AllDirectories))
            {
                yield return CommentRecord.Parser.ParseFrom(await File.ReadAllBytesAsync(file.FullName));
            }
        }

        public async IAsyncEnumerable<CommentRecord> GetByContentId(Guid contentId)
        {
            foreach (var file in GetContentIndexDirPath(contentId.ToString()).GetFiles())
            {
                yield return await Get(file.Name);
            }
        }

        public async IAsyncEnumerable<CommentRecord> GetByParentId(Guid parentId)
        {
            foreach (var file in GetParentIndexDirPath(parentId.ToString()).GetFiles())
            {
                yield return await Get(file.Name);
            }
        }

        public async IAsyncEnumerable<CommentResponseRecord> GetCommentResponseRecordByContentId(Guid contentId, Guid userId)
        {
            await foreach(var c in GetByContentId(contentId))
                yield return c.ToCommentResponseRecord(userId);
        }

        public async IAsyncEnumerable<CommentResponseRecord> GetCommentResponseRecordByParentId(Guid parentId, Guid userId)
        {
            await foreach (var c in GetByParentId(parentId))
                yield return c.ToCommentResponseRecord(userId);
        }

        public async Task Insert(CommentRecord record)
        {
            var fdComment = GetCommentFilePath(record);
            var tComment = File.WriteAllBytesAsync(fdComment.FullName, record.ToByteArray());
            var tIndex = CreateIndexes(record);

            await Task.WhenAll(tComment, tIndex);
        }

        public async Task Update(CommentRecord record)
        {
            var fdComment = GetCommentFilePath(record);
            await File.WriteAllBytesAsync(fdComment.FullName, record.ToByteArray());
        }

        private FileInfo GetCommentFilePath(CommentRecord record)
        {
            return GetCommentFilePath(record.Public.CommentID);
        }

        private FileInfo GetCommentFilePath(string commentId)
        {
            return commentDir.CreateGuidFileInfo(commentId.ToGuid(), 3);
        }

        private FileInfo GetContentIndexFilePath(CommentRecord record)
        {
            return GetContentIndexFilePath(record.Public.ContentID, record.Public.CommentID);
        }

        private DirectoryInfo GetContentIndexDirPath(string contentID)
        {
            return contentIndexDir.CreateGuidDirectory(contentID.ToGuid());
        }

        private FileInfo GetContentIndexFilePath(string contentID, string commentId)
        {
            var dir = GetContentIndexDirPath(contentID);
            return new FileInfo(dir.FullName + "/" + commentId);
        }

        private FileInfo GetParentIndexFilePath(CommentRecord record)
        {
            return GetParentIndexFilePath(record.Public.ParentCommentID, record.Public.CommentID);
        }

        private DirectoryInfo GetParentIndexDirPath(string parentID)
        {
            return parentIndexDir.CreateGuidDirectory(parentID.ToGuid());
        }

        private FileInfo GetParentIndexFilePath(string parentID, string commentId)
        {
            var dir = GetParentIndexDirPath(parentID);
            return new FileInfo(dir.FullName + "/" + commentId);
        }
    }
}
