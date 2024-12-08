﻿using Learn.Bit.Client.Core.Styles;

namespace Learn.Bit.Client.Windows.Services;
public partial class WindowsDeviceCoordinator : IBitDeviceCoordinator
{
    public async Task ApplyTheme(bool isDark)
    {
        Application.SetColorMode(isDark ? SystemColorMode.Dark : SystemColorMode.Classic);
        Application.OpenForms[0]!.FormCaptionBackColor = ColorTranslator.FromHtml(isDark ? ThemeColors.PrimaryDarkBgColor : ThemeColors.PrimaryLightBgColor);
    }
}
