using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using qf4net;

namespace Samples.Lighter
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
    public class Form1 : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button buttonCreateLighter;
        private System.Windows.Forms.Button buttonFlintSpin;
        private System.Windows.Forms.Button buttonValvePress;
        private System.Windows.Forms.Button buttonReleaseValve;
        private System.Windows.Forms.Button buttonIncreaseAirFlow;
        private System.Windows.Forms.Button buttonDecreaseAirFlow;
        private System.Windows.Forms.Button buttonIncreaseFuelFlow;
        private System.Windows.Forms.Button buttonDecreaseFuelFlow;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxFuelMixtureState;
        private System.Windows.Forms.TextBox textBoxAirFlowState;
        private System.Windows.Forms.TextBox textBoxValveState;
        private System.Windows.Forms.TextBox textBoxFlintState;
        private System.Windows.Forms.PictureBox pictureBoxNotLit;
        private System.Windows.Forms.PictureBox pictureBoxLitUp;
        private System.Windows.Forms.Panel panelInfo;
        private System.Windows.Forms.Label labelPictureWikiPedia;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public Form1()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            InitHsmRunner ();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if (components != null) 
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Form1));
            this.buttonCreateLighter = new System.Windows.Forms.Button();
            this.buttonFlintSpin = new System.Windows.Forms.Button();
            this.buttonValvePress = new System.Windows.Forms.Button();
            this.buttonReleaseValve = new System.Windows.Forms.Button();
            this.buttonIncreaseAirFlow = new System.Windows.Forms.Button();
            this.buttonDecreaseAirFlow = new System.Windows.Forms.Button();
            this.buttonIncreaseFuelFlow = new System.Windows.Forms.Button();
            this.buttonDecreaseFuelFlow = new System.Windows.Forms.Button();
            this.panelInfo = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxFuelMixtureState = new System.Windows.Forms.TextBox();
            this.textBoxAirFlowState = new System.Windows.Forms.TextBox();
            this.textBoxValveState = new System.Windows.Forms.TextBox();
            this.textBoxFlintState = new System.Windows.Forms.TextBox();
            this.pictureBoxNotLit = new System.Windows.Forms.PictureBox();
            this.pictureBoxLitUp = new System.Windows.Forms.PictureBox();
            this.labelPictureWikiPedia = new System.Windows.Forms.Label();
            this.panelInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCreateLighter
            // 
            this.buttonCreateLighter.Location = new System.Drawing.Point(26, 24);
            this.buttonCreateLighter.Name = "buttonCreateLighter";
            this.buttonCreateLighter.Size = new System.Drawing.Size(146, 23);
            this.buttonCreateLighter.TabIndex = 0;
            this.buttonCreateLighter.Text = "Create Lighter Elements";
            this.buttonCreateLighter.Click += new System.EventHandler(this.buttonCreateLighter_Click);
            // 
            // buttonFlintSpin
            // 
            this.buttonFlintSpin.Enabled = false;
            this.buttonFlintSpin.Location = new System.Drawing.Point(560, 66);
            this.buttonFlintSpin.Name = "buttonFlintSpin";
            this.buttonFlintSpin.Size = new System.Drawing.Size(150, 23);
            this.buttonFlintSpin.TabIndex = 4;
            this.buttonFlintSpin.Text = "Strike Flint";
            this.buttonFlintSpin.Click += new System.EventHandler(this.buttonFlintSpin_Click);
            // 
            // buttonValvePress
            // 
            this.buttonValvePress.Enabled = false;
            this.buttonValvePress.Location = new System.Drawing.Point(26, 66);
            this.buttonValvePress.Name = "buttonValvePress";
            this.buttonValvePress.Size = new System.Drawing.Size(146, 23);
            this.buttonValvePress.TabIndex = 1;
            this.buttonValvePress.Text = "Press Valve Switch";
            this.buttonValvePress.Click += new System.EventHandler(this.buttonValvePress_Click);
            // 
            // buttonReleaseValve
            // 
            this.buttonReleaseValve.Enabled = false;
            this.buttonReleaseValve.Location = new System.Drawing.Point(26, 102);
            this.buttonReleaseValve.Name = "buttonReleaseValve";
            this.buttonReleaseValve.Size = new System.Drawing.Size(146, 23);
            this.buttonReleaseValve.TabIndex = 5;
            this.buttonReleaseValve.Text = "Release Valve Switch";
            this.buttonReleaseValve.Click += new System.EventHandler(this.buttonReleaseValve_Click);
            // 
            // buttonIncreaseAirFlow
            // 
            this.buttonIncreaseAirFlow.Enabled = false;
            this.buttonIncreaseAirFlow.Location = new System.Drawing.Point(206, 66);
            this.buttonIncreaseAirFlow.Name = "buttonIncreaseAirFlow";
            this.buttonIncreaseAirFlow.Size = new System.Drawing.Size(144, 23);
            this.buttonIncreaseAirFlow.TabIndex = 2;
            this.buttonIncreaseAirFlow.Text = "Increase Air Flow";
            this.buttonIncreaseAirFlow.Click += new System.EventHandler(this.buttonIncreaseAirFlow_Click);
            // 
            // buttonDecreaseAirFlow
            // 
            this.buttonDecreaseAirFlow.Enabled = false;
            this.buttonDecreaseAirFlow.Location = new System.Drawing.Point(206, 102);
            this.buttonDecreaseAirFlow.Name = "buttonDecreaseAirFlow";
            this.buttonDecreaseAirFlow.Size = new System.Drawing.Size(144, 23);
            this.buttonDecreaseAirFlow.TabIndex = 6;
            this.buttonDecreaseAirFlow.Text = "Decrease Air Flow";
            this.buttonDecreaseAirFlow.Click += new System.EventHandler(this.buttonDecreaseAirFlow_Click);
            // 
            // buttonIncreaseFuelFlow
            // 
            this.buttonIncreaseFuelFlow.Enabled = false;
            this.buttonIncreaseFuelFlow.Location = new System.Drawing.Point(390, 66);
            this.buttonIncreaseFuelFlow.Name = "buttonIncreaseFuelFlow";
            this.buttonIncreaseFuelFlow.Size = new System.Drawing.Size(144, 23);
            this.buttonIncreaseFuelFlow.TabIndex = 3;
            this.buttonIncreaseFuelFlow.Text = "Increase Fuel Flow";
            this.buttonIncreaseFuelFlow.Click += new System.EventHandler(this.buttonIncreaseFuelFlow_Click);
            // 
            // buttonDecreaseFuelFlow
            // 
            this.buttonDecreaseFuelFlow.Enabled = false;
            this.buttonDecreaseFuelFlow.Location = new System.Drawing.Point(390, 102);
            this.buttonDecreaseFuelFlow.Name = "buttonDecreaseFuelFlow";
            this.buttonDecreaseFuelFlow.Size = new System.Drawing.Size(144, 23);
            this.buttonDecreaseFuelFlow.TabIndex = 7;
            this.buttonDecreaseFuelFlow.Text = "Decrease Fuel Flow";
            this.buttonDecreaseFuelFlow.Click += new System.EventHandler(this.buttonDecreaseFuelFlow_Click);
            // 
            // panelInfo
            // 
            this.panelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.panelInfo.Controls.Add(this.textBoxFuelMixtureState);
            this.panelInfo.Controls.Add(this.label4);
            this.panelInfo.Controls.Add(this.label3);
            this.panelInfo.Controls.Add(this.label2);
            this.panelInfo.Controls.Add(this.label1);
            this.panelInfo.Controls.Add(this.textBoxAirFlowState);
            this.panelInfo.Controls.Add(this.textBoxValveState);
            this.panelInfo.Controls.Add(this.textBoxFlintState);
            this.panelInfo.Location = new System.Drawing.Point(30, 172);
            this.panelInfo.Name = "panelInfo";
            this.panelInfo.Size = new System.Drawing.Size(354, 230);
            this.panelInfo.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(18, 20);
            this.label1.Name = "label1";
            this.label1.TabIndex = 0;
            this.label1.Text = "FuelMixture State";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(18, 68);
            this.label2.Name = "label2";
            this.label2.TabIndex = 1;
            this.label2.Text = "Air Flow State";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(18, 114);
            this.label3.Name = "label3";
            this.label3.TabIndex = 2;
            this.label3.Text = "Valve State";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(18, 162);
            this.label4.Name = "label4";
            this.label4.TabIndex = 3;
            this.label4.Text = "Flint State";
            // 
            // textBoxFuelMixtureState
            // 
            this.textBoxFuelMixtureState.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFuelMixtureState.Location = new System.Drawing.Point(140, 18);
            this.textBoxFuelMixtureState.Name = "textBoxFuelMixtureState";
            this.textBoxFuelMixtureState.ReadOnly = true;
            this.textBoxFuelMixtureState.Size = new System.Drawing.Size(200, 20);
            this.textBoxFuelMixtureState.TabIndex = 0;
            this.textBoxFuelMixtureState.Text = "";
            // 
            // textBoxAirFlowState
            // 
            this.textBoxAirFlowState.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAirFlowState.Location = new System.Drawing.Point(140, 70);
            this.textBoxAirFlowState.Name = "textBoxAirFlowState";
            this.textBoxAirFlowState.ReadOnly = true;
            this.textBoxAirFlowState.Size = new System.Drawing.Size(200, 20);
            this.textBoxAirFlowState.TabIndex = 1;
            this.textBoxAirFlowState.Text = "";
            // 
            // textBoxValveState
            // 
            this.textBoxValveState.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxValveState.Location = new System.Drawing.Point(140, 118);
            this.textBoxValveState.Name = "textBoxValveState";
            this.textBoxValveState.ReadOnly = true;
            this.textBoxValveState.Size = new System.Drawing.Size(200, 20);
            this.textBoxValveState.TabIndex = 2;
            this.textBoxValveState.Text = "";
            // 
            // textBoxFlintState
            // 
            this.textBoxFlintState.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFlintState.Location = new System.Drawing.Point(140, 168);
            this.textBoxFlintState.Name = "textBoxFlintState";
            this.textBoxFlintState.ReadOnly = true;
            this.textBoxFlintState.Size = new System.Drawing.Size(200, 20);
            this.textBoxFlintState.TabIndex = 3;
            this.textBoxFlintState.Text = "";
            // 
            // pictureBoxNotLit
            // 
            this.pictureBoxNotLit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxNotLit.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxNotLit.Image")));
            this.pictureBoxNotLit.Location = new System.Drawing.Point(434, 172);
            this.pictureBoxNotLit.Name = "pictureBoxNotLit";
            this.pictureBoxNotLit.Size = new System.Drawing.Size(218, 218);
            this.pictureBoxNotLit.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxNotLit.TabIndex = 9;
            this.pictureBoxNotLit.TabStop = false;
            this.pictureBoxNotLit.Visible = false;
            // 
            // pictureBoxLitUp
            // 
            this.pictureBoxLitUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxLitUp.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxLitUp.Image")));
            this.pictureBoxLitUp.Location = new System.Drawing.Point(442, 192);
            this.pictureBoxLitUp.Name = "pictureBoxLitUp";
            this.pictureBoxLitUp.Size = new System.Drawing.Size(212, 174);
            this.pictureBoxLitUp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxLitUp.TabIndex = 10;
            this.pictureBoxLitUp.TabStop = false;
            this.pictureBoxLitUp.Visible = false;
            // 
            // labelPictureWikiPedia
            // 
            this.labelPictureWikiPedia.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelPictureWikiPedia.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.labelPictureWikiPedia.Location = new System.Drawing.Point(8, 418);
            this.labelPictureWikiPedia.Name = "labelPictureWikiPedia";
            this.labelPictureWikiPedia.Size = new System.Drawing.Size(260, 23);
            this.labelPictureWikiPedia.TabIndex = 12;
            this.labelPictureWikiPedia.Text = "Pictures from WikiPedia";
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(732, 450);
            this.Controls.Add(this.labelPictureWikiPedia);
            this.Controls.Add(this.pictureBoxLitUp);
            this.Controls.Add(this.pictureBoxNotLit);
            this.Controls.Add(this.panelInfo);
            this.Controls.Add(this.buttonIncreaseAirFlow);
            this.Controls.Add(this.buttonReleaseValve);
            this.Controls.Add(this.buttonValvePress);
            this.Controls.Add(this.buttonFlintSpin);
            this.Controls.Add(this.buttonCreateLighter);
            this.Controls.Add(this.buttonDecreaseAirFlow);
            this.Controls.Add(this.buttonIncreaseFuelFlow);
            this.Controls.Add(this.buttonDecreaseFuelFlow);
            this.Name = "Form1";
            this.Text = "Lighter Sample";
            this.panelInfo.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() 
        {
            Application.Run(new Form1());
        }

		IHsmExecutionModel _SharedExecutionModel;

		private void InitHsmRunner()
		{
			_SharedExecutionModel = new MultipleHsmsPerThread();
		}

        LighterFrame frame;
        int _Counter = 0;
        private void buttonCreateLighter_Click(object sender, System.EventArgs e)
        {
            _Counter++;
            string id = _Counter.ToString (); // Guid.NewGuid ().ToString ();
            frame = _SharedExecutionModel.CreateHsm (id);
            
            frame.StateChange += new EventHandler(stateChange);
            
            foreach (Control control in Controls)
            {
                if(control is Button)
                {
                    control.Enabled = true;
                }
            }
            
            buttonCreateLighter.Enabled = false;
            
            UpdateStates ();
        }
	    
        private void stateChange(object sender, EventArgs e)
        {
            if(this.InvokeRequired)
            {
                this.Invoke (new EventHandler (stateChange), new object[] {e});
            } 
            else
            {
                UpdateStates ();
            }
        }	    

        private void UpdateStates ()
        {
            textBoxFuelMixtureState.Text = frame.FuelMixtureState;
            textBoxAirFlowState.Text = frame.AirFlowState;
            textBoxFlintState.Text = frame.FlintState;
            textBoxValveState.Text = frame.ValveState;
            
            pictureBoxLitUp.Visible = frame.FrameIsLit;
            pictureBoxNotLit.Visible = !pictureBoxLitUp.Visible;
        }
        private void buttonFlintSpin_Click(object sender, System.EventArgs e)
        {
            frame.SpinFlint ();
        }

        private void buttonValvePress_Click(object sender, System.EventArgs e)
        {
            frame.PressValve ();
        }

        private void buttonReleaseValve_Click(object sender, System.EventArgs e)
        {
            frame.ReleaseValve ();
        }

        private void buttonIncreaseAirFlow_Click(object sender, System.EventArgs e)
        {
            frame.IncreaseAirFlow ();
        }

        private void buttonDecreaseAirFlow_Click(object sender, System.EventArgs e)
        {
            frame.DecreaseAirFlow ();
        }

        private void buttonIncreaseFuelFlow_Click(object sender, System.EventArgs e)
        {
            frame.IncreaseFuelFlow ();
        }

        private void buttonDecreaseFuelFlow_Click(object sender, System.EventArgs e)
        {
            frame.DecreaseFuelFlow ();
        }
    }
}
