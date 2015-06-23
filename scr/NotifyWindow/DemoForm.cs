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


    }
}
