using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace maxx53.tools
{
    [ToolboxBitmap(typeof(Form))]
    public class NotifyWindow : Component
    {

        #region PopupForm Class
        public class PopupForm : Form
        {
            #region Custom RichTextBox Class
            public class RichTextLabel : RichTextBox
            {
                public RichTextLabel()
                {
                    base.ReadOnly = true;
                    base.BorderStyle = BorderStyle.None;
                    base.TabStop = false;
                    base.SetStyle(ControlStyles.Selectable, false);
                    this.SetStyle(ControlStyles.Opaque, true);
                    this.SetStyle(ControlStyles.OptimizedDoubleBuffer, false);

                    base.MouseEnter += delegate(object sender, EventArgs e)
                    {
                        this.Cursor = Cursors.Default;
                    };
                }

                const int WM_SETFOCUS = 0x0007;
                const int WM_KILLFOCUS = 0x0008;

                protected override void WndProc(ref Message m)
                {
                    if (m.Msg == WM_SETFOCUS) m.Msg = WM_KILLFOCUS;

                    base.WndProc(ref m);
                }

                protected override CreateParams CreateParams
                {
                    get
                    {
                        CreateParams parms = base.CreateParams;
                        parms.ExStyle |= 0x20;  // Turn on WS_EX_TRANSPARENT
                        return parms;
                    }
                }

                public void ForceRefresh()
                {
                    this.UpdateStyles();
                }

            }
            #endregion

            private Timer OpenAnimTimer = new Timer();
            private Timer CloseAnimTimer = new Timer();

            private Timer LiveTimer = new Timer();
            private RichTextLabel label = new RichTextLabel();

            public string LabelText
            {
                get
                {
                    return label.Text;
                }
                set
                {
                    label.Text = value;
                }
            }

            public string LabelRTF
            {
                get
                {
                    return label.Rtf;
                }
                set
                {
                    label.Rtf = value;
                }
            }
            public static int Time = 2000;
            public static int AnimSpeed = 5;

            private static int AnimStep = 0;
            private double OpacStep =.1;
            private int formY = 0;
            

            public PopupForm(Image _backImage, int pos)
            {
                //Init
                this.OpenAnimTimer.Interval = 16;
                this.OpenAnimTimer.Tick += new System.EventHandler(this.OpenAnimTimer_Tick);

                this.LiveTimer.Interval = Time;
                this.LiveTimer.Tick += new System.EventHandler(this.LiveTimer_Tick);


                this.CloseAnimTimer.Interval = 16;
                this.CloseAnimTimer.Tick += new System.EventHandler(this.CloseAnimTimer_Tick);

                this.BackColor = System.Drawing.Color.Lime;
                this.Controls.Add(this.label);
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.Name = "PopupForm";
                this.ShowInTaskbar = false;
                this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                this.Text = "popupForm";
                this.TransparencyKey = System.Drawing.Color.Lime;
                this.TopMost = true;
                this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PopupForm_FormClosing);
                this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PopupForm_MouseClick);
                this.label.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PopupForm_MouseClick);
                this.label.LinkClicked += new LinkClickedEventHandler(this.label_LinkClicked);

                this.label.Location = new System.Drawing.Point(12, 10);
                this.label.Name = "label";


                if (_backImage != null)
                {
                    this.BackgroundImage = _backImage;
                    this.ClientSize = _backImage.Size;
                    this.label.Size = new Size(_backImage.Size.Width - 24, _backImage.Size.Height - 20);
                }
                else
                {
                    this.ClientSize = new System.Drawing.Size(310, 229);
                    this.BackgroundImage = DrawFilledRectangle(this.ClientSize);
                    this.label.Size = new Size(this.ClientSize.Width - 24, this.ClientSize.Height - 20);

                }

                formY = Screen.PrimaryScreen.WorkingArea.Height - this.Height - this.Height * pos;
                AnimStep = this.Height / AnimSpeed;
                OpacStep = 1.0 / AnimSpeed;



            }


            private Bitmap DrawFilledRectangle(Size size)
            {
                Bitmap bmp = new Bitmap(size.Width, size.Height);
                using (Graphics graph = Graphics.FromImage(bmp))
                {
                    using (Brush aGradientBrush = new LinearGradientBrush(new Point(0, size.Height), new Point(size.Width, 0), Color.White, Color.SkyBlue))
                    {

                        Rectangle ImageSize = new Rectangle(0, 0, size.Width, size.Height);
                        graph.FillRectangle(aGradientBrush, ImageSize);

                    }

                }
                return bmp;
            }


            private void OpenAnimTimer_Tick(object sender, EventArgs e)
            {
                this.Opacity += OpacStep;
                this.Location = new Point(this.Location.X, this.Location.Y - AnimStep);

                if (this.Location.Y <= formY)
                {
                    this.Location = new Point(this.Location.X, formY);
                    this.Opacity = 1;
                    OpenAnimTimer.Stop();
                }
            }


            private void CloseAnimTimer_Tick(object sender, EventArgs e)
            {
                this.Opacity -= OpacStep;
                this.Location = new Point(this.Location.X, this.Location.Y + AnimStep);

                if (this.Location.Y > formY + this.Height)
                {
                    this.Close();
                }
            }

            private void LiveTimer_Tick(object sender, EventArgs e)
            {
                CloseAnimTimer.Enabled = true;
                LiveTimer.Enabled = false;
            }


            public void resetForm()
            {
                this.Opacity = 0;
                this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - this.Width, formY + this.Height);
                OpenAnimTimer.Start();
                LiveTimer.Start();
            }

            private void PopupForm_FormClosing(object sender, FormClosingEventArgs e)
            {
                CloseAnimTimer.Stop();
                this.Visible = false;
                e.Cancel = true;
            }

            private void label_LinkClicked(object sender, LinkClickedEventArgs e)
            {
                System.Diagnostics.Process.Start(e.LinkText);
            }

            private void PopupForm_MouseClick(object sender, MouseEventArgs e)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    this.Close();
                }

            }


        }
        #endregion

        #region Properties

        private int _lifeTime = 2000;
        public int LifeTime
        {
            get
            {
                return _lifeTime;
            }
            set
            {
                _lifeTime = value;
                PopupForm.Time = _lifeTime;
            }
        }

        private int _animSpeed = 5;
        public int AnimSpeed
        {
            get
            {
                return _animSpeed;
            }
            set
            {
                _animSpeed = value;
                PopupForm.AnimSpeed = _animSpeed;
            }
        }

        private int _maxCount = 4;
        public int MaxCount
        {
            get
            {
                return _maxCount;
            }
            set
            {
                _maxCount = value;
                InitForms();
            }
        }

        private Image _backImage = null;
        public Image BackImage
        {
            get
            {
                return _backImage;
            }
            set
            {
                _backImage = value;

                //set image
                for (int i = 0; i < _maxCount; i++)
                {
                    pfArr[i] = new PopupForm(_backImage, i);
                }
            }
        }

        #endregion

        private PopupForm[] pfArr;

        public NotifyWindow()
        {
            InitForms();
        }

        private void InitForms()
        {
            pfArr = new PopupForm[_maxCount];

            for (int i = 0; i < _maxCount; i++)
            {
                pfArr[i] = new PopupForm(_backImage, i);
            }
        }

        public void Show(string text, bool isRTF)
        {
            int pos = 0;

            for (int i = 0; i < pfArr.Length; i++)
            {
                if (pfArr[i].Visible == false)
                {
                    pos = i;
                    break;
                }
            }

            pfArr[pos].resetForm();

            if (isRTF)
                pfArr[pos].LabelRTF = text;
            else
                pfArr[pos].LabelText = text;

            pfArr[pos].Show();

        }

    }
}
