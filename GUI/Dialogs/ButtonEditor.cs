﻿using Newtonsoft.Json;
using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.Events;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.GUI.CustomControls.ButtonEditor;
using SuchByte.MacroDeck.GUI.Dialogs;
using SuchByte.MacroDeck.GUI.MainWindowContents;
using SuchByte.MacroDeck.Icons;
using SuchByte.MacroDeck.Interfaces;
using SuchByte.MacroDeck.Plugins;
using SuchByte.MacroDeck.Profiles;
using SuchByte.MacroDeck.Server;
using SuchByte.MacroDeck.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace SuchByte.MacroDeck.GUI
{
    public partial class ButtonEditor : DialogForm
    {
        private ActionButton.ActionButton actionButton;
        private ActionButton.ActionButton actionButtonEdited;
        private readonly Folders.MacroDeckFolder folder;
        private EventSelector eventSelector;
        private ActionSelectorOnPress actionSelectorOnPress;

        public ActionButton.ActionButton ActionButton { get { return this.actionButton; } }

        public ButtonEditor(ActionButton.ActionButton actionButton, Folders.MacroDeckFolder folder)
        {
            InitializeComponent();
            this.groupAppearance.Text = Language.LanguageManager.Strings.Appearance;
            this.groupButtonState.Text = Language.LanguageManager.Strings.ButtonState;
            this.lblButtonState.Text = Language.LanguageManager.Strings.ButtonState;
            this.radioButtonOff.Text = Language.LanguageManager.Strings.Off;
            this.radioButtonOn.Text = Language.LanguageManager.Strings.On;
            this.labelAlignTop.Text = Language.LanguageManager.Strings.Top;
            this.labelAlignCenter.Text = Language.LanguageManager.Strings.Center;
            this.labelAlignBottom.Text = Language.LanguageManager.Strings.Bottom;
            this.lblPath.Text = Language.LanguageManager.Strings.Path;
            this.lblCurrentStateLabel.Text = Language.LanguageManager.Strings.CurrentState;
            this.lblStateBinding.Text = Language.LanguageManager.Strings.StateBinding;
            this.radioOnPress.Text = Language.LanguageManager.Strings.OnPress;
            this.radioOnEvent.Text = Language.LanguageManager.Strings.OnEvent;
            this.btnApply.Text = Language.LanguageManager.Strings.Save;
            this.btnOk.Text = Language.LanguageManager.Strings.Ok;

            this.folder = folder;
            this.actionButton = actionButton;
            
            using (InstalledFontCollection col = new InstalledFontCollection())
            {
                foreach (FontFamily fontFamily in col.Families)
                {
                    fonts.Items.Add(fontFamily.Name);
                }
            }
            this.listStateBinding.Items.Add("");
            foreach (Variables.Variable variable in Variables.VariableManager.Variables)
            {
                this.listStateBinding.Items.Add(variable.Name);
            }
            this.actionButton.StateChanged += this.OnStateChanged;
            this.LoadButton();
        }

        private void ButtonEditor_Load(object sender, EventArgs e)
        {
            this.btnPreview.Radius = ProfileManager.CurrentProfile.ButtonRadius;

        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            if (this == null || this.IsDisposed || this.Disposing) return;
            try
            {
                this.Invoke(new Action(() =>
                {
                
                        bool newState = ((ActionButton.ActionButton)sender).State;
                        this.lblCurrentState.Text = newState ? "On" : "Off";
                        this.radioButtonOff.Checked = !newState;
                        this.radioButtonOn.Checked = newState;
                }));
            }
            catch { }

        }

        private void UpdateLabel()
        {
            try
            {
                if (radioButtonOff.Checked && !radioButtonOn.Checked)
                {
                    this.actionButtonEdited.LabelOff.LabelText = this.labelText.Text;
                    this.actionButtonEdited.LabelOff.Size = (float)this.fontSize.Value;
                    this.actionButtonEdited.LabelOff.FontFamily = this.fonts.Text;
                    if (this.labelAlignTop.Checked)
                    {
                        this.actionButtonEdited.LabelOff.LabelPosition = ButtonLabelPosition.TOP;
                    }
                    else if (this.labelAlignCenter.Checked)
                    {
                        this.actionButtonEdited.LabelOff.LabelPosition = ButtonLabelPosition.CENTER;
                    }
                    else if (this.labelAlignBottom.Checked)
                    {
                        this.actionButtonEdited.LabelOff.LabelPosition = ButtonLabelPosition.BOTTOM;
                    }
                    Task.Run(() =>
                    {
                        Bitmap labelBitmap = new Bitmap(250, 250);
                        string labelOffText = actionButtonEdited.LabelOff.LabelText.ToString();
                        labelOffText = VariableManager.RenderTemplate(labelOffText);
                        /*foreach (Variables.Variable variable in Variables.VariableManager.Variables)
                        {
                            if (labelOffText.ToLower().Contains("{" + variable.Name.ToLower() + "}"))
                            {
                                labelOffText = labelOffText.Replace("{" + variable.Name + "}", variable.Value.ToString(), StringComparison.OrdinalIgnoreCase);
                            }
                        }*/
                        labelBitmap = Utils.LabelGenerator.GetLabel(labelBitmap, labelOffText, this.actionButtonEdited.LabelOff.LabelPosition, new Font(this.actionButtonEdited.LabelOff.FontFamily, this.actionButtonEdited.LabelOff.Size), this.actionButtonEdited.LabelOff.LabelColor, Color.Black, new SizeF(2.0F, 2.0F));
                        this.actionButtonEdited.LabelOff.LabelBase64 = Utils.Base64.GetBase64FromBitmap(labelBitmap);
                        this.Invoke(new Action(() => {
                            if (this != null && this.Disposing == false && this.IsDisposed == false)
                            {
                                this.btnPreview.ForegroundImage = labelBitmap;
                            }
                            }));
                        });

                }
                else if(!radioButtonOff.Checked && radioButtonOn.Checked)
                {
                    this.actionButtonEdited.LabelOn.LabelText = this.labelText.Text;
                    this.actionButtonEdited.LabelOn.Size = (float)this.fontSize.Value;
                    this.actionButtonEdited.LabelOn.FontFamily = this.fonts.Text;
                    if (this.labelAlignTop.Checked)
                    {
                        this.actionButtonEdited.LabelOn.LabelPosition = ButtonLabelPosition.TOP;
                    }
                    else if (this.labelAlignCenter.Checked)
                    {
                        this.actionButtonEdited.LabelOn.LabelPosition = ButtonLabelPosition.CENTER;
                    }
                    else if (this.labelAlignBottom.Checked)
                    {
                        this.actionButtonEdited.LabelOn.LabelPosition = ButtonLabelPosition.BOTTOM;
                    }
                    Task.Run(() =>
                    {
                        Bitmap labelBitmap = new Bitmap(250, 250);
                        string labelOnText = actionButtonEdited.LabelOn.LabelText.ToString();
                        labelOnText = VariableManager.RenderTemplate(labelOnText);

                        /*foreach (Variables.Variable variable in Variables.VariableManager.Variables)
                        {
                            if (labelOnText.ToLower().Contains("{" + variable.Name.ToLower() + "}"))
                            {
                                labelOnText = labelOnText.Replace("{" + variable.Name + "}", variable.Value.ToString(), StringComparison.OrdinalIgnoreCase);
                            }
                        }*/
                        labelBitmap = Utils.LabelGenerator.GetLabel(labelBitmap, labelOnText, this.actionButtonEdited.LabelOn.LabelPosition, new Font(this.actionButtonEdited.LabelOn.FontFamily, this.actionButtonEdited.LabelOn.Size), this.actionButtonEdited.LabelOn.LabelColor, Color.Black, new SizeF(2.0F, 2.0F));
                        this.actionButtonEdited.LabelOn.LabelBase64 = Utils.Base64.GetBase64FromBitmap(labelBitmap);
                        this.Invoke(new Action(() => {
                            if (this != null && this.Disposing == false && this.IsDisposed == false)
                            {
                                this.btnPreview.ForegroundImage = labelBitmap;
                            }
                        }));
                });
                }
            } catch { }
        }

        private void RefreshLabel()
        {
            this.labelText.TextChanged -= this.LabelChanged;
            this.fontSize.ValueChanged -= this.LabelChanged;
            this.labelAlignTop.CheckedChanged -= this.LabelChanged;
            this.labelAlignCenter.CheckedChanged -= this.LabelChanged;
            this.labelAlignBottom.CheckedChanged -= this.LabelChanged;
            this.fonts.SelectedIndexChanged -= this.LabelChanged;
            
            try
            {
                if (radioButtonOff.Checked && !radioButtonOn.Checked)
                {
                    this.labelText.Text = this.actionButtonEdited.LabelOff.LabelText;
                    this.fontSize.Value = (int)this.actionButtonEdited.LabelOff.Size;
                    this.fonts.Text = this.actionButtonEdited.LabelOff.FontFamily;
                    
                    if (this.actionButtonEdited.LabelOff.LabelPosition == SuchByte.MacroDeck.ActionButton.ButtonLabelPosition.TOP)
                    {
                        this.labelAlignTop.Checked = true;
                    }
                    else if (this.actionButtonEdited.LabelOff.LabelPosition == SuchByte.MacroDeck.ActionButton.ButtonLabelPosition.CENTER)
                    {
                        this.labelAlignCenter.Checked = true;
                    }
                    else
                    {
                        this.labelAlignBottom.Checked = true;
                    }
                }
                else
                {
                    this.labelText.Text = this.actionButtonEdited.LabelOn.LabelText;
                    this.fontSize.Value = (int)this.actionButtonEdited.LabelOn.Size;
                    this.fonts.Text = this.actionButtonEdited.LabelOn.FontFamily;

                    if (this.actionButtonEdited.LabelOn.LabelPosition == SuchByte.MacroDeck.ActionButton.ButtonLabelPosition.TOP)
                    {
                        this.labelAlignTop.Checked = true;
                    }
                    else if (this.actionButtonEdited.LabelOn.LabelPosition == SuchByte.MacroDeck.ActionButton.ButtonLabelPosition.CENTER)
                    {
                        this.labelAlignCenter.Checked = true;
                    }
                    else
                    {
                        this.labelAlignBottom.Checked = true;
                    }
                }
                if (this.labelText.Text.Length == 0)
                {
                    this.labelText.PlaceHolderText = Language.LanguageManager.Strings.Label;
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            this.labelText.TextChanged += this.LabelChanged;
            this.fontSize.ValueChanged += this.LabelChanged;
            this.labelAlignTop.CheckedChanged += this.LabelChanged;
            this.labelAlignCenter.CheckedChanged += this.LabelChanged;
            this.labelAlignBottom.CheckedChanged += this.LabelChanged;
            this.fonts.SelectedIndexChanged += this.LabelChanged;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public void RefreshIcon()
        {
            try
            {
                if (radioButtonOff.Checked && !radioButtonOn.Checked)
                {
                    if (this.actionButtonEdited.IconOff != null && this.actionButtonEdited.IconOff.Split(".").Length > 1)
                    {
                        Icons.IconPack iconPack = IconManager.GetIconPackByName(this.actionButtonEdited.IconOff.Split(".")[0]);
                        Icons.Icon icon = IconManager.GetIcon(iconPack, long.Parse(this.actionButtonEdited.IconOff.Split(".")[1]));
                        if (icon != null)
                            this.btnPreview.BackgroundImage = Utils.Base64.GetImageFromBase64(icon.IconBase64);
                    } else
                    {
                        this.btnPreview.BackgroundImage = null;
                    }

                    if (this.actionButtonEdited.LabelOff != null && this.actionButtonEdited.LabelOff.LabelBase64.Length > 0)
                    {
                        Image label = Utils.Base64.GetImageFromBase64(this.actionButtonEdited.LabelOff.LabelBase64);
                        if (label != null)
                            this.btnPreview.ForegroundImage = label;
                    }
                    else
                    {
                        this.btnPreview.ForegroundImage = null;
                    }
                }
                else if(!radioButtonOff.Checked && radioButtonOn.Checked)
                {
                    if (this.actionButtonEdited.IconOn != null && this.actionButtonEdited.IconOn.Split(".").Length > 1)
                    {
                        Icons.IconPack iconPack = IconManager.GetIconPackByName(this.actionButtonEdited.IconOn.Split(".")[0]);
                        Icons.Icon icon = IconManager.GetIcon(iconPack, long.Parse(this.actionButtonEdited.IconOn.Split(".")[1]));
                        if (icon != null)
                            this.btnPreview.BackgroundImage = Utils.Base64.GetImageFromBase64(icon.IconBase64);
                    }
                    else
                    {
                        this.btnPreview.BackgroundImage = null;
                    }

                    if (this.actionButtonEdited.LabelOn != null && this.actionButtonEdited.LabelOn.LabelBase64.Length > 0)
                    {
                        Image label = Utils.Base64.GetImageFromBase64(this.actionButtonEdited.LabelOn.LabelBase64);
                        if (label != null)
                            this.btnPreview.ForegroundImage = label;
                    }
                    else
                    {
                        this.btnPreview.ForegroundImage = null;
                    }
                }
                this.btnPreview.ShowGIFIndicator = this.btnPreview.BackgroundImage != null && this.btnPreview.BackgroundImage.RawFormat.ToString().ToLower() == "gif";
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void Apply()
        {
            Debug.WriteLine("Apply");
            this.actionButton = this.actionButtonEdited;
            this.actionButton.EventListeners = new List<EventListener>();

            foreach (PluginAction pluginAction in this.actionButton.Actions)
            {
                pluginAction.SetActionButton(this.actionButton);
            }

            foreach (EventItem eventItem in this.eventSelector.EventItems())
            {
                if (eventItem == null) continue;
                foreach (PluginAction pluginAction in eventItem.EventListener.Actions)
                {
                    pluginAction.SetActionButton(this.actionButton);
                }
                this.actionButton.EventListeners.Add(eventItem.EventListener);
            }

            foreach (ActionButton.ActionButton actionButton in this.folder.ActionButtons.FindAll(actionButton => actionButton.Position_Y == this.actionButton.Position_Y && actionButton.Position_X == this.actionButton.Position_X).ToArray())
            {
                //ProfileManager.RemoveEventHandler(actionButton);
                this.folder.ActionButtons.Remove(actionButton);
            }
            this.folder.ActionButtons.Add(this.actionButton);
            //ProfileManager.AddEventHandler(this.actionButton);
            ProfileManager.Save();
            MacroDeckServer.UpdateFolder(this.folder);
            ProfileManager.UpdateVariableLabels(this.actionButton);
            this.actionButton.UpdateBindingState();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            this.Apply();
        }


        private void BtnOk_Click(object sender, EventArgs e)
        {
            this.Apply();
            this.Close();
        }

        private void BtnPreview_Click(object sender, EventArgs e)
        {
            this.OpenIconSelector();
        }

        private void OpenIconSelector()
        {
            using (var iconSelector = new IconSelector())
            {
                if (iconSelector.ShowDialog() == DialogResult.OK)
                {
                    if (iconSelector.SelectedIconPack != null && iconSelector.SelectedIcon != null)
                    {
                        if (radioButtonOff.Checked && !radioButtonOn.Checked)
                        {
                            this.actionButtonEdited.IconOff = iconSelector.SelectedIconPack.Name + "." + iconSelector.SelectedIcon.IconId;
                        }
                        else
                        {
                            this.actionButtonEdited.IconOn = iconSelector.SelectedIconPack.Name + "." + iconSelector.SelectedIcon.IconId;
                        }
                        this.RefreshIcon();
                    }
                }
            }
        }

        public void LoadButton()
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                Error = (sender, args) => { args.ErrorContext.Handled = true; }
            };

            this.actionButtonEdited = JsonConvert.DeserializeObject<ActionButton.ActionButton>(JsonConvert.SerializeObject(this.actionButton, jsonSerializerSettings), jsonSerializerSettings); // Make a copy of the current action button

            this.buttonPath.Text = this.folder.DisplayName + "\\" + (this.actionButton.Position_Y + 1) + "." + (this.actionButton.Position_X + 1);
            this.btnPreview.BackgroundImageLayout = ImageLayout.Stretch;

            bool currentState = this.actionButton.State;
            this.lblCurrentState.Text = currentState ? "On" : "Off";
            this.listStateBinding.Text = this.actionButtonEdited.StateBindingVariable;

            this.RefreshLabel();
            this.RefreshIcon();
            this.actionSelectorOnPress = new ActionSelectorOnPress(this.actionButtonEdited);
            this.eventSelector = new EventSelector(this.actionButtonEdited);
            this.actionSelectorOnPress.RefreshActions();
            this.eventSelector.RefreshEventsList();
            this.SetSelector(actionSelectorOnPress);
        }

        

        private void LabelChanged(object sender, EventArgs e)
        {
            this.UpdateLabel();
        }


       
        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.RefreshLabel();
            this.RefreshIcon();
        }

        private void BtnEditIcon_Click(object sender, EventArgs e)
        {
            this.OpenIconSelector();
        }

        private void BtnRemoveIcon_Click(object sender, EventArgs e)
        {
            if (radioButtonOff.Checked && !radioButtonOn.Checked)
            {
                this.actionButtonEdited.IconOff = null;
            }
            else
            {
                this.actionButtonEdited.IconOn = null;
            }
            this.RefreshIcon();
        }

        private void BtnClearLabelText_Click(object sender, EventArgs e)
        {
            this.labelText.Clear();
        }

        private void BtnForeColor_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    if (radioButtonOff.Checked && !radioButtonOn.Checked)
                    {
                        this.actionButtonEdited.LabelOff.LabelColor = colorDialog.Color;
                    }
                    else
                    {
                        this.actionButtonEdited.LabelOn.LabelColor = colorDialog.Color;
                    }
                    this.UpdateLabel();
                }
            }
            
        }

        private void BtnAddVariable_Click(object sender, EventArgs e)
        {
            this.variablesContextMenu.Items.Clear();
            foreach (Variables.Variable variable in Variables.VariableManager.Variables)
            {
                ToolStripMenuItem item = new ToolStripMenuItem
                {
                    ForeColor = Color.White,
                    Text = variable.Name,
                };
                item.Click += AddVariableContextMenuItemClick;
                this.variablesContextMenu.Items.Add(item);
            }
            this.variablesContextMenu.Show(PointToScreen(new Point(((PictureButton)sender).Bounds.Left, ((PictureButton)sender).Bounds.Bottom)));
        }

        private void AddVariableContextMenuItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            var selectionIndex = this.labelText.SelectionStart;
            this.labelText.Text = this.labelText.Text.Insert(selectionIndex, "{" + item.Text + "}");
            this.labelText.SelectionStart = selectionIndex + ("{" + item.Text + "}").Length;
        }

        private void ListStateBinding_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.actionButtonEdited.StateBindingVariable = this.listStateBinding.Text;
        }

        private void BtnDeleteStateBinding_Click(object sender, EventArgs e)
        {
            this.listStateBinding.Text = "";
        }

        private void RadioOnPress_CheckedChanged(object sender, EventArgs e)
        {
            if (radioOnPress.Checked)
            {
                this.SetSelector(actionSelectorOnPress);
            }
        }

        private void RadioOnEvent_CheckedChanged(object sender, EventArgs e)
        {
            if (radioOnEvent.Checked)
            {
                this.SetSelector(eventSelector);
            }
        }

        private void SetSelector(Control control)
        {
            this.selectorPanel.Controls.Clear();
            this.selectorPanel.Controls.Add(control);
        }

        private void labelText_Load(object sender, EventArgs e)
        {

        }

        private void BtnOpenTemplateEditor_Click(object sender, EventArgs e)
        {
            using (var templateEditor = new TemplateEditor(this.labelText.Text))
            {
                if (templateEditor.ShowDialog() == DialogResult.OK)
                {
                    this.labelText.Text = templateEditor.Template;
                }
            }
        }
    }
}
