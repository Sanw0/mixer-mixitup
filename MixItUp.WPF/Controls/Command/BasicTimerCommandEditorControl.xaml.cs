﻿using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for BasicTimerCommandEditorControl.xaml
    /// </summary>
    public partial class BasicTimerCommandEditorControl : CommandEditorControlBase
    {
        private CommandWindow window;

        private BasicCommandTypeEnum commandType;
        private TimerCommand command;

        private ActionControlBase actionControl;

        public BasicTimerCommandEditorControl(CommandWindow window, TimerCommand command)
            : this(window, BasicCommandTypeEnum.None)
        {
            this.command = command;
        }

        public BasicTimerCommandEditorControl(CommandWindow window, BasicCommandTypeEnum commandType)
        {
            this.window = window;
            this.commandType = commandType;

            InitializeComponent();
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        protected override async Task OnLoaded()
        {
            if (this.command != null)
            {
                this.NameTextBox.Text = this.command.Name;
                if (this.command.Actions.First() is ChatAction)
                {
                    this.actionControl = new ChatActionControl(null, (ChatAction)this.command.Actions.First());
                }
                else if (this.command.Actions.First() is SoundAction)
                {
                    this.actionControl = new SoundActionControl(null, (SoundAction)this.command.Actions.First());
                }
            }
            else
            {
                if (this.commandType == BasicCommandTypeEnum.Chat)
                {
                    this.actionControl = new ChatActionControl(null);
                }
                else if (this.commandType == BasicCommandTypeEnum.Sound)
                {
                    this.actionControl = new SoundActionControl(null);
                }
            }

            this.ActionControlControl.Content = this.actionControl;

            await base.OnLoaded();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await this.window.RunAsyncOperation(async () =>
            {
                if (string.IsNullOrEmpty(this.NameTextBox.Text))
                {
                    await MessageBoxHelper.ShowMessageDialog("Name is missing");
                    return;
                }

                ActionBase action = this.actionControl.GetAction();
                if (action == null)
                {
                    if (this.actionControl is ChatActionControl)
                    {
                        await MessageBoxHelper.ShowMessageDialog("The chat message must not be empty");
                    }
                    else if (this.actionControl is SoundActionControl)
                    {
                        await MessageBoxHelper.ShowMessageDialog("The sound file path must not be empty");
                    }
                    return;
                }

                TimerCommand newCommand = new TimerCommand(this.NameTextBox.Text);
                newCommand.IsBasic = true;
                newCommand.Actions.Add(action);

                if (this.command != null)
                {
                    ChannelSession.Settings.TimerCommands.Remove(this.command);
                    newCommand.ID = this.command.ID;
                }
                ChannelSession.Settings.TimerCommands.Add(newCommand);
                this.CommandSavedSuccessfully(newCommand);

                await ChannelSession.SaveSettings();

                this.window.Close();
            });
        }
    }
}
