using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using JsonFlatFileDataStore;
using MacMon.Database;
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

        public const string Action = "action";
        public const string Timestamp = "timestamp";
        public const string User = "user";
        
        private readonly DataStore _store;
        private readonly Channel _channel;

        public UserActivitiesMonitor(DataStore store, Channel channel)
        {
            _store = store;
            _channel = channel;
        }

        public void OnStart()
        {
            new Thread(RunMessagePump).Start();
        }

        public void OnStop()
        {
            Application.Exit();
        }
        
        void RunMessagePump()
        {
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
                    Action = (string)data[UserActivitiesMonitor.Action],
                    User = (string) data[UserActivitiesMonitor.User],
                    Timestamp = (long) data[UserActivitiesMonitor.Timestamp]
                };
                Locally(activity);
            }
            else
            {
                Server(data);
            }
        }

        private async void Locally(UserActivity activity)
        {
            var activities = _store.GetCollection<UserActivity>(Store.UserActivitiesKey);
            await activities.InsertOneAsync(activity);
        }

        private void Server(Dictionary<string, object> data)
        {
            if (_channel.canPush)
            {
                _channel.Push(MacMonWebSocket.UserActivityChanged, data);
            }
            else
            {
                var activity = new UserActivity
                {
                    Action = (string)data[UserActivitiesMonitor.Action],
                    User = (string) data[UserActivitiesMonitor.User],
                    Timestamp = (long) data[UserActivitiesMonitor.Timestamp]
                };
                Locally(activity);
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
            var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var data = new Dictionary<string, object>
            {
                {UserActivitiesMonitor.Timestamp, timeStamp}, {UserActivitiesMonitor.User, Env.GetUsername()}
            };
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    data.Add(UserActivitiesMonitor.Action, UserActivitiesMonitor.Lock);
                    break;
                case SessionSwitchReason.SessionLogoff:
                    data.Add(UserActivitiesMonitor.Action, UserActivitiesMonitor.LogOut);
                    break;
                case SessionSwitchReason.SessionLogon:
                    data.Add(UserActivitiesMonitor.Action, UserActivitiesMonitor.LogIn);
                    break;
                case SessionSwitchReason.SessionUnlock:
                    data.Add(UserActivitiesMonitor.Action, UserActivitiesMonitor.Unlock);
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
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            Load += HiddenForm_Load;
            FormClosing += HiddenForm_FormClosing;
            ResumeLayout(false);
        }
    }
}