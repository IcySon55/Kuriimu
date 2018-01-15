using System;
using System.Windows.Forms;
using Kukkii.Properties;

namespace Kukkii
{
    public partial class ErrorLog : Form
    {
        private string _errorText = string.Empty;

        public ErrorLog(string errorText)
        {
            InitializeComponent();

            _errorText = errorText;
        }

        private void Import_Load(object sender, EventArgs e)
        {
            Icon = Resources.kukkii;

            txtErrorLog.Text = _errorText;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}