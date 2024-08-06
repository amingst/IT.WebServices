﻿using IT.WebServices.Fragments.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services.Data
{
    public interface IProgressDataProvider
    {
        Task LogProgress(Guid userId, Guid contentId, float progress);
    }
}