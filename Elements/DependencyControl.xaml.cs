// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SplittableDataGridSAmple.Elements
{
    public sealed partial class DependencyControl : UserControl
    {
        private string anotherText;



        public string NewText
        {
            get { return (string)GetValue(NewTextProperty); }
            set { SetValue(NewTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NewText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewTextProperty =
            DependencyProperty.Register("NewText", typeof(string), typeof(DependencyControl), new PropertyMetadata(null));



        public string AnotherText
        {
            get { return anotherText; }
            set { anotherText = value; }
        }

        public DependencyControl()
        {
            this.InitializeComponent();
            AnotherText = "no change";

        }
    }
}
