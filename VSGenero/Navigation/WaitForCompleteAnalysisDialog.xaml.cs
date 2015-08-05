/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using Microsoft.VisualStudioTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VSGenero.EditorExtensions.Intellisense;

namespace VSGenero.Navigation
{
    /// <summary>
    /// Interaction logic for WaitForCompleteAnalysisDialog.xaml
    /// </summary>
    partial class WaitForCompleteAnalysisDialog : DialogWindowVersioningWorkaround
    {
        private GeneroProjectAnalyzer _analyzer;

        public WaitForCompleteAnalysisDialog(GeneroProjectAnalyzer analyzer)
        {
            _analyzer = analyzer;
            InitializeComponent();

            new Thread(AnalysisComplete).Start();
        }

        private void _cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void AnalysisComplete()
        {
            _analyzer.WaitForCompleteAnalysis(UpdateItemsRemaining);
        }

        private bool UpdateItemsRemaining(int itemsLeft)
        {
            if (itemsLeft == 0)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    this.DialogResult = true;
                    this.Close();
                }));
                return false;
            }

            bool? dialogResult = null;
            Dispatcher.Invoke((Action)(() =>
            {
                dialogResult = DialogResult;
                if (dialogResult == null)
                {
                    _progress.Maximum = itemsLeft;
                }
            }));


            return dialogResult == null;
        }
    }
}
