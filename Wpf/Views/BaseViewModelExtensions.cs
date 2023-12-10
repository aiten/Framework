/*
  This file is part of  https://github.com/aiten/Framework.

  Copyright (c) Herbert Aitenbichler

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace Framework.Wpf.Views;

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using ViewModels;

public static class BaseViewModelExtensions
{
    public static void DefaultInitForBaseViewModel(this BaseViewModel vm)
    {
        vm.MessageBox ??= MessageBox.Show;

        vm.BrowseFileNameFunc ??= (filename, saveFile) =>
        {
            Microsoft.Win32.FileDialog dlg;
            if (saveFile)
            {
                dlg = new Microsoft.Win32.SaveFileDialog();
            }
            else
            {
                dlg = new Microsoft.Win32.OpenFileDialog();
            }

            dlg.FileName = filename;
            var dir = Path.GetDirectoryName(filename);
            if (!string.IsNullOrEmpty(dir))
            {
                dlg.InitialDirectory = dir;
                dlg.FileName         = Path.GetFileName(filename);
            }

            if (dlg.ShowDialog() ?? false)
            {
                return dlg.FileName;
            }

            return null;
        };
    }

    public static void DefaultInitForBaseViewModel(this Window view)
    {
        if (view.DataContext is BaseViewModel vm)
        {
            vm.DefaultInitForBaseViewModel();

            var closeAction = new Action(view.Close);
            var dialogOkAction = new Action(
                () =>
                {
                    view.DialogResult = true;
                    view.Close();
                });
            var dialogCancelAction = new Action(
                () =>
                {
                    view.DialogResult = false;
                    view.Close();
                });
            var loadedEvent = new RoutedEventHandler(
                async (v, e) =>
                {
                    if (view.DataContext is BaseViewModel vmm)
                    {
                        await vmm.Loaded();
                    }
                });

            RoutedEventHandler? unloadedEvent = null;

            unloadedEvent = (v, _) =>
            {
                vm.CloseAction        =  null;
                vm.DialogOKAction     =  null;
                vm.DialogCancelAction =  null;
                view.Loaded           -= loadedEvent;
                view.Unloaded         -= unloadedEvent;
            };

            vm.CloseAction        = closeAction;
            vm.DialogOKAction     = dialogOkAction;
            vm.DialogCancelAction = dialogCancelAction;

            view.Loaded   += loadedEvent;
            view.Unloaded += unloadedEvent;
        }
    }

    public static void DefaultInitForBaseViewModel(this Page view)
    {
        if (view.DataContext is BaseViewModel vm)
        {
            vm.DefaultInitForBaseViewModel();

            view.Loaded += async (v, _) =>
            {
                if (view.DataContext is BaseViewModel vmm)
                {
                    await vmm.Loaded();
                }
            };
        }
    }
}