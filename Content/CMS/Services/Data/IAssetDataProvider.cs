using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IT.WebServices.Fragments.Content;

namespace IT.WebServices.Content.CMS.Services.Data
{
    public interface IAssetDataProvider
    {
        IAsyncEnumerable<AssetRecord> GetAll();
        IAsyncEnumerable<AssetListRecord> GetAllShort();
        Task<AssetRecord> GetById(Guid assetId);
        Task<AssetRecord> GetByOldAssetId(string oldAssetId);
        Task<bool> Delete(Guid assetId);
        Task<bool> Exists(Guid assetId);
        Task Save(AssetRecord asset);
        Task<List<AssetListRecord>> GetByAssetTypeAsync(AssetType assetType);
    }
}
