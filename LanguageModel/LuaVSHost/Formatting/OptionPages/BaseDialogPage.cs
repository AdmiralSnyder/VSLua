﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.LuaLanguageService.Formatting.OptionPages
{
    internal class BaseDialogPage : UIElementDialogPage
    {
        protected override UIElement Child { get; }
    }
}