using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using JsonFlatFileDataStore;
using MacMon.Models;
using MacMon.Network;
using MacMon.Services.WebSocket;
using Microsoft.Win32;
using Phoenix;

namespace MacMon.Jobs
{
    public class UserActivitiesMonitor
    {
        public const string LogOut = "LOG_OUT";
        public const string LogIn = "LOG_IN";
        public const string Lock = "LOCK";
        public const string Unlock = "UNLOCK";
        public const string POWER_OFF = "POWER_OFF";
        
        private readonly DataStore _store;
        private readonly Channel _channel;

        public UserActivitiesMonitor(DataStore store, Channel channel)
        {
            _store = store;
            _channel = channel;
        }

        public void OnStart()
        {
            EventLog.WriteEntry("SimpleService", "Starting SimpleService");
            new Thread(RunMessagePump).Start();
        }

        public void OnStop()
        {
            EventLog.WriteEntry("SimpleService.MessagePump", "Starting SimpleService Message Pump");
            Application.Run(new HiddenForm(_store, _channel));
        }
        
        void RunMessagePump()
        {
            EventLog.WriteEntry("SimpleService.MessagePump", "Starting SimpleService Message Pump");
            Application.Run(new HiddenForm(_store, _channel));
        }
    }
    
    public partial class HiddenForm : Form
    {
        private readonly DataStore _store;
        private readonly Channel _channel;
        
        public HiddenForm(DataStore store, Channel channel)
        {
            _store = store;
            _channel = channel;
            InitializeComponent();
        }

        private void Save(Dictionary<string, object> data)
        {
            if (Connection.IsAvailable())
            {
                var activity = new UserActivity
                {
                    Action = (string)data["action"],
                    User = (string) data["user"],
                    Date = (long) data["date"]
                };
                Locally(activity);
            }
            else
            {
                Server(data);
            }
        }

        private void Locally(UserActivity activity)
        {
            _store.Reload();
        }

        private void Server(Dictionary<string, object> data)
        {
            if (_channel.canPush)
            {
                _channel.Push(MacMonWebSocket.USER_ACTIVITY_CHANGED, data);
            }
        }

        private void HiddenForm_Load(object sender, EventArgs e)
        {
            SystemEvents.SessionSwitch += SessionSwitched;
        }

        private void HiddenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SystemEvents.SessionSwitch -= SessionSwitched;
        }

        private void SessionSwitched(object sender, SessionSwitchEventArgs e)
        {
            var data = new Dictionary<string, object> {{"date", ""}, {"user", Env.GetUsername()}};
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    data.Add("action", UserActivitiesMonitor.Lock);
                    break;
                case SessionSwitchReason.SessionLogoff:
                    data.Add("action", UserActivitiesMonitor.LogOut);
                    break;
                case SessionSwitchReason.SessionLogon:
                    data.Add("action", UserActivitiesMonitor.LogIn);
                    break;
                case SessionSwitchReason.SessionUnlock:
                    data.Add("action", UserActivitiesMonitor.Unlock);
                    break;
                default:
                    EventLog.WriteEntry("User Activity", e.Reason.ToString());
                    break;
            }
            
            Save(data);
        }
    }

    partial class HiddenForm
    {
        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(0, 0);
            FormBorderStyle = FormBorderStyle.None;
            Name = "HiddenForm";
            Text = "HiddenForm";
            WindowState = FormWindowState.Minimized;
            Load += HiddenForm_Load;
            FormClosing += HiddenForm_FormClosing;
            ResumeLayout(false);
        }
    }
}