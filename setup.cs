using System;
using System.Xml;
using System.Windows.Forms;
using centrafuse.Plugins;

namespace web
{
	/*
	 * Setup class inherits from CFSetup
	 * so that it will not show up as a seperate
	 * plugin, but a dialog within a plugin.
	 * It uses the standard setup screens from the
	 * main application
	*/
	public class setup : CFSetup
    {

#region Variables

        private web Web = null;
        private int currpage = 1;

#endregion

#region Construction

        public setup(ICFMain mForm, ConfigReader config, LanguageReader lang, web webInst)
        {
            this.MainForm = mForm;
            this.Web = webInst;

            this.pluginConfig = config;
            this.pluginLang = lang;

            CF_initSetup(3, 3);

            this.CF_updateText("TITLE", this.pluginLang.ReadField("/APPLANG/SETUP/HEADER"));
        }

#endregion

#region CFSetup

        public override void CF_setupReadSettings(int currentpage, bool advanced)
        {
            try 
            {
                int i = CFSetupButton.One;
                currpage = currentpage;

                //if (CF_showAdvancedSettings) 
                //{
                    /*******************************************************************************************/
                    /*****  ADVANCED SETTINGS - PAGE 1  ********************************************************/
                    /*******************************************************************************************/
                    if (currentpage == 1) 
                    {
                        // TEXT BUTTONS (1-4)
                        ButtonHandler[i] = new CFSetupHandler(SetDisplayName);
                        ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/LABEL1");
                        ButtonValue[i++] = pluginLang.ReadField("/APPLANG/WEB/DISPLAYNAME");

                        ButtonHandler[i] = new CFSetupHandler(SetTextSize); ;
                        ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/LABEL3");
                        ButtonValue[i++] = this.pluginLang.ReadField("/APPLANG/SETUP/" + pluginConfig.ReadField("/APPCONFIG/TEXTSIZE").ToUpper());
                        
                        ButtonHandler[i] = new CFSetupHandler(SetDisplay); ;
                        ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/WEBDISPLAY");
                        ButtonValue[i++] = LanguageReader.GetText("APPLANG/SETUP/DISPLAY") + pluginConfig.ReadField("/APPCONFIG/DISPLAY");

                        ButtonHandler[i] = new CFSetupHandler(SetHomePageButtonURL);
                        ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/HOMEPAGE") + " 1";
                        ButtonValue[i++] = pluginConfig.ReadField("/APPCONFIG/HOMEPAGE1URL");

                        // BOOL BUTTONS (5-8)

                        //ButtonHandler[i] = new CFSetupHandler(SetFullScreen);
                        //ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/LABEL4");
                        //ButtonValue[i++] = pluginConfig.ReadField("/APPCONFIG/FULLSCREEN");
                        
                        ButtonHandler[i] = new CFSetupHandler(SetAutoDial);
                        ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/LABEL5");
                        ButtonValue[i++] = pluginConfig.ReadField("/APPCONFIG/AUTODIAL");
                        
                        ButtonHandler[i] = new CFSetupHandler(SetDisablePopups);
                        ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/LABEL6");
                        ButtonValue[i++] = pluginConfig.ReadField("/APPCONFIG/DISABLEPOPUPS");
                        
                        ButtonHandler[i] = new CFSetupHandler(SetAutoHide);
                        ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/AUTOHIDE");
                        ButtonValue[i++] = pluginConfig.ReadField("/APPCONFIG/AUTOHIDE");

                        ButtonHandler[i] = new CFSetupHandler(SetHomePage);
                        ButtonText[i] = pluginLang.ReadField("/APPLANG/FAVORITES/LOAD") + " " + pluginLang.ReadField("/APPLANG/SETUP/LABEL2");
                        ButtonValue[i++] = pluginConfig.ReadField("/APPCONFIG/HOMEPAGE");
                    }
                    else if (currentpage == 2)
                    {
                        // TEXT BUTTONS (1-4)
                        ButtonHandler[i] = new CFSetupHandler(SetHomePageButtonURL);
                        ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/HOMEPAGE") + " 2";
                        ButtonValue[i++] = pluginConfig.ReadField("/APPCONFIG/HOMEPAGE2URL");

                        ButtonHandler[i] = new CFSetupHandler(SetHomePageButtonURL); ;
                        ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/HOMEPAGE") + " 3";
                        ButtonValue[i++] = pluginConfig.ReadField("/APPCONFIG/HOMEPAGE3URL");

                        ButtonHandler[i] = new CFSetupHandler(SetHomePageButtonURL);
                        ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/HOMEPAGE") + " 4";
                        ButtonValue[i++] = pluginConfig.ReadField("/APPCONFIG/HOMEPAGE4URL");

                        ButtonHandler[i] = new CFSetupHandler(SetHomePageButtonURL); ;
                        ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/HOMEPAGE") + " 5";
                        ButtonValue[i++] = pluginConfig.ReadField("/APPCONFIG/HOMEPAGE5URL");

                        // BOOL BUTTONS (5-8)

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";
                    }
                    else if (currentpage == 3)
                    {
                        // TEXT BUTTONS (1-4)

                        ButtonHandler[i] = new CFSetupHandler(SetHomePageButtonURL);
                        ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/HOMEPAGE") + " 6";
                        ButtonValue[i++] = pluginConfig.ReadField("/APPCONFIG/HOMEPAGE6URL");

                        ButtonHandler[i] = new CFSetupHandler(SetHomePageButtonURL); ;
                        ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/HOMEPAGE") + " 7";
                        ButtonValue[i++] = pluginConfig.ReadField("/APPCONFIG/HOMEPAGE7URL");

                        ButtonHandler[i] = new CFSetupHandler(SetHomePageButtonURL);
                        ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/HOMEPAGE") + " 8";
                        ButtonValue[i++] = pluginConfig.ReadField("/APPCONFIG/HOMEPAGE8URL");

                        ButtonHandler[i] = new CFSetupHandler(SetHomePageButtonURL); ;
                        ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/HOMEPAGE") + " 9";
                        ButtonValue[i++] = pluginConfig.ReadField("/APPCONFIG/HOMEPAGE9URL");

                        // BOOL BUTTONS (5-8)

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";
                    }
                //}
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }

#endregion

#region Button Clicks

		private void SetDisplayName(ref object value)
		{
			try {
				/*
				 * Launches system OSK dialog, retrieves the results,
				 * and stores them in the configuration XML.
				*/
				object tempobject;
				string resultvalue, resulttext;
                if (this.CF_systemDisplayDialog(CF_Dialogs.OSK, this.pluginLang.ReadField("/APPLANG/SETUP/BUTTON1TEXT"), ButtonValue[(int)value], null, out resultvalue, out resulttext, out tempobject, null, true, true, true, true, false, false, 1) == DialogResult.OK)
				{
					this.pluginLang.WriteField("/APPLANG/WEB/DISPLAYNAME", resultvalue);
                    ButtonValue[(int)value] = resultvalue;
                }
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}


		private void SetHomePageButtonURL(ref object value)
        {
            try
            {
                int pval = (int)value + 2; // page 2
                if (currpage == 1)
                    pval = 1; // page 1
                else if (currpage == 3)
                    pval = pval + 4; // page 3

                /*
                 * Launches system OSK dialog, retrieves the results,
                 * and stores them in the configuration XML.
                */
                object tempobject;
                string resultvalue, resulttext;
                if (this.CF_systemDisplayDialog(CF_Dialogs.OSK, LanguageReader.GetText("/APPLANG/BUTTONS/ENTER") + " " + this.pluginLang.ReadField("/APPLANG/SETUP/HOMEPAGEBTN" + pval.ToString() + "URL"), ButtonValue[(int)value], null, out resultvalue, out resulttext, out tempobject, null, true, true, true, true, false, false, 1) == DialogResult.OK)
                {
#if !(WindowsCE || Mono)
                    if (this.CF_getConnectionStatus())
                        this.Web.CapturePageImage(resultvalue, pval);
#endif
                    this.pluginConfig.WriteField("/APPCONFIG/HOMEPAGE" + pval.ToString() + "URL", resultvalue);
                    ButtonValue[(int)value] = resultvalue;
                }
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }


		private void SetTextSize(ref object value)
		{
			try {
				/*
				 * Launches system FileBrowser dialog, retrieves the results,
				 * and stores them in the configuration XML.  This uses a custom
				 * FileBrowser dialog.  It accepts an array of CFListViewItem
				 * objects.  This allows you to display a dialog with options
				 * for the user to choose from.
				*/
				CFControls.CFListViewItem[] textoptions = new CFControls.CFListViewItem[5];
				textoptions[0] = new CFControls.CFListViewItem(this.pluginLang.ReadField("/APPLANG/SETUP/LARGEST"), "Largest", -1, false);
				textoptions[1] = new CFControls.CFListViewItem(this.pluginLang.ReadField("/APPLANG/SETUP/LARGER"), "Larger", -1, false);
				textoptions[2] = new CFControls.CFListViewItem(this.pluginLang.ReadField("/APPLANG/SETUP/MEDIUM"), "Medium", -1, false);
				textoptions[3] = new CFControls.CFListViewItem(this.pluginLang.ReadField("/APPLANG/SETUP/SMALLER"), "Smaller", -1, false);
				textoptions[4] = new CFControls.CFListViewItem(this.pluginLang.ReadField("/APPLANG/SETUP/SMALLEST"), "Smallest", -1, false);

				object tempobject;
				string resultvalue, resulttext;
                if (this.CF_systemDisplayDialog(CF_Dialogs.FileBrowser, this.pluginLang.ReadField("/APPLANG/SETUP/BUTTON3TEXT"), this.pluginLang.ReadField("/APPLANG/SETUP/TEXTSIZES"), ButtonValue[(int)value], out resultvalue, out resulttext, out tempobject, textoptions, true, true, true, true, false, false, 1) == DialogResult.OK)
				{
                    this.pluginConfig.WriteField("/APPCONFIG/TEXTSIZE", resultvalue);
                    ButtonValue[(int)value] = resultvalue;
                }
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}


		private void SetDisplay(ref object value)
		{
			try {
				CFControls.CFListViewItem[] playlist = new CFControls.CFListViewItem[CFTools.AllScreens.Length];

				for(int i=0;i<CFTools.AllScreens.Length;i++)
				{
					playlist[i] = new CFControls.CFListViewItem(LanguageReader.GetText("APPLANG/SETUP/DISPLAY") + (i+1), (i+1).ToString(), -1, false);
				}

				object tempobject;
				string resultvalue, resulttext;
                if (this.CF_systemDisplayDialog(CF_Dialogs.FileBrowser, LanguageReader.GetText("APPLANG/SETUP/SELECTDISPLAYTEXT"), LanguageReader.GetText("APPLANG/SETUP/DISPLAYS"), ButtonValue[(int)value], out resultvalue, out resulttext, out tempobject, playlist, true, true, true, false, false, false, 1) == DialogResult.OK)
				{
                    this.pluginConfig.WriteField("/APPCONFIG/DISPLAY", resultvalue);
                    ButtonValue[(int)value] = LanguageReader.GetText("APPLANG/SETUP/DISPLAY") + resultvalue;
                }
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}


		private void SetFullScreen(ref object value)
		{
            this.pluginConfig.WriteField("/APPCONFIG/FULLSCREEN", value.ToString());
		}


		private void SetAutoDial(ref object value)
		{
            this.pluginConfig.WriteField("/APPCONFIG/AUTODIAL", value.ToString());
		}


		private void SetDisablePopups(ref object value)
		{
            this.pluginConfig.WriteField("/APPCONFIG/DISABLEPOPUPS", value.ToString());
		}


		private void SetAutoHide(ref object value)
		{
            this.pluginConfig.WriteField("/APPCONFIG/AUTOHIDE", value.ToString());
		}


        private void SetHomePage(ref object value)
        {
            this.pluginConfig.WriteField("/APPCONFIG/HOMEPAGE", value.ToString());
        }

#endregion

	}
}