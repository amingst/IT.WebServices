using IT.WebServices.Fragments.Comment;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Comment.Services.Data
{
    public interface ICommentDataProvider
    {
        Task CreateIndexes(CommentRecord record);
        Task<bool> Delete(CommentRecord record);
        Task<bool> Delete(Guid commentId);
        Task DeleteIndexes(CommentRecord record);
        Task<bool> Exists(Guid commentId);
        Task<CommentRecord> Get(Guid commentId);
        IAsyncEnumerable<CommentRecord> GetAll();
        IAsyncEnumerable<CommentRecord> GetByContentId(Guid contentId);
        IAsyncEnumerable<CommentRecord> GetByParentId(Guid parentId);
        IAsyncEnumerable<CommentResponseRecord> GetCommentResponseRecordByContentId(Guid contentId, Guid userId);
        IAsyncEnumerable<CommentResponseRecord> GetCommentResponseRecordByParentId(Guid parentId, Guid userId);
        Task Insert(CommentRecord record);
        Task Update(CommentRecord record);
    }
}
