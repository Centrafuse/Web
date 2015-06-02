using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using System.Text;
using centrafuse.Plugins;

namespace web
{
	public class web : CFPlugin
    {

#region Variables

		private string dialpage = "";
		private string currenturl = "";
        private string currenturlfront = "";
        private string currenturlrear = "";
        private int capturepage = -1;
        private const int numhomepagelinks = 9;
		private bool reload = false;
		private bool firstshow = true;
		private bool autodial = false;
		private bool isfullscreen = false;
		private bool dialing = false;
		private bool disablepopups = true;
		private bool autohide = false;
		private bool firstrun = true;
        private bool showingLoading = false;
        private bool wasrearscreen = false;
        private bool loadhomepageonstart = true;

        public static string appdatapath = "";
        public static string startuppath = "";

        private static bool logEvents = false;

        private class homeurls
        {
            public string one = String.Empty;
            public string two = String.Empty;
            public string three = String.Empty;
            public string four = String.Empty;
            public string five = String.Empty;
            public string six = String.Empty;
            public string seven = String.Empty;
            public string eight = String.Empty;
            public string nine = String.Empty;
        }
        private homeurls homeurl = new homeurls();

		private Timer backTimer;
		private Timer hideTimer;
		private Timer hideStatusTimer;

        private WebBrowser webBrowser;

        public static bool goback = false;
        public static bool statusbar = true;

#if !WindowsCE

		private MyMessageFilter msgFilter;

		public class MyMessageFilter : IMessageFilter 
		{
			web mainForm = null;

			public MyMessageFilter(web pForm)
			{
				mainForm = pForm;
			}

			public bool PreFilterMessage(ref Message m) 
			{
				switch(m.Msg)
				{
					case Win32.WM_LBUTTONDOWN:
						return mainForm.gestureDown();
					case Win32.WM_LBUTTONUP:
						return mainForm.gestureUp();
					case Win32.WM_MOUSEMOVE:
						return mainForm.gestureMove();
					case Win32.WM_LBUTTONDBLCLK:
						return mainForm.gestureDoubleClick();
				}

				return false;
			}
		}

#endif

#endregion

#region Construction

        public web()
		{
#if !WindowsCE
			msgFilter = new MyMessageFilter(this);
#endif
            web.appdatapath = CFTools.AppDataPath;
            web.startuppath = CFTools.StartupPath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web";
		}

#endregion	

#region Gestures
#if !WindowsCE

        public static int startx = 0;
		public static int starty = 0;
		public static int motionx = 0;
		public static int motiony = 0;
		public static bool movingup = false;
		public static bool movingdown = false;
        public static bool movingleft = false;
        public static bool movingright = false;
		public static bool firedmotion = false;
		public static bool gesturestart = false;
		public static bool gesturedown = false;


		public bool gestureDoubleClick()
		{
			bool retvalue = false;

			try
			{
				web.gesturestart = false;
				web.gesturedown = false;

                if (this.webBrowser != null)
				{
                    Rectangle realpos = new Rectangle(this.Bounds.X + this.webBrowser.Bounds.X, this.Bounds.Y + this.webBrowser.Bounds.Y, this.webBrowser.Bounds.Width, this.webBrowser.Bounds.Height);
				
					if(Cursor.Position.X >= realpos.X && Cursor.Position.X <= ((realpos.X + realpos.Width) - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth) && Cursor.Position.Y >= realpos.Y && Cursor.Position.Y <= ((realpos.Y + realpos.Height) - System.Windows.Forms.SystemInformation.HorizontalScrollBarHeight))
					{
						if(!web.statusbar)
						{
							ChangeStatus(false);
							hideStatusTimer.Enabled = true;
						}
						else
						{
							ChangeStatus(true);
							hideStatusTimer.Enabled = false;
						}

						retvalue = true;
					}
				}

			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }

			return retvalue;
		}


		public bool gestureDown()
		{
			bool retvalue = false;

			try
			{
                if (this.webBrowser != null)
				{
                    Rectangle realpos = new Rectangle(this.Bounds.X + this.webBrowser.Bounds.X, this.Bounds.Y + this.webBrowser.Bounds.Y, this.webBrowser.Bounds.Width, this.webBrowser.Bounds.Height);
				
					if(Cursor.Position.X >= realpos.X && Cursor.Position.X <= ((realpos.X + realpos.Width) - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth) && Cursor.Position.Y >= realpos.Y && Cursor.Position.Y <= ((realpos.Y + realpos.Height) - System.Windows.Forms.SystemInformation.HorizontalScrollBarHeight))
					{
						web.startx = Cursor.Position.X;
						web.starty = Cursor.Position.Y;
						web.gesturestart = true;
					}
				}
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }

			return retvalue;
		}


		public bool gestureUp()
		{
			bool retvalue = false;

			try
			{
				web.gesturestart = false;
				web.gesturedown = false;

				if(web.firedmotion || web.movingup || web.movingdown)
				{
					web.startx = 0;
					web.starty = 0;
					web.motionx = 0;
					web.motiony = 0;

					retvalue = true;

					web.movingup = false;
					web.movingdown = false;
					web.firedmotion = false;
				}
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }

			return retvalue;
		}


		private bool gestureMove()
		{
			bool retvalue = false;

			try
			{
				if(web.gesturestart && !web.gesturedown && (Math.Abs(web.startx - Cursor.Position.X) > 4 || Math.Abs(web.starty - Cursor.Position.Y) > 4))
					web.gesturedown = true;

				if(web.gesturestart && web.gesturedown)
				{
					retvalue = true;

					if(web.movingup && (Cursor.Position.Y - web.motiony) < 0)
					{
						web.movingup = false;
						web.startx = Cursor.Position.X;
						web.starty = Cursor.Position.Y;
					}
					else if(web.movingdown && (Cursor.Position.Y - web.motiony) < 0)
					{
						web.movingdown = false;
						web.startx = Cursor.Position.X;
						web.starty = Cursor.Position.Y;
					}
                    else if (web.movingleft && (Cursor.Position.X - web.motionx) < 0)
                    {
                        web.movingleft = false;
                        web.startx = Cursor.Position.X;
                        web.starty = Cursor.Position.Y;
                    }
                    else if (web.movingright && (Cursor.Position.X - web.motionx) < 0)
                    {
                        web.movingright = false;
                        web.startx = Cursor.Position.X;
                        web.starty = Cursor.Position.Y;
                    }

					web.motionx = Cursor.Position.X;
					web.motiony = Cursor.Position.Y;

					int xdiff = web.startx - web.motionx;
					int ydiff = web.starty - web.motiony;

                    if (ydiff <= (20 * -1) && !web.firedmotion && !web.movingleft && !web.movingright)
					{
						web.movingup = true;
						browsersend("{UP}");
					}
                    else if (ydiff >= 20 && !web.firedmotion && !web.movingleft && !web.movingright)
					{
						web.movingdown = true;
						browsersend("{DOWN}");
					}
					else if(xdiff <= (20 * -1) && !web.firedmotion && !web.movingup && !web.movingdown)
					{
						//web.firedmotion = true;
                        web.movingright = true;
                        //try { webBrowser.GoForward(); }
						//catch {}
                        browsersend("{RIGHT}");
					}
					else if(xdiff >= 20 && !web.firedmotion && !web.movingup && !web.movingdown)
					{
						//web.firedmotion = true;
                        web.movingleft = true;
                        //try { webBrowser.GoBack(); }
						//catch {}
                        browsersend("{LEFT}");
					}
				}
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }

			return retvalue;
		}

#endif
#endregion

#region CFPlugin

		public override void CF_sectionToggleFullScreen()
		{
			minmax();
		}


		public override void CF_pluginInit()
		{
            // Supports rear screen display
            this.CF_params.supportsRearScreen = true;

            this.CF3_initPlugin("Web", true);

			LoadSettings();

            this.CF_localskinsetup();

            backTimer = new Timer();
            backTimer.Enabled = true;
            backTimer.Interval = 500;
            backTimer.Tick += new EventHandler(backTimer_Tick);

            hideTimer = new Timer();
            hideTimer.Enabled = false;
            hideTimer.Interval = 35000;
            hideTimer.Tick += new EventHandler(hideTimer_Tick);

            hideStatusTimer = new Timer();
            hideStatusTimer.Enabled = false;
            hideStatusTimer.Interval = 5000;
            hideStatusTimer.Tick += new EventHandler(hideStatusTimer_Tick);
		}


		public override void CF_localskinsetup()
		{
            CFTools.writeLog("WEB", "CF_localskinsetup", "isfullscreen = " + isfullscreen.ToString());
			try
			{
                if (isfullscreen || (CFTools.AllScreens.Length >= CF_displayHooks.displayNumber && CF_displayHooks.displayNumber > 1 && !CF_displayHooks.rearScreen))
                {
                    if (CFTools.AllScreens.Length >= CF_displayHooks.displayNumber && CF_displayHooks.displayNumber > 1)
                        CF_displayHooks.boundsAttributeId = "secbounds";
                    else
                        CF_displayHooks.boundsAttributeId = "fullbounds";
                }
                else
                    CF_displayHooks.boundsAttributeId = "bounds";

                CF3_initSection("Web");

                Rectangle browserbounds = new Rectangle();

                if (isfullscreen || (CFTools.AllScreens.Length >= CF_displayHooks.displayNumber && CF_displayHooks.displayNumber > 1 && !CF_displayHooks.rearScreen))
				{
					isfullscreen = true;
					if (CFTools.AllScreens.Length >= CF_displayHooks.displayNumber && CF_displayHooks.displayNumber > 1)
                        browserbounds = this.CF_createRect(SkinReader.ParseBounds(this.pluginSkinReader.GetControlAttribute("Web", "IEContainer", "secbounds")), true);
					else
                        browserbounds = this.CF_createRect(SkinReader.ParseBounds(this.pluginSkinReader.GetControlAttribute("Web", "IEContainer", "fullbounds")), true);
				}
				else
                    browserbounds = this.CF_createRect(SkinReader.ParseBounds(this.pluginSkinReader.GetControlAttribute("Web", "IEContainer", "bounds")));


                if (this.webBrowser != null)
                    this.webBrowser.Bounds = browserbounds;

                this.CF_createButtonClick("URL", new MouseEventHandler(navBtn_Click));
                this.CF_createButtonClick("UNLOAD", new MouseEventHandler(unload_Click));
                this.CF_createButtonClick("BACK", new MouseEventHandler(backBtn_Click));
                this.CF_createButtonClick("FORWARD", new MouseEventHandler(forwardBtn_Click));
                this.CF_createButtonClick("STOP", new MouseEventHandler(stopBtn_Click));
                this.CF_createButtonClick("REFRESH", new MouseEventHandler(reloadBtn_Click));
                this.CF_createButtonClick("HOME", new MouseEventHandler(homeBtn_Click));
                this.CF_createButtonClick("FAVORITES", new MouseEventHandler(favBtn_Click));
                this.CF_createButtonClick("KEYBOARD", new MouseEventHandler(keyboardBtn_Click));
                this.CF_createButtonClick("MINMAX", new MouseEventHandler(minmaxBtn_Click));
                this.CF_createButtonClick("PAGEUP", new MouseEventHandler(pageupBtn_Click));
                this.CF_createButtonClick("PAGEDOWN", new MouseEventHandler(pagedownBtn_Click));

                if (wasrearscreen != CF_displayHooks.rearScreen && this.webBrowser != null)
                {
                    if (!CF_displayHooks.rearScreen && currenturlfront != "")
                        BrowseTo(currenturlfront);
                    else if (currenturlrear != "")
                        BrowseTo(currenturlrear);
                }

                wasrearscreen = CF_displayHooks.rearScreen;
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}


		public override void CF_pluginClose()
		{
			try
			{
                pluginConfig.WriteField("/APPCONFIG/LASTPAGE", currenturlfront);
                pluginConfig.WriteField("/APPCONFIG/LASTPAGEREAR", currenturlfront);
                pluginConfig.Save();
			}
			catch{}

            if (this.webBrowser != null)
                this.webBrowser.Dispose();

			this.Dispose();
		}


		public override void CF_pluginShow()
		{
            CFTools.writeLog("WEB", "CF_pluginShow", "");

			try
			{
				if(firstshow)
				{
					LoadWeb();
					firstshow = false;
				}

				/*
				 * Shows the plugin.
				*/
				this.Visible = true;

				/*
				 * Navigates to homepage or current URL.
				*/
                if (this.reload && this.currenturl != "")
                {
                    this.reload = false;
                    Application.DoEvents();

                    if (this.CF_getConnectionStatus())
                    {
                        BrowseTo(currenturl);
                    }
                    else if (autodial && !dialing)
                    {
                        dialing = true;
                        dialpage = currenturl;
                        this.CF_systemCommand(CF_Actions.CONNECT, this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");

                        if (this.CF_getConnectionStatus())
                            BrowseTo(currenturl);
                    }
                }
                else if (this.currenturl == "")
                {
                    this.reload = false;
                    Application.DoEvents();

                    string homepage = this.pluginConfig.ReadField("/APPCONFIG/HOMEURL");
                    if (homepage == null || homepage == "")
                        homepage = "file://" + web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "homepage.htm";

                    if (this.CF_getConnectionStatus())
                    {
                        BrowseTo(homepage);
                    }
                    else if (autodial && !dialing)
                    {
                        dialing = true;
                        dialpage = homepage;
                        this.CF_systemCommand(CF_Actions.CONNECT, this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");

                        if (this.CF_getConnectionStatus())
                            BrowseTo(homepage);
                    }
                }

                else
                {
                    //this.Invalidate();
                    this.Refresh();
                }

                if(web.statusbar)
					hideStatusTimer.Enabled = true;

                CF_setGraffitiStatusGlobal(false);
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}


        public override void CF_pluginHide()
        {
            CF_setGraffitiStatusGlobal(true);
            base.CF_pluginHide();
        }


		public override DialogResult CF_pluginShowSetup()
		{
            CFTools.writeLog("WEB", "CF_pluginShowSetup", "");
			/*
			 * Return DialogResult.OK for the main application
			 * to update from plugin changes.
			*/
			DialogResult returnvalue = DialogResult.Cancel;

			try
			{
				/*
				 * Creates a new plugin setup instance.
				 * If you create a CFDialog or CFSetup you must
				 * set its MainForm property to the main plugins
				 * MainForm property.
				*/
                setup mysetup = new setup(this.MainForm, this.pluginConfig, this.pluginLang, this);
				returnvalue = mysetup.ShowDialog(this);
                if (returnvalue == DialogResult.OK)
                {
                    LoadSettings();

                    for (int i = 1; i <= numhomepagelinks; i++)
                    {
                        if (File.Exists(web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + i.ToString() + "tmp.jpg"))
                        {
                            File.Delete(web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + i.ToString() + ".jpg");
                            File.Move(web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + i.ToString() + "tmp.jpg", web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + i.ToString() + ".jpg");
                        }
                    }
                }
                else
                {
                    for (int i = 1; i <= numhomepagelinks; i++)
                    {
                        if (File.Exists(web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + i.ToString() + "tmp.jpg"))
                            File.Delete(web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + i.ToString() + "tmp.jpg");

                    }
                }
				mysetup.Close();
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }

			return returnvalue;
		}


		public override string CF_pluginData(string command, string param1)
		{
			string retvalue = "";
			return retvalue;
		}


		public override void CF_pluginCommand(string command, string param1, string param2)
		{
			try
			{
				if(firstshow)
				{
					LoadWeb();
					firstshow = false;
				}

				switch(command.ToUpper())
				{
					case "BROWSE":
						if(this.CF_getConnectionStatus())
						{
							//if(param2 == "FULLSCREEN" && !isfullscreen)
								//minmax();
							
							BrowseTo(param1);
						}
						else if(autodial && !dialing)
						{
							dialing = true;
							dialpage = param1;
                            this.CF_systemCommand(CF_Actions.CONNECT, this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");

							if(this.CF_getConnectionStatus())
							{
								//if(param2 == "FULLSCREEN" && !isfullscreen)
									//minmax();
								
								BrowseTo(param1);
							}
							else
								this.CF_systemDisplayDialog(CF_Dialogs.OkBox, LanguageReader.GetText("APPLANG/DIALOGTEXT/NOINTERNETAVAILABLE"));
						}
						else
							this.CF_systemDisplayDialog(CF_Dialogs.OkBox, LanguageReader.GetText("APPLANG/DIALOGTEXT/NOINTERNETAVAILABLE"));
						break;
				}
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}


        private void web_CF_Event_connectionEstablished(object sender, EventArgs e)
        {
            if (autodial && dialing && this.CF_getConnectionStatus())
                BrowseTo(dialpage);
        }


        private void web_CF_Event_connectionError(object sender, EventArgs e)
        {
            dialpage = "";
            dialing = false;
        }


        public override string CF_pluginDefaultConfig()
        {
            if (!Directory.Exists(web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs"))
                Directory.CreateDirectory(web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs");

            // Create thumbs if doesn't exist
            if (File.Exists(web.startuppath + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + "1.jpg"))
            {
                for (int i = 1; i <= numhomepagelinks; i++)
                {
                    if (!File.Exists(web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + i.ToString() + ".jpg"))
                        File.Copy(web.startuppath + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + i + ".jpg", web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + i.ToString() + ".jpg");
                }
            }

            return "<APPCONFIG>" + Environment.NewLine +
                   "    <SKIN>Aura</SKIN>" + Environment.NewLine +
                   "    <APPLANG>English</APPLANG>" + Environment.NewLine +
                   "    <LOGEVENTS>False</LOGEVENTS>" + Environment.NewLine +
                   "    <HOMEPAGE>True</HOMEPAGE>" + Environment.NewLine +
                   "    <TEXTSIZE>Medium</TEXTSIZE>" + Environment.NewLine +
                   "    <FULLSCREEN>True</FULLSCREEN>" + Environment.NewLine +
                   "    <AUTODIAL>False</AUTODIAL>" + Environment.NewLine +
                   "    <DISABLEPOPUPS>True</DISABLEPOPUPS>" + Environment.NewLine +
                   "    <DISPLAY>1</DISPLAY>" + Environment.NewLine +
                   "    <LASTPAGE>http://www.fluxmedia.net/websearch</LASTPAGE>" + Environment.NewLine +
                   "    <LASTPAGEREAR>http://www.fluxmedia.net/websearch</LASTPAGEREAR>" + Environment.NewLine +
                   "    <HOMEURL></HOMEURL>" + Environment.NewLine +
                   "    <HOMEPAGE1URL>http://www.fluxmedia.net/websearch</HOMEPAGE1URL>" + Environment.NewLine +
                   "    <HOMEPAGE2URL>http://www.centrafuse.com</HOMEPAGE2URL>" + Environment.NewLine +
                   "    <HOMEPAGE3URL>http://www.centrafuse.com/support</HOMEPAGE3URL>" + Environment.NewLine +
                   "    <HOMEPAGE4URL>http://www.centrafuse.com/forums</HOMEPAGE4URL>" + Environment.NewLine +
                   "    <HOMEPAGE5URL>http://www.centrafuse.com/account</HOMEPAGE5URL>" + Environment.NewLine +
                   "    <HOMEPAGE6URL>http://www.centrafuse.com/KB</HOMEPAGE6URL>" + Environment.NewLine +
                   "    <HOMEPAGE7URL>http://www.youtube.com/Centrafuse</HOMEPAGE7URL>" + Environment.NewLine +
                   "    <HOMEPAGE8URL>http://www.twitter.com/Centrafuse</HOMEPAGE8URL>" + Environment.NewLine +
                   "    <HOMEPAGE9URL>http://www.facebook.com/Centrafuse</HOMEPAGE9URL>" + Environment.NewLine +
                   "</APPCONFIG>" + Environment.NewLine;
        }

#endregion

#region Browser/Label Events

        private void webBrowser_GotFocus(object sender, System.EventArgs e)
        {
#if !WindowsCE
            if (webBrowser.Document != null)
            {
                //if (!webBrowser.IsBusy)
                webBrowser.Document.Focus();
            }
#endif
        }


        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                CFTools.writeLog("WEB", "webBrowser_DocumentCompleted: " + e.Url.ToString().ToLower() + ", RS = " + webBrowser.ReadyState.ToString());

                if (webBrowser.ReadyState != WebBrowserReadyState.Complete)
                    return;

                // set the size of the text in web browser
                this.SetTextSize();
                
                if (e.Url.ToString().ToLower() != "about:blank")
                {
                    this.CF_updateButtonText("URL", e.Url.ToString());

                    currenturl = e.Url.ToString();

                    if (!CF_displayHooks.rearScreen)
                        currenturlfront = currenturl;
                    else
                        currenturlrear = currenturl;
                }

                if (this.showingLoading)
                    this.CF_systemCommand(CF_Actions.HIDEINFO, this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");
                this.showingLoading = false;
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }


		private void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
		{
            try
            {
                CFTools.writeLog("WEB", "webBrowser_Navigating: " + e.Url.ToString());

                if (e.Url.ToString() == "http://www.fluxmedia.net/apphome")
                {
                    this.CF_systemCommand(CF_Actions.MAINMENU, this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");
                    return;
                }

                hideTimer.Enabled = true;
                if (this.Visible)
                {
                    if (!showingLoading)
                        this.CF_systemCommand(CF_Actions.SHOWINFO, LanguageReader.GetText("/APPLANG/GENERIC/LOADING"), "DISPLAY", CF_displayHooks.displayNumber.ToString(), this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");
                    showingLoading = true;
                }
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }

            return;
		}


#if !WindowsCE
		private void webBrowser_NewWindow(object sender, System.ComponentModel.CancelEventArgs e)
		{
            try
            {
                CFTools.writeLog("WEB", "webBrowser_NewWindow", "disablepopups = " + disablepopups.ToString() + ", StatusText = " + this.webBrowser.StatusText);
                e.Cancel = true;
                if (disablepopups)
                {
                    if (this.showingLoading)
                        this.CF_systemCommand(CF_Actions.HIDEINFO, this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");
                    this.showingLoading = false;
                }
                else if (capturepage == -1)
                    BrowseTo(this.webBrowser.StatusText);
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }

			return;
		}
#endif


        private void web_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.Visible)
                {
                    if (!web.statusbar)
                    {
                        ChangeStatus(false);
                        hideStatusTimer.Enabled = true;
                    }

#if !WindowsCE
                    Application.RemoveMessageFilter(msgFilter);
                    Application.AddMessageFilter(msgFilter);
#endif
                }
                else
                {
#if !WindowsCE
                    Application.RemoveMessageFilter(msgFilter);
#endif
                }
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }

#endregion

#region System Functions

        //private static System.Threading.Mutex fileMtx = new Mutex() ;
        private static string logfile = web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "web.log";

        public static void DebugToFile(string msg)
        {
            DebugToFile(msg, true); // just call main debug function with 'append' set to true
            return;
        }


        private static void DebugToFile(string msg, bool append)
        {
            if (!logEvents)
                return;
            if (!append)
                CFTools.writeModuleLog("startup", logfile);
            else
                CFTools.writeModuleLog(msg, logfile);
            return;
        }


        private void ChangeStatus(bool hide)
        {
            try
            {
                if (this.webBrowser != null)
                {
                    if (hide)
                    {
                        this.webBrowser.Bounds = new Rectangle(0, 0, this.Bounds.Width, this.Bounds.Height);
                        web.statusbar = false;
                    }
                    else
                    {
                        if (isfullscreen)
                        {
                            if (CFTools.AllScreens.Length >= CF_displayHooks.displayNumber && CF_displayHooks.displayNumber > 1)
                                this.webBrowser.Bounds = this.CF_createRect(SkinReader.ParseBounds(this.pluginSkinReader.GetControlAttribute("Web", "IEContainer", "secbounds")));
                            else
                                this.webBrowser.Bounds = this.CF_createRect(SkinReader.ParseBounds(this.pluginSkinReader.GetControlAttribute("Web", "IEContainer", "fullbounds")));
                        }
                        else
                            this.webBrowser.Bounds = this.CF_createRect(SkinReader.ParseBounds(this.pluginSkinReader.GetControlAttribute("Web", "IEContainer", "bounds")));

                        web.statusbar = true;
                    }
                }
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }


        private void LoadWeb()
        {
            try
            {
                /*
                 * Creates a new web browser object and events.
                */
                //System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(web));
                this.webBrowser = new WebBrowser();
#if !WindowsCE
                this.webBrowser.NewWindow += new System.ComponentModel.CancelEventHandler(this.webBrowser_NewWindow);
#endif
                this.webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(this.webBrowser_DocumentCompleted);
                this.webBrowser.GotFocus += new EventHandler(this.webBrowser_GotFocus);
                this.webBrowser.Navigating += new WebBrowserNavigatingEventHandler(this.webBrowser_Navigating);
                this.webBrowser.ScriptErrorsSuppressed = true;

                this.VisibleChanged += new EventHandler(web_VisibleChanged);

                /*
                 * Creates new event to fire when an internet connection is established.
                */
                this.CF_events.connectionEstablished += new EventHandler(web_CF_Event_connectionEstablished);

                /*
                 * Creates new event to fire when an internet connection error occurs.
                */
                this.CF_events.connectionError += new EventHandler(web_CF_Event_connectionError);

                if (isfullscreen || (CFTools.AllScreens.Length >= CF_displayHooks.displayNumber && CF_displayHooks.displayNumber > 1))
                {
                    isfullscreen = true;

                    if (webBrowser != null)
                    {
                        if (CFTools.AllScreens.Length >= CF_displayHooks.displayNumber && CF_displayHooks.displayNumber > 1)
                            this.webBrowser.Bounds = this.CF_createRect(SkinReader.ParseBounds(this.pluginSkinReader.GetControlAttribute("Web", "IEContainer", "secbounds")));
                        else
                            this.webBrowser.Bounds = this.CF_createRect(SkinReader.ParseBounds(this.pluginSkinReader.GetControlAttribute("Web", "IEContainer", "fullbounds")));
                    }
                }
                else
                {
                    if (webBrowser != null)
                        this.webBrowser.Bounds = this.CF_createRect(SkinReader.ParseBounds(this.pluginSkinReader.GetControlAttribute("Web", "IEContainer", "bounds")));
                }

                this.Controls.Add(this.webBrowser);
                this.ResumeLayout(false);
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }


        public void LoadSettings()
        {
            try
            {
                // Forces current page to reload after settings have changed.
                this.reload = true;

                // The display name is shown in the application to represent the plugin.  This sets the display name from the configuration file.
                this.CF_params.displayName = this.pluginLang.ReadField("/APPLANG/WEB/DISPLAYNAME");
                this.CF_params.settingsDisplayDesc = this.pluginLang.ReadField("/APPLANG/SETUP/DESCRIPTION");

                bool tempfullscreen = isfullscreen;

                // Reads the fullscreen option from the configuration file and sets the web browser to fullscreen if true.
                /*
                if (firstrun)
                {
                    try { isfullscreen = Boolean.Parse(this.pluginConfig.ReadField("/APPCONFIG/FULLSCREEN")); }
                    catch { isfullscreen = false; }
                    this.CF_displayHooks.boundsAttributeId = (isfullscreen ? "fullbounds" : "bounds");
                }
                */
                isfullscreen = true;

                // This sets the auto dial feature from the configuration file.
                try { this.autodial = Boolean.Parse(this.pluginConfig.ReadField("/APPCONFIG/AUTODIAL")); }
                catch { this.autodial = false; }

                // This sets the enable popups feature from the configuration file.
                try { this.disablepopups = Boolean.Parse(this.pluginConfig.ReadField("/APPCONFIG/DISABLEPOPUPS")); }
                catch { this.disablepopups = false; }

                // This sets the autohide feature from the configuration file.
                try
                {
                    this.autohide = Boolean.Parse(this.pluginConfig.ReadField("/APPCONFIG/AUTOHIDE"));
                }
                catch
                {
                    this.pluginConfig.WriteField("/APPCONFIG/AUTOHIDE", "False");
                    this.pluginConfig.Save();
                    this.autohide = false;
                }

                // This sets which monitor to display the plugin on.
                try { this.CF_displayHooks.displayNumber = Int32.Parse(this.pluginConfig.ReadField("/APPCONFIG/DISPLAY")); }
                catch { this.CF_displayHooks.displayNumber = 1; }

                try { logEvents = Boolean.Parse(this.pluginConfig.ReadField("/APPCONFIG/LOGEVENTS")); }
                catch { logEvents = false; }

                try { loadhomepageonstart = Boolean.Parse(this.pluginConfig.ReadField("/APPCONFIG/HOMEPAGE")); }
                catch { loadhomepageonstart = true; }

                try { homeurl.one = this.pluginConfig.ReadField("/APPCONFIG/HOMEPAGE1URL"); }
                catch { homeurl.one = "http://www.fluxmedia.net/websearch"; }
                try { homeurl.two = this.pluginConfig.ReadField("/APPCONFIG/HOMEPAGE2URL"); }
                catch { homeurl.two = "http://www.fluxmedia.net/websearch"; }
                try { homeurl.three = this.pluginConfig.ReadField("/APPCONFIG/HOMEPAGE3URL"); }
                catch { homeurl.three = "http://www.fluxmedia.net/websearch"; }
                try { homeurl.four = this.pluginConfig.ReadField("/APPCONFIG/HOMEPAGE4URL"); }
                catch { homeurl.four = "http://www.fluxmedia.net/websearch"; }
                try { homeurl.five = this.pluginConfig.ReadField("/APPCONFIG/HOMEPAGE5URL"); }
                catch { homeurl.five = "http://www.fluxmedia.net/websearch"; }
                try { homeurl.six = this.pluginConfig.ReadField("/APPCONFIG/HOMEPAGE6URL"); }
                catch { homeurl.six = "http://www.fluxmedia.net/websearch"; }
                try { homeurl.seven = this.pluginConfig.ReadField("/APPCONFIG/HOMEPAGE7URL"); }
                catch { homeurl.seven = "http://www.fluxmedia.net/websearch"; }
                try { homeurl.eight = this.pluginConfig.ReadField("/APPCONFIG/HOMEPAGE8URL"); }
                catch { homeurl.eight = "http://www.fluxmedia.net/websearch"; }
                try { homeurl.nine = this.pluginConfig.ReadField("/APPCONFIG/HOMEPAGE9URL"); }
                catch { homeurl.nine = "http://www.fluxmedia.net/websearch"; }

                BuildHomepage();

                try { currenturlfront = this.pluginConfig.ReadField("/APPCONFIG/LASTPAGE"); }
                catch { currenturlfront = "http://www.fluxmedia.net/websearch"; }

                try { currenturlrear = this.pluginConfig.ReadField("/APPCONFIG/LASTPAGEREAR"); }
                catch { currenturlrear = "http://www.fluxmedia.net/websearch"; }

                if (!loadhomepageonstart)
                    currenturl = (this.CF_displayHooks.rearScreen ? currenturlrear : currenturlfront);
                else
                {
                    currenturl = this.pluginConfig.ReadField("/APPCONFIG/HOMEURL");
                    if (currenturl == null || currenturl == "")
                        currenturl = "file://" + web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "homepage.htm";
                }
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }

            firstrun = false;
        }


        private string getURL(string url)
        {
            try
            {
                if (!url.ToUpper().StartsWith("HTTP://") && !url.ToUpper().StartsWith("HTTPS://") && !url.ToUpper().StartsWith("FTP://") && !url.ToUpper().StartsWith("FILE://"))
                    url = "http://" + url;
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }

            return url;
        }


		private void minmax()
		{
			try
			{
				if(isfullscreen && CFTools.AllScreens.Length >= CF_displayHooks.displayNumber && CF_displayHooks.displayNumber > 1 && !CF_displayHooks.rearScreen)
				{
					this.Visible = false;
					return;
				}

                isfullscreen = !isfullscreen;
                this.CF_displayHooks.boundsAttributeId = (isfullscreen ? "fullbounds" : "bounds");

                string currentURL = CF_getButtonText("URL");

                CF_localskinsetup();

                CF_updateButtonText("URL", currentURL);

                this.Invalidate();
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}


		private void browsersend(string data)
		{
            this.webBrowser.Focus();
			CFTools.CFSendKeys(data);
		}


		private void BrowseTo(string theurl)
		{
            CFTools.writeLog("WEB", "BrowseTo", "url = " + theurl);
			try
			{
				dialpage = "";
				dialing = false;

				this.CF_updateButtonText("URL", theurl);
#if !WindowsCE
                webBrowser.Navigate(theurl);
#else
                webBrowser.Navigate(new Uri(getURL(theurl)));
#endif
				currenturl = theurl;

                if (!CF_displayHooks.rearScreen)
                    currenturlfront = currenturl;
                else
                    currenturlrear = currenturl;
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }

#if !(WindowsCE || Mono)
        public void CapturePageImage(string url, int button)
        {
            CFTools.writeLog("WEB", "CapturePageImage1", "url = " + url + ", button = " + button.ToString());

            try
            {
                if (this.CF_getConnectionStatus())
                {
                    //capturepage = button;
                    capturepage = 0;
                    CapturePageImage2(url, button);
                    //CF_pluginCommand("BROWSE", url, "FULLSCREEN");
                    //Application.DoEvents();
                }
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }


        private void CapturePageImage2(string URL, int button)
        {
            CFTools.writeLog("WEB", "CapturePageImage2", "button = " + button.ToString());

            try
            {
                this.CF_systemCommand(CF_Actions.SHOWINFO, LanguageReader.GetText("/APPLANG/GENERIC/LOADING"), "FORCE", this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");

                Bitmap bmp = ClassWSThumb.GetWebSiteThumbnail(URL, 800, 600, 200, 150);
                bmp.Save(web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + button.ToString() + "tmp.jpg", ImageFormat.Jpeg);

                //**if Jpeg is the image format you need just replace the 2 lines above for:
                //
                //

                //** but don't forget to add the reference
                //   to the class in the page 'Default.aspx'(code behind):
                //using System.Drawing.Imaging;
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }

            this.CF_systemCommand(CF_Actions.HIDEINFO);
        }
#endif


        /*
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
        */

        private void BuildHomepage()
        {
            try
            {
                string homepage = "<html>";
                string onmouseover = " onmouseover='this.style.cursor=\"pointer\";this.style.backgroundColor=\"gray\"'";
                string onmouseout = " onmouseout='this.style.backgroundColor=\"white\"'";

                if (isfullscreen)
                    homepage += "<font style='font-size:12px;font-weight:bold;font-family:Arial,Helvetica,sans-serif'><br></font>";

                homepage += "<table border='0' align='center' width='" + ((int)(this.Bounds.Width - 20)).ToString() + "' cellpadding='10' cellspacing='10' bgcolor='white'><tr>" +
                            "<td align='center' colspan='3' bgcolor='white' align='center'><font style='font-size:22px;font-weight:bold;font-family:Arial,Helvetica,sans-serif'>" + LanguageReader.GetText("/APPLANG/GPS/FAVORITES") + "</font></td></tr><tr>" +
                            "<td align='center' valign='top' width='33%'" + onmouseover + onmouseout + " bgcolor='white' onclick='window.location=\"" + homeurl.one + "\"'>" +
                            "<img src='" + web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + "1.jpg' border='1' style='border-color:#A8A8A8' width='190'><font style='font-size:9px;font-weight:bold;font-family:Arial,Helvetica,sans-serif'><br><br>" +
                            homeurl.one + "</font></td>" +
                            "<td align='center' valign='top' width='33%'" + onmouseover + onmouseout + " bgcolor='white' onclick='window.location=\"" + homeurl.two + "\"'>" +
                            "<img src='" + web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + "2.jpg' border='1' style='border-color:#A8A8A8' width='190'><font style='font-size:9px;font-weight:bold;font-family:Arial,Helvetica,sans-serif'><br><br>" +
                            homeurl.two + "</font></td>" +
                            "<td align='center' valign='top' width='33%'" + onmouseover + onmouseout + " bgcolor='white' onclick='window.location=\"" + homeurl.three + "\"'>" +
                            "<img src='" + web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + "3.jpg' border='1' style='border-color:#A8A8A8' width='190'><font style='font-size:9px;font-weight:bold;font-family:Arial,Helvetica,sans-serif'><br><br>" +
                            homeurl.three + "</font></td>" +
                            "</tr><tr>" +
                            "<td align='center' valign='top' width='33%'" + onmouseover + onmouseout + " bgcolor='white' onclick='window.location=\"" + homeurl.four + "\"'>" +
                            "<img src='" + web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + "4.jpg' border='1' style='border-color:#A8A8A8' width='190'><font style='font-size:9px;font-weight:bold;font-family:Arial,Helvetica,sans-serif'><br><br>" +
                            homeurl.four + "</font></td>" +
                            "<td align='center' valign='top' width='33%'" + onmouseover + onmouseout + " bgcolor='white' onclick='window.location=\"" + homeurl.five + "\"'>" +
                            "<img src='" + web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + "5.jpg' border='1' style='border-color:#A8A8A8' width='190'><font style='font-size:9px;font-weight:bold;font-family:Arial,Helvetica,sans-serif'><br><br>" +
                            homeurl.five + "</font></td>" +
                            "<td align='center' valign='top' width='33%'" + onmouseover + onmouseout + " bgcolor='white' onclick='window.location=\"" + homeurl.six + "\"'>" +
                            "<img src='" + web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + "6.jpg' border='1' style='border-color:#A8A8A8' width='190'><font style='font-size:9px;font-weight:bold;font-family:Arial,Helvetica,sans-serif'><br><br>" +
                            homeurl.six + "</font></td>" +
                            "</tr><tr>" +
                            "<td align='center' valign='top' width='33%'" + onmouseover + onmouseout + " bgcolor='white' onclick='window.location=\"" + homeurl.seven + "\"'>" +
                            "<img src='" + web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + "7.jpg' border='1' style='border-color:#A8A8A8' width='190'><font style='font-size:9px;font-weight:bold;font-family:Arial,Helvetica,sans-serif'><br><br>" +
                            homeurl.seven + "</font></td>" +
                            "<td align='center' valign='top' width='33%'" + onmouseover + onmouseout + " bgcolor='white' onclick='window.location=\"" + homeurl.eight + "\"'>" +
                            "<img src='" + web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + "8.jpg' border='1' style='border-color:#A8A8A8' width='190'><font style='font-size:9px;font-weight:bold;font-family:Arial,Helvetica,sans-serif'><br><br>" +
                            homeurl.eight + "</font></td>" +
                            "<td align='center' valign='top' width='33%'" + onmouseover + onmouseout + " bgcolor='white' onclick='window.location=\"" + homeurl.nine + "\"'>" +
                            "<img src='" + web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "Thumbs" + Path.DirectorySeparatorChar + "9.jpg' border='1' style='border-color:#A8A8A8' width='190'><font style='font-size:9px;font-weight:bold;font-family:Arial,Helvetica,sans-serif'><br><br>" +
                            homeurl.nine + "</font></td>" +
                            "</tr></table>";

                homepage += "</html>";

#if !WindowsCE
                File.WriteAllText(web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "homepage.htm", homepage, Encoding.Unicode);
#else
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "homepage.htm"))
                {
                    writer.Write(homepage);
                }
#endif
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }

		
        private void SetTextSize()
		{
			try
			{
#if !WindowsCE
				switch(this.pluginConfig.ReadField("/APPCONFIG/TEXTSIZE").Trim().ToLower())
				{
					case "smallest":
                        webBrowser.Document.Body.Style = webBrowser.Document.Body.Style += ";font-size:12px;";
						break;
					case "smaller":
                        webBrowser.Document.Body.Style = webBrowser.Document.Body.Style += ";font-size:14px;";
						break;
					case "medium":
                        webBrowser.Document.Body.Style = webBrowser.Document.Body.Style += ";font-size:16px;";
						break;
					case "larger":
                        webBrowser.Document.Body.Style = webBrowser.Document.Body.Style += ";font-size:20px;";
						break;
					case "largest":
                        webBrowser.Document.Body.Style = webBrowser.Document.Body.Style += ";font-size:26px;";
						break;
				}
#endif
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

#endregion

#region Button Click Events

        /*
		 * Navigate button click event.
		*/
        private void navBtn_Click(object sender, MouseEventArgs e)
        {
            try
            {
#if !WindowsCE
                Application.RemoveMessageFilter(msgFilter);
#endif
                bool oldstatus = hideStatusTimer.Enabled;
                hideStatusTimer.Enabled = false;

                /*
                 * Launches system OSK dialog, retrieves the results,
                 * and navigates to the specified web page.
                */
                string resultvalue, resulttext;
                if (this.CF_systemDisplayDialog(CF_Dialogs.OSK, this.pluginLang.ReadField("/APPLANG/WEB/ENTERURL"), out resultvalue, out resulttext) == DialogResult.OK)
                {
                    if (this.CF_getConnectionStatus())
                    {
                        BrowseTo(resultvalue);
                    }
                    else if (autodial && !dialing)
                    {
                        dialing = true;
                        dialpage = resultvalue;
                        this.CF_systemCommand(CF_Actions.CONNECT, this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");

                        if (this.CF_getConnectionStatus())
                            BrowseTo(resultvalue);
                        else
                            this.CF_systemDisplayDialog(CF_Dialogs.OkBox, LanguageReader.GetText("APPLANG/DIALOGTEXT/NOINTERNETAVAILABLE"));
                    }
                    else
                        this.CF_systemDisplayDialog(CF_Dialogs.OkBox, LanguageReader.GetText("APPLANG/DIALOGTEXT/NOINTERNETAVAILABLE"));
                }

                hideStatusTimer.Enabled = oldstatus;
#if !WindowsCE
                Application.AddMessageFilter(msgFilter);
#endif
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }


        private void unload_Click(object sender, MouseEventArgs e)
        {
            try
            {
#if !WindowsCE
                Application.RemoveMessageFilter(msgFilter);
#endif
                bool oldstatus = hideStatusTimer.Enabled;
                hideStatusTimer.Enabled = false;

                BrowseTo("about:blank");

                hideStatusTimer.Enabled = oldstatus;
#if !WindowsCE
                Application.AddMessageFilter(msgFilter);
#endif
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }


		/*
		 * Navigates the web browser back.
		*/
		private void backBtn_Click(object sender, MouseEventArgs e)
		{
			bool oldstatus = hideStatusTimer.Enabled;
			hideStatusTimer.Enabled = false;

            try { webBrowser.GoBack(); }
			catch {}

			hideStatusTimer.Enabled = oldstatus;
		}

		/*
		 * Navigates the web browser forward.
		*/
		private void forwardBtn_Click(object sender, MouseEventArgs e)
		{
			bool oldstatus = hideStatusTimer.Enabled;
			hideStatusTimer.Enabled = false;

            try { webBrowser.GoForward(); }
			catch {}

			hideStatusTimer.Enabled = oldstatus;
		}

		/*
		 * Navigates the web browser home.
		*/
		private void homeBtn_Click(object sender, MouseEventArgs e)
		{
			try
			{
				bool oldstatus = hideStatusTimer.Enabled;
				hideStatusTimer.Enabled = false;

                string homepage = this.pluginConfig.ReadField("/APPCONFIG/HOMEURL");
                if (homepage == null || homepage == "")
                    homepage = "file://" + web.appdatapath + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar + "Web" + Path.DirectorySeparatorChar + "homepage.htm";
				if(this.CF_getConnectionStatus())
				{
					BrowseTo(homepage);
				}
				else if(autodial && !dialing)
				{
					dialing = true;
					dialpage = homepage;
                    this.CF_systemCommand(CF_Actions.CONNECT, this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");

					if(this.CF_getConnectionStatus())
						BrowseTo(homepage);
					else
						this.CF_systemDisplayDialog(CF_Dialogs.OkBox, LanguageReader.GetText("APPLANG/DIALOGTEXT/NOINTERNETAVAILABLE"));
				}
				else
					this.CF_systemDisplayDialog(CF_Dialogs.OkBox, LanguageReader.GetText("APPLANG/DIALOGTEXT/NOINTERNETAVAILABLE"));

				hideStatusTimer.Enabled = oldstatus;
			}
			catch {}
		}

		/*
		 * Stops the loading of the current page.
		*/
		private void stopBtn_Click(object sender, MouseEventArgs e)
		{
			bool oldstatus = hideStatusTimer.Enabled;
			hideStatusTimer.Enabled = false;

            try { webBrowser.Stop(); }
			catch {}

            if (this.showingLoading)
                this.CF_systemCommand(CF_Actions.HIDEINFO, this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");
            this.showingLoading = false;
			hideStatusTimer.Enabled = oldstatus;
		}

		/*
		 * Launches favorites dialog.
		*/
		private void favBtn_Click(object sender, MouseEventArgs e)
		{
			try
			{
#if !WindowsCE
				Application.RemoveMessageFilter(msgFilter);
#endif
				bool oldstatus = hideStatusTimer.Enabled;
				hideStatusTimer.Enabled = false;

				/*
				 * Creates a new instance of the favorites dialog,
				 * retrieves the URL, and navigates to the
				 * specified page.
				 * 
				 * If you create a CFDialog or CFSetup you must
				 * set its MainForm property to the main plugins
				 * MainForm property.
				*/
                string currurl = "";
                if(webBrowser.Url != null)
                    currurl = webBrowser.Url.ToString();

				favorites myfavs = new favorites(currurl, this.CF_displayHooks.displayNumber, this.CF_displayHooks.rearScreen);
				myfavs.MainForm = this.MainForm;

				/*
				 * Calling pluginInit manually is a good idea if you check
				 * any system variables or call any CF functions inside
				 * your init method.  This is because the MainForm has to be
				 * set before these functions will work.
				*/
				myfavs.CF_pluginInit();

				if(myfavs.ShowDialog() == DialogResult.OK)
				{
					if(myfavs.resultData != "" && this.CF_getConnectionStatus())
					{
						BrowseTo(myfavs.resultData);
					}
					else if(myfavs.resultData != "" && autodial && !dialing)
					{
						dialing = true;
						dialpage = myfavs.resultData;
                        this.CF_systemCommand(CF_Actions.CONNECT, this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");

						if(this.CF_getConnectionStatus())
							BrowseTo(myfavs.resultData);
						else
							this.CF_systemDisplayDialog(CF_Dialogs.OkBox, LanguageReader.GetText("APPLANG/DIALOGTEXT/NOINTERNETAVAILABLE"));
					}
					else
						this.CF_systemDisplayDialog(CF_Dialogs.OkBox, LanguageReader.GetText("APPLANG/DIALOGTEXT/NOINTERNETAVAILABLE"));
				}

				myfavs.Close();
				hideStatusTimer.Enabled = oldstatus;
#if !WindowsCE
				Application.AddMessageFilter(msgFilter);
#endif
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

		/*
		 * Reloads the current page.
		*/
		private void reloadBtn_Click(object sender, MouseEventArgs e)
		{
			try
			{
				bool oldstatus = hideStatusTimer.Enabled;
				hideStatusTimer.Enabled = false;

				if(currenturl != "" && this.CF_getConnectionStatus())
				{
					BrowseTo(currenturl);
				}
				else if(currenturl != "" && autodial && !dialing)
				{
					dialing = true;
					dialpage = currenturl;
                    this.CF_systemCommand(CF_Actions.CONNECT, this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");

					if(this.CF_getConnectionStatus())
						BrowseTo(currenturl);
					else
						this.CF_systemDisplayDialog(CF_Dialogs.OkBox, LanguageReader.GetText("APPLANG/DIALOGTEXT/NOINTERNETAVAILABLE"));
				}
				else
					this.CF_systemDisplayDialog(CF_Dialogs.OkBox, LanguageReader.GetText("APPLANG/DIALOGTEXT/NOINTERNETAVAILABLE"));

				hideStatusTimer.Enabled = oldstatus;
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

		/*
		 * Launches system OSK to enter text into web browser.
		*/
		private void keyboardBtn_Click(object sender, MouseEventArgs e)
		{
			try
			{
#if !WindowsCE
				Application.RemoveMessageFilter(msgFilter);
#endif
				bool oldstatus = hideStatusTimer.Enabled;
				hideStatusTimer.Enabled = false;

				string resultvalue, resulttext;
				if(this.CF_systemDisplayDialog(CF_Dialogs.OSK, this.pluginLang.ReadField("/APPLANG/WEB/ENTERTEXT"), out resultvalue, out resulttext) == DialogResult.OK)
					browsersend(resultvalue);

				hideStatusTimer.Enabled = oldstatus;
#if !WindowsCE
				Application.AddMessageFilter(msgFilter);
#endif
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

		/*
		 * Launches system OSK to enter text into web browser.
		*/
		private void minmaxBtn_Click(object sender, MouseEventArgs e)
		{
			bool oldstatus = hideStatusTimer.Enabled;
			hideStatusTimer.Enabled = false;

			minmax();
			hideStatusTimer.Enabled = oldstatus;
		}

		/*
		 * Launches system OSK to enter text into web browser.
		*/
		private void pageupBtn_Click(object sender, MouseEventArgs e)
		{
			try
			{
				bool oldstatus = hideStatusTimer.Enabled;
				hideStatusTimer.Enabled = false;
				browsersend("{PGUP}");
				hideStatusTimer.Enabled = oldstatus;
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

		/*
		 * Launches system OSK to enter text into web browser.
		*/
		private void pagedownBtn_Click(object sender, MouseEventArgs e)
		{
			try
			{
				bool oldstatus = hideStatusTimer.Enabled;
				hideStatusTimer.Enabled = false;
				browsersend("{PGDN}");
				hideStatusTimer.Enabled = oldstatus;
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

#endregion

#region Timer Events

        private void backTimer_Tick(object sender, EventArgs e)
		{
			if(web.goback)
			{
				web.goback = false;

                try { webBrowser.GoBack(); }
				catch {}
			}
		}


		private void hideTimer_Tick(object sender, EventArgs e)
		{
			hideTimer.Enabled = false;
            if (this.showingLoading)
                this.CF_systemCommand(CF_Actions.HIDEINFO, this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");
            this.showingLoading = false;
		}


		private void hideStatusTimer_Tick(object sender, EventArgs e)
		{
			if(this.autohide)
			{
				ChangeStatus(true);
				hideStatusTimer.Enabled = false;
			}
        }

#endregion

    }
}