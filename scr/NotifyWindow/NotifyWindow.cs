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
           
            public RichTextLabel label = new RichTextLabel();

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
            public static int AnimSpeed = 25;
            public static int Transparency = 100;

            private const int frameDelay = 16;
            private static int AnimStep = 0;
            private double OpacStep =.1;
            private double OpacLevel = 1.0;
            private int formY = 0;

            

            public PopupForm()
            {
                //Create

                this.OpenAnimTimer.Interval = frameDelay;
                this.OpenAnimTimer.Tick += new System.EventHandler(this.OpenAnimTimer_Tick);

                this.LiveTimer.Interval = Time;
                this.LiveTimer.Tick += new System.EventHandler(this.LiveTimer_Tick);

                this.CloseAnimTimer.Interval = frameDelay;
                this.CloseAnimTimer.Tick += new System.EventHandler(this.CloseAnimTimer_Tick);

                this.BackColor = System.Drawing.Color.Lime;
                this.Controls.Add(this.label);
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                this.TransparencyKey = System.Drawing.Color.Lime;
                this.TopMost = true;
                this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PopupForm_FormClosing);
                this.label.Location = new System.Drawing.Point(12, 10);
                this.label.LinkClicked += new LinkClickedEventHandler(this.label_LinkClicked);
            }


            public void InitForm(Image _backImage, int pos, MouseEventHandler mouseDown)
            {
                //Init

                this.MouseDown += mouseDown;
                this.label.MouseDown += mouseDown;

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
                var PrettySpeed = 200 / AnimSpeed;
                AnimStep = this.Height / PrettySpeed;
                OpacLevel = (double)Transparency / 100;
                OpacStep = OpacLevel / (double)PrettySpeed;

                this.Opacity = OpacLevel;
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
                    this.Opacity = OpacLevel;
                    OpenAnimTimer.Stop();
                    LiveTimer.Start();
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
                LiveTimer.Stop();
                CloseAnimTimer.Start();
            }

            public void resetForm()
            {
                this.Opacity = 0;
                this.Visible = false;
                this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - this.Width, formY + this.Height);

                CloseAnimTimer.Enabled = false;
                LiveTimer.Enabled = false;
                OpenAnimTimer.Start();
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
        }
        #endregion

        #region Properties


        private int _lifeTime = 2000;
        [DefaultValue(2000)]
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
                initialized = false;
            }
        }


        private int _animSpeed = 25;
        [DefaultValue(25)]   
        public int AnimSpeed
        {
            get
            {
                return _animSpeed;
            }
            set
            {
                if ((value <= 100) & (value > 0))
                {
                    _animSpeed = value;
                    PopupForm.AnimSpeed = _animSpeed;
                    initialized = false;
                }
                else
                    MessageBox.Show("Value is out of limits.", "AnimSpeed");
            }
        }


        private int _transparency = 100;
        [DefaultValue(100)]
        public int Transparency
        {
            get
            {
                return _transparency;
            }
            set
            {
                if ((value <= 100) & (value > 0))
                {
                    _transparency = value;
                    PopupForm.Transparency = _transparency;
                    initialized = false;
                }
                else
                    MessageBox.Show("Value is out of limits.", "Transparency");


            }
        }
 
        private int _maxCount = 4;
        [DefaultValue(4)]  
        public int MaxCount
        {
            get
            {
                return _maxCount;
            }
            set
            {
                _maxCount = value;
                CreateForms();
                initialized = false;
            }
        }

        private Image _backImage = null;
        [DefaultValue(null)]  
        public Image BackImage
        {
            get
            {
                return _backImage;
            }
            set
            {
                _backImage = value;
                initialized = false;
            }
        }

        #endregion

        public event MouseEventHandler OnMouseDown;

        private PopupForm[] pfArr;
        private bool initialized = false;
        private int busyPos = -1;

        public NotifyWindow()
        {
            CreateForms();
        }

        private void CreateForms()
        {
            pfArr = new PopupForm[_maxCount];

            for (int i = 0; i < _maxCount; i++)
            {
                pfArr[i] = new PopupForm();
            }
        }

        public void InitForms()
        {
            for (int i = 0; i < _maxCount; i++)
            {
                pfArr[i].InitForm(_backImage, i, OnMouseDown);
            }
        }


        public void Show(string text, bool isRTF)
        {
            if (!initialized)
            {
                InitForms();
                initialized = true;
            }

            int pos = 0;
            bool isBusy = true;

            for (int i = 0; i < pfArr.Length; i++)
            {
                if (pfArr[i].Visible == false)
                {
                    pos = i;
                    isBusy = false;
                    break;
                }
            }

            if (isBusy)
            {
                busyPos++;

                if (busyPos == MaxCount)
                {
                    //reset count
                    busyPos = 0;
                }

                pos = busyPos;
            }
            else busyPos = -1;


            pfArr[pos].resetForm();

            if (isRTF)
                pfArr[pos].LabelRTF = text;
            else
                pfArr[pos].LabelText = text;

            pfArr[pos].Show();

        }

    }
}
