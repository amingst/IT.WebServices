﻿using Google.Protobuf;
using Microsoft.Extensions.Options;
using IT.WebServices.Fragments.Content;
using IT.WebServices.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IT.WebServices.Helpers;

namespace IT.WebServices.Content.CMS.Services.Data
{
    public class FileSystemAssetDataProvider : IAssetDataProvider
    {
        private readonly DirectoryInfo assetDir;

        public FileSystemAssetDataProvider(IOptions<AppSettings> settings)
        {
            var root = new DirectoryInfo(settings.Value.DataStore);
            root.Create();
            assetDir = root.CreateSubdirectory("cms").CreateSubdirectory("asset");
        }

        public Task<bool> Delete(Guid assetId)
        {
            var fd = GetContentFilePath(assetId);
            var res = fd.Exists;
            fd.Delete();
            return Task.FromResult(res);
        }

        public Task<bool> Exists(Guid assetId)
        {
            var fd = GetContentFilePath(assetId);
            return Task.FromResult(fd.Exists);
        }

        public async IAsyncEnumerable<AssetRecord> GetAll()
        {
            var files = assetDir.GetFiles();
            foreach (var f in files)
            {
                AssetRecord? record = null;
                var filename = f.FullName;
                try
                {
                    var resFile = await File.ReadAllBytesAsync(filename);
                    record = AssetRecord.Parser.ParseFrom(resFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file '{filename}': {ex.Message}");
                    // Optionally create an error record
                    // record = new AssetRecord { Data = $"Error: {ex.Message}" };
                }

                if (record != null)
                {
                    yield return record;
                }
                // If record is null due to an error, we simply don't yield it, and continue.
            }
        }
            //foreach (var file in assetDir.GetFiles())
            //{
            //    var filename = file.FullName;
            //    var resFile = await File.ReadAllBytesAsync(filename); 
            //    yield return AssetRecord.Parser.ParseFrom(resFile);
            //}
        

        public async IAsyncEnumerable<AssetListRecord> GetAllShort()
        {
            foreach (var file in assetDir.GetFiles())
            {
                yield return AssetRecord.Parser.ParseFrom(await File.ReadAllBytesAsync(file.FullName)).ToAssetListRecord();
            }
        }

        public async Task<AssetRecord> GetById(Guid assetId)
        {
            var fd = GetContentFilePath(assetId);
            if (!fd.Exists)
                return null;

            return AssetRecord.Parser.ParseFrom(await File.ReadAllBytesAsync(fd.FullName));
        }

        public async Task<AssetRecord> GetByOldAssetId(string oldAssetId)
        {
            await foreach (var rec in GetAll())
            {
                if (rec.OldAssetId == oldAssetId)
                    return rec;
            }

            return null;
        }

        public async Task<List<AssetListRecord>> GetByAssetTypeAsync(AssetType assetType)
        {
            IAsyncEnumerable<AssetRecord> found = GetAll();
            List<AssetListRecord> records = new ();
            List<AssetListRecord> res = new ();

            await foreach (var rec in found)
            {
                AssetListRecord listRec = null;
                switch (assetType)
                {
                    case AssetType.Audio:
                        if (assetType == AssetType.Image)
                            continue;

                        listRec = rec.ToAssetListRecord();
                        break;
                    case AssetType.Image:
                        if (assetType == AssetType.Audio)
                            continue;

                        listRec = rec.ToAssetListRecord();
                        break;
                }

                if (listRec is not null)
                    res.Add(listRec);

                continue;

            }

            return res;
        }

        public async Task Save(AssetRecord asset)
        {
            var id = asset.AssetIDGuid;
            var fd = GetContentFilePath(id);
            await File.WriteAllBytesAsync(fd.FullName, asset.ToByteArray());
        }

        private FileInfo GetContentFilePath(Guid assetId)
        {
            return assetDir.CreateGuidFileInfo(assetId);
        }
    }
}
