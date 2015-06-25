using System;
using System.Windows.Forms;

namespace maxx53.tools
{
    public partial class DemoForm : Form
    {
        public DemoForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            notifyWindow1.Show(textBox1.Text, true);
        }

        private void notifyWindow1_OnMouseDown(object sender, MouseEventArgs e)
        {
            MessageBox.Show("Button: " + e.Button.ToString() + Environment.NewLine + "Position: " + e.Location.ToString(), "Handle your mouse downs!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}
