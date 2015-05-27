using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace Microsoft.VisualStudio.VSCommon.Utilities
{
    public partial class ExceptionForm : Form
    {
        private Exception _currentException;
        private bool _generalPopulated, _stackTracePopulated, _innerExceptionPopulated, _otherPopulated;

        public ExceptionForm(Exception e)
        {
            _currentException = e;
            InitializeComponent();
        }

        private void ExceptionForm_Load(object sender, EventArgs e)
        {
            // process the exception to display the information available.
            // Only do the general information for right now
            PopulateGeneralInfo();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            string filename = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(ExceptionForm)).Location), string.Format("exception_info_{0}.txt", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")));
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                SaveGeneralInfo(sw);
                sw.WriteLine();
                SaveStackTrace(sw);
                sw.WriteLine();
                SaveInnerException(sw);
                sw.WriteLine();
                SaveOtherInfo(sw);
            }

            Process.Start("notepad.exe", filename);
        }

        private void SaveGeneralInfo(StreamWriter sw)
        {
            sw.WriteLine(string.Format("Exception Type: {0}", _currentException.GetType().FullName));
            sw.WriteLine(string.Format("Message: {0}", _currentException.Message));
            sw.WriteLine(string.Format("Source: {0}", _currentException.Source));
            sw.WriteLine(string.Format("TargetMethod: {0}", GetTargetMethodFormat(_currentException)));
        }

        private void SaveStackTrace(StreamWriter sw)
        {
            sw.WriteLine("StackTrace:");
            string[] stackTrace = _currentException.StackTrace.Split(new char[] { '\n' });

            foreach (string st in stackTrace)
            {
                sw.WriteLine("\t" + st);
            }
        }

        private void SaveInnerException(StreamWriter sw)
        {
            sw.WriteLine("InnerException: ");

            Exception innerEx = _currentException;

            string tabs = "\t";

            while (null != innerEx)
            {
                sw.WriteLine(tabs + innerEx.GetType().FullName);
                sw.WriteLine(tabs + innerEx.Message);
                sw.WriteLine(tabs + GetTargetMethodFormat(innerEx));

                innerEx = innerEx.InnerException;
                tabs += "\t";
            }
        }

        private void SaveOtherInfo(StreamWriter sw)
        {
            sw.WriteLine("Other Info: ");

            Hashtable ht =
            this.GetCustomExceptionInfo(_currentException);
            IDictionaryEnumerator ide = ht.GetEnumerator();

            while (ide.MoveNext())
            {
                sw.WriteLine("\t" + ide.Key.ToString());
                if (null != ide.Value)
                {
                    sw.WriteLine("\t\t" + ide.Value.ToString());
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabGeneralInfo)
            {
                PopulateGeneralInfo();
            }
            else if (tabControl1.SelectedTab == tabStackTrace)
            {
                PopulateStackTrace();
            }
            else if (tabControl1.SelectedTab == tabInnerException)
            {
                PopulateInnerException();
            }
            else
            {
                PopulateOtherInformation();
            }
        }

        private void PopulateGeneralInfo()
        {
            if (!_generalPopulated)
            {
                textBoxExceptionType.Text = _currentException.GetType().FullName;
                textBoxGeneralMessage.Text = _currentException.Message;
                textBoxGeneralSource.Text = _currentException.Source;
                textBoxGeneralTargetMethod.Text = GetTargetMethodFormat(_currentException);
                _generalPopulated = true;
            }
        }

        private string GetTargetMethodFormat(Exception e)
        {
            if (e.TargetSite != null &&
                e.TargetSite.DeclaringType != null &&
                e.TargetSite.DeclaringType.Assembly != null &&
                e.TargetSite.Name != null)
            {
                var name = e.TargetSite.DeclaringType.Assembly.GetName();
                if(name != null && name.Name != null)
                {
                    return "[" +
                    name.Name +
                    "]" + e.TargetSite.DeclaringType +
                    "::" + e.TargetSite.Name + "()";
                }
            }

            return "";
        }

        private void PopulateStackTrace()
        {
            if (!_stackTracePopulated && _currentException.StackTrace != null)
            {
                string[] stackTrace = _currentException.StackTrace.Split(new char[] { '\n' });

                foreach (string st in stackTrace)
                {
                    this.listViewStackTrace.Items.Add(new ListViewItem(st));
                }

                _stackTracePopulated = true;
            }
        }

        private void PopulateInnerException()
        {
            if (!_innerExceptionPopulated)
            {
                Exception innerEx = _currentException;
                TreeNode parentNode = null,
                    childNode = null, childMessage = null,
                    childTarget = null;

                this.treeViewInnerException.BeginUpdate();

                while (null != innerEx)
                {
                    childNode = new TreeNode(
                        innerEx.GetType().FullName);
                    childMessage = new TreeNode(
                        innerEx.Message);
                    childTarget = new TreeNode(
                        GetTargetMethodFormat(innerEx));

                    childNode.Nodes.Add(childMessage);
                    childNode.Nodes.Add(childTarget);

                    if (null != parentNode)
                    {
                        parentNode.Nodes.Add(childNode);
                    }
                    else
                    {
                        this.treeViewInnerException.Nodes.Add(
                            childNode);
                    }

                    parentNode = childNode;
                    innerEx = innerEx.InnerException;
                }

                this.treeViewInnerException.EndUpdate();

                _innerExceptionPopulated = true;
            }
        }

        private void PopulateOtherInformation()
        {
            if (!_otherPopulated)
            {
                Hashtable ht =
            this.GetCustomExceptionInfo(_currentException);
                IDictionaryEnumerator ide = ht.GetEnumerator();

                this.listViewOtherInfo.Items.Clear();
                this.listViewOtherInfo.BeginUpdate();

                ListViewItem lvi;

                while (ide.MoveNext())
                {
                    lvi = new ListViewItem(ide.Key.ToString());
                    if (null != ide.Value)
                    {
                        lvi.SubItems.Add(ide.Value.ToString());
                    }
                    this.listViewOtherInfo.Items.Add(lvi);
                }

                this.listViewOtherInfo.EndUpdate();

                _otherPopulated = true;
            }
        }

        private Hashtable GetCustomExceptionInfo(Exception Ex)
        {
            Hashtable customInfo = new Hashtable();

            foreach (PropertyInfo pi in
                Ex.GetType().GetProperties())
            {
                Type baseEx = typeof(System.Exception);

                if (null == baseEx.GetProperty(pi.Name))
                {
                    customInfo.Add(pi.Name,
                        pi.GetValue(Ex, null));
                }
            }

            return customInfo;
        }
    }
}
