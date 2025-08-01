﻿using ServiceHub.Core.Models.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Services.Interfaces
{
    public interface IWordCharacterCounterService
    {
        Task<WordCharacterCountResponseModel> CountTextAsync(WordCharacterCountRequestModel request);
    }
}
