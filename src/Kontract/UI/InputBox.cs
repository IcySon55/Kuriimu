using System;
using System.Windows.Forms;

namespace Kuriimu
{
    public partial class InputBox : Form
    {
        #region Properties

        public string Caption
        {
            get => Text;
            set => Text = value;
        }

        public string Question
        {
            get => lblQuestion.Text;
            set => lblQuestion.Text = value;
        }

        public string Response
        {
            get => txtResponse.Text;
            set => txtResponse.Text = value;
        }

        public int MaxLength
        {
            get => txtResponse.MaxLength;
            set => txtResponse.MaxLength = value;
        }

        #endregion

        public InputBox()
        {
            InitializeComponent();
        }

        public static string Show(string question, string caption, string defaultResponse = "", int maxLength = 0)
        {
            var box = new InputBox
            {
                Caption = caption,
                Question = question,
                Response = defaultResponse,
                MaxLength = maxLength
            };
            box.ShowDialog();
            return box.Response;
        }

        private void Name_Load(object sender, EventArgs e) { }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (txtResponse.Text.Trim().Length > MaxLength && MaxLength > 0)
                MessageBox.Show("The name entered is too long. Valid names can only be " + MaxLength + " character(s) long.", "Input Too Long", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            else
                DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}