﻿namespace Learn.Bit.Server.Api.Models.Emailing;

public partial class ElevatedAccessTokenTemplateModel
{
    public required string DisplayName { get; set; }

    public required string Token { get; set; }
}