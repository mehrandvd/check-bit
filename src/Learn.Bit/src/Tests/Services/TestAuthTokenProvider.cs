﻿using Learn.Bit.Client.Core.Services.Contracts;

namespace Learn.Bit.Tests.Services;
public partial class TestAuthTokenProvider : IAuthTokenProvider
{
    [AutoInject] private IStorageService storageService = default!;

    public async Task<string?> GetAccessToken()
    {
        return await storageService.GetItem("access_token");
    }
}
