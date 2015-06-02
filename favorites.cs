using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Text;
using System.IO;
using centrafuse.Plugins;

namespace web
{
	/*
	 * Favorites class inherits from CFDialog
	 * so that it will not show up as a seperate
	 * plugin, but a dialog within a plugin.  Main
	 * CF_Event's will not fire automatically.  They
	 * will have to be manually fired from the parent
	 * CFPlugin, if needed.
	*/
	public class favorites : CFDialog
	{
		
        private bool atroot = false;
		private string currenturl = "";
		private string currentpath = "";
        private string favoritespath = "";
		private CFControls.CFListView favoriteslist;
		
		
        public favorites(string currurl, int display, bool rearscreen)
		{
			/*
			 * Sets global variable with current URL.
			*/
			currenturl = currurl;

            this.CF_displayHooks.displayNumber = display;
            this.CF_displayHooks.rearScreen = rearscreen;

			/*
			 * The plugin name should have a matching section in the
			 * skin.xml file.
			*/

#if !WindowsCE
            this.favoritespath = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);
            if(string.IsNullOrEmpty(this.favoritespath)) throw new NotImplementedException("No favorites path");
#else
            StringBuilder resultPath = new StringBuilder(255);
            Win32.SHGetSpecialFolderPath((IntPtr)0, resultPath, Win32.CSIDL_FAVORITES, 0);
            this.favoritespath = resultPath.ToString().Trim();
#endif
		}

		
        public string resultData
		{
			get
			{
				try
				{
					CFControls.CFListViewItem temp = favoriteslist.SelectedItem;
				
					if(temp != null)
						return temp.Value;
					else
						return "";
				}
				catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }

				return "";
			}
		}

#region CFPlugin methods

		public override void CF_pluginInit()
		{
			try
			{
				this.VisibleChanged += new EventHandler(favorites_VisibleChanged);

                this.CF3_initPlugin("Web", true);

				this.CF_localskinsetup();

				loadFavorites(favoritespath);
				favoriteslist.SelectedIndex = 0;
				updateListCount();
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

		
        public override void CF_localskinsetup()
		{
			try
			{
                this.CF3_initDialog("Favorites");

                // reference our listview
                this.favoriteslist = this.listviewArray[0];
                this.favoriteslist.SelectedIndexChanged += new EventHandler(favoriteslist_SelectedIndexChanged);
                this.favoriteslist.ListKeyDown += new KeyEventHandler(favoriteslist_ListKeyDown);
                this.favoriteslist.SingleClick += new EventHandler(favoriteslist_Click);

				this.CF_createButtonEvents("PAGEUP", new MouseEventHandler(pageup_Click), new MouseEventHandler(pageup_Down));
                this.CF_createButtonEvents("PAGEDOWN", new MouseEventHandler(pagedown_Click), new MouseEventHandler(pagedown_Down));
				this.CF_createButtonClick("BACK", new MouseEventHandler(back_Click));
                this.CF_createButtonClick("FORWARD", new MouseEventHandler(forward_Click));
                this.CF_createButtonClick("NEWFOLDER", new MouseEventHandler(newfolderBtn_Click));
				this.CF_createButtonClick("REMOVE", new MouseEventHandler(removeBtn_Click));
                this.CF_createButtonClick("ADDFAVORITE", new MouseEventHandler(addfavBtn_Click));
				this.CF_createButtonClick("LOAD", new MouseEventHandler(load_Click));
				this.CF_createButtonClick("CLOSE", new MouseEventHandler(close_Click));

				this.CF_updateText("TITLE", this.pluginLang.ReadField("/APPLANG/FAVORITES/HEADER"));
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

#endregion

#region System Functions

		private void loadFavorites(string path)
		{
			try
			{
				if(Directory.Exists(path))
				{
					currentpath = path;
					atroot = false;
					
					/*
					 * Updates the labels text with current location.
					*/
                    this.CF_updateText("LISTHEADER", currentpath.Replace(favoritespath, "Favorites"));

					/*
					 * Clears all items from the list box.
					*/
					favoriteslist.ClearList();

					IComparer icomp = null;
					string[] dirs = null;
				
					dirs = Directory.GetDirectories(path);
					Array.Sort(dirs, icomp);
				
					for(int i=0;i<dirs.Length;i++)
					{
						// ARB 11/10/10 - fix hard-coded path separator behavior that breaks Mono
						string[] temp = CFTools.PathSplit(dirs[i]);

						/*
						 * Adds an item to the list box.  The first parameter is
						 * what is displayed, the second parameter is the value of
						 * the item, the third parameter is the index of the image
						 * array (Set to -1 if you do not want to use an image), and
						 * the fourth parameter is whether or not the item is a directory.
						*/
						favoriteslist.Items.Add(new CFControls.CFListViewItem(temp[temp.Length - 1], dirs[i], 0, true));
					}
								
					string[] files = null;
					files = Directory.GetFiles(path, "*.url");

					for(int j=0;j<files.Length;j++)
					{
						// ARB 11/10/10 - fix hard-coded path separator behavior that breaks Mono
						string[] temp = CFTools.PathSplit(files[j]);
						string fname = temp[temp.Length - 1];
                        string url = CFTools.GetPrivateProfileString("InternetShortcut", "URL", "", files[j]);

                        favoriteslist.Items.Add(new CFControls.CFListViewItem(fname.Replace(".url", ""), url, -1, false, files[j]));
					}

                    if (path.ToUpper() == favoritespath.ToUpper())
						atroot = true;

					favoriteslist.Refresh();
					updateListCount();
				}
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

#endregion

#region ListView/System Events

		private void favoriteslist_Click(object sender, EventArgs e)
		{
			try
			{
				if(favoriteslist.Items.Count > 0)
				{
					/*
					 * Always use AbsoluteItem when getting the selected item in the Click or Single Click event.
					 * This will return the selected item always, where as in single click mode SelectedItem potentially
					 * returns the previous item.
					*/
					CFControls.CFListViewItem temp = favoriteslist.AbsoluteItem;
					
					if(temp != null && temp.IsDir)
					{
						loadFavorites(temp.Value);
						favoriteslist.SelectedIndex = 0;
						favoriteslist.Refresh();
					}
					else
						selectBtn_Click(true);

					updateListCount();
				}
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

		
        private void updateListCount()
		{
			try
			{
				if(this.favoriteslist.Items.Count > 0)
					this.CF_updateText("LISTCOUNT", (this.favoriteslist.SelectedIndex + 1) + "/" + this.favoriteslist.Items.Count);
				else
					this.CF_updateText("LISTCOUNT", "0/0");
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

		
        private void favoriteslist_SelectedIndexChanged(object sender, EventArgs e)
		{
			updateListCount();
		}

		
        private void favorites_VisibleChanged(object sender, EventArgs e)
		{
			if(this.Visible)
			{
				this.Visible = true;
				this.favoriteslist.Focus();
			}
		}

		
        private void favoriteslist_ListKeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Left)
				this.backBtn_Click();
			else if(e.KeyCode == Keys.Right)
				this.forwardBtn_Click();
		}

#endregion

#region Button Clicks

		
        private void pageup_Down(object sender, MouseEventArgs e)
		{
			favoriteslist.startPaging(CFControls.CFListView.PagingDirection.UP);
		}

		
        private void pageup_Click(object sender, MouseEventArgs e)
		{
			favoriteslist.stopPaging();
		}

		
        private void pagedown_Down(object sender, MouseEventArgs e)
		{
			favoriteslist.startPaging(CFControls.CFListView.PagingDirection.DOWN);
		}

		
        private void pagedown_Click(object sender, MouseEventArgs e)
		{
			favoriteslist.stopPaging();
		}

		
        private void load_Click(object sender, MouseEventArgs e)
		{
			this.selectBtn_Click(false);
		}

		
        private void close_Click(object sender, MouseEventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		
        private void forward_Click(object sender, MouseEventArgs e)
		{
			forwardBtn_Click();
		}

		
        private void back_Click(object sender, MouseEventArgs e)
		{
			backBtn_Click();
		}

		
        private void newfolderBtn_Click(object sender, MouseEventArgs e)
		{
			try
			{
				/*
				 * Launches system OSK, retrieves results, and creates new
				 * web favorites folder.
				*/
				string resultvalue, resulttext;
				if(this.CF_systemDisplayDialog(CF_Dialogs.OSK, this.pluginLang.ReadField("/APPLANG/FAVORITES/CREATENEWFOLDER"), "", out resultvalue, out resulttext) == DialogResult.OK)
				{
					resultvalue = resultvalue.Replace("\\", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");

					if(!Directory.Exists(Path.Combine(currentpath, resultvalue)))
					{
						Directory.CreateDirectory(Path.Combine(currentpath, resultvalue));
						loadFavorites(currentpath);
					}
				}
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

		
        private void addfavBtn_Click(object sender, MouseEventArgs e)
		{
			try
			{
				string favtext = currenturl.Replace("http://", "").Replace("\\", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");

				string resultvalue, resulttext;
				if(this.CF_systemDisplayDialog(CF_Dialogs.OSK, this.pluginLang.ReadField("/APPLANG/FAVORITES/SAVE"), favtext, out resultvalue, out resulttext) == DialogResult.OK)
				{
					FileStream sb = null;
					resultvalue = resultvalue.Replace("\\", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");

					if(File.Exists(Path.Combine(currentpath, resultvalue + ".url")))
					{
						if(this.CF_systemDisplayDialog(CF_Dialogs.YesNo, this.pluginLang.ReadField("/APPLANG/FAVORITES/OVERWRITE")) == DialogResult.OK)
						{
							sb = new FileStream(Path.Combine(currentpath, resultvalue + ".url"), FileMode.Create);
						}
					}
					else
					{
						sb = new FileStream(Path.Combine(currentpath, resultvalue + ".url"), FileMode.CreateNew);
					}

					if(sb != null)
					{
						string urlcontent = "[DEFAULT]" + Environment.NewLine;
						urlcontent += "BASEURL=" + currenturl + Environment.NewLine;
						urlcontent += "[InternetShortcut]" + Environment.NewLine;
						urlcontent += "URL=" + currenturl + Environment.NewLine;

						StreamWriter sw = new StreamWriter(sb, System.Text.Encoding.Unicode);
						sw.Write(urlcontent);
						sw.Close();
						sb.Close();

						loadFavorites(currentpath);

						for(int i=0;i<favoriteslist.Items.Count;i++)
						{
							if(((CFControls.CFListViewItem)favoriteslist.Items[i]).Text.ToUpper().Trim() == resultvalue.ToUpper().Trim())
							{
								favoriteslist.SelectedIndex = i;
								break;
							}
						}
					}
				}
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

		
        private void removeBtn_Click(object sender, MouseEventArgs e)
		{
			try
			{
				CFControls.CFListViewItem temp = favoriteslist.SelectedItem;

				if(temp != null && (favoriteslist.SelectedIndex != -1 && favoriteslist.Items.Count > 0))
				{
					if(temp.IsDir)
					{
						if(this.CF_systemDisplayDialog(CF_Dialogs.YesNo, this.pluginLang.ReadField("/APPLANG/FAVORITES/REMOVEDIR")) == DialogResult.OK)
						{
							Directory.Delete(temp.Value, true);
							favoriteslist.Items.RemoveAt(favoriteslist.SelectedIndex);

							if(favoriteslist.SelectedIndex > (favoriteslist.Items.Count - 1))
							{
								if((favoriteslist.SelectedIndex - 1) > 0)
									favoriteslist.SelectedIndex = favoriteslist.SelectedIndex - 1;
								else
									favoriteslist.SelectedIndex = 0;
							}

							favoriteslist.Refresh();
							updateListCount();
						}
					}
					else					
					{
						if(this.CF_systemDisplayDialog(CF_Dialogs.YesNo, this.pluginLang.ReadField("/APPLANG/FAVORITES/REMOVEFAV")) == DialogResult.OK)
						{
							File.Delete((string)temp.qResult);
							favoriteslist.Items.RemoveAt(favoriteslist.SelectedIndex);

							if(favoriteslist.SelectedIndex > (favoriteslist.Items.Count - 1))
							{
								if((favoriteslist.SelectedIndex - 1) > 0)
									favoriteslist.SelectedIndex = favoriteslist.SelectedIndex - 1;
								else
									favoriteslist.SelectedIndex = 0;
							}

							favoriteslist.Refresh();
							updateListCount();
						}
					}
				}
				else
					this.CF_systemDisplayDialog(CF_Dialogs.OkBox, this.pluginLang.ReadField("/APPLANG/FAVORITES/NOSELECT"));
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}


		private void selectBtn_Click(bool listclick)
		{
			try
			{
				CFControls.CFListViewItem temp = favoriteslist.SelectedItem;

				if(temp != null)
					this.DialogResult = DialogResult.OK;
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}


		private void forwardBtn_Click()
		{
			try
			{
				CFControls.CFListViewItem temp = favoriteslist.SelectedItem;
			
				if(temp != null && temp.IsDir)
				{
					loadFavorites(temp.Value);
					favoriteslist.SelectedIndex = 0;
					favoriteslist.Refresh();
					updateListCount();
				}
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}


		private void backBtn_Click()
		{
			try
			{
				/*
				 * This should be added if using a list control with multiple levels.
				 * This function is for single click mode and removes one from the array
				 * of list item history.  It does nothing when single click is disabled.
				*/
				favoriteslist.goBack();

				if(!atroot)
				{
					string oldsel = currentpath;
					// ARB 11/10/10 - fix hard-coded path separator behavior that breaks Mono
					string[] tarray = CFTools.PathSplit(currentpath);
					string path = "";

					for(int a=0;a<tarray.Length - 1;a++)
					{
						path += tarray[a] + Path.DirectorySeparatorChar;
					}

					if(tarray.Length > 2)
						path = path.Substring(0,path.Length - 1);
				
					loadFavorites(path);
			
					ArrayList ztemp = favoriteslist.Items;
					int oldindex = 0;

					for(int i=0;i<ztemp.Count;i++)
					{
						if(((CFControls.CFListViewItem)ztemp[i]).Value == oldsel)
							oldindex = i;
					}

					favoriteslist.SelectedIndex = oldindex;
				}
				else if(atroot)
				{
					bool gotit = false;
                    loadFavorites(favoritespath);

					for(int i=0;i<favoriteslist.Items.Count;i++)
					{
						if(((CFControls.CFListViewItem)favoriteslist.Items[i]).Text.ToUpper() == currentpath.ToUpper())
						{
							gotit = true;
							favoriteslist.SelectedIndex = i;
							break;
						}
						else if(((CFControls.CFListViewItem)favoriteslist.Items[i]).Value.ToUpper() == currentpath.ToUpper())
						{
							gotit = true;
							favoriteslist.SelectedIndex = i;
							break;
						}
					}

                    if (!gotit)
                    {
                        this.DialogResult = DialogResult.Cancel;
                        return;
                    }
				}

				favoriteslist.Refresh();
				updateListCount();
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

#endregion

	}
}