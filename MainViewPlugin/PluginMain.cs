﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Documents;
using Mcv.PluginV2;
using Mcv.PluginV2.Messages;
namespace Mcv.MainViewPlugin
{
    [Export(typeof(IPlugin))]
    public class MainViewPlugin : IPlugin
    {
        public PluginId Id { get; } = new PluginId(new Guid("A53CCC44-6A14-4533-99CA-184D5584E257"));
        public string Name => "MainViewPlugin";
        public List<string> Roles { get; } = new List<string> { "mainview" };
        public IPluginHost Host { get; set; } = default!;
        private IAdapter _adapter;
        private MainViewModel _vm;
        private readonly DynamicOptionsTest _options;
        private IMainViewPluginOptions LoadOptions()
        {
            var loadedOptions = Host.RequestMessage(new RequestLoadPluginOptions(Name)) as ReplyPluginOptions;

            var options = new DynamicOptionsTest();
            options.Deserialize(loadedOptions?.RawOptions);
            return options;
        }
        private MainWindow _v;

        public void SetMessage(INotifyMessageV2 message)
        {
            if (_adapter == null)
            {
                return;
            }
            switch (message)
            {
                case NotifyConnectionAdded connAdded:
                    _adapter.OnConnectionAdded(connAdded.ConnSt);
                    break;
                case NotifyConnectionRemoved connRemoved:
                    _adapter.OnConnectionRemoved(connRemoved.ConnId);
                    break;
                case NotifyPluginInfoList pluginInfoList:
                    {
                        foreach (var pluginInfo in pluginInfoList.Plugins)
                        {
                            //_adapter.OnPluginAdded(pluginInfo);
                            OnPluginAdded(pluginInfo);
                        }
                    }
                    break;
                case NotifyConnectionStatusChanged connStChanged:
                    _adapter.OnConnectionStatusChanged(connStChanged.ConnStDiff);
                    break;
                //case NotifySiteAdded siteAdded:
                //    _adapter.OnSiteAdded(siteAdded.SiteId, siteAdded.SiteDisplayName);
                //    break;
                //case NotifyBrowserAdded browserAdded:
                //    _adapter.AddBrowserProfile(browserAdded.).OnBrowserAdded(browserAdded.BrowserProfileId, browserAdded.BrowserDisplayName, browserAdded.ProfileDisplayName);
                //    break;
                case NotifyMessageReceived messageReceived:
                    {
                        MyUser? user = null;
                        if (messageReceived.UserId is not null)
                        {
                            user = _userStore.GetUser(messageReceived.UserId);
                            user.Nickname = messageReceived.Nickname;
                        }
                        _adapter.OnMessageReceived(messageReceived, user);
                    }
                    break;
                case NotifyMetadataUpdated metadataUpdated:
                    _adapter.OnMetadataUpdated(metadataUpdated);
                    break;
                case NotifyPluginAdded pluginAdded:
                    {
                        OnPluginAdded(pluginAdded.PluginId, pluginAdded.PluginName, pluginAdded.PluginRole);
                    }
                    break;
            }
        }
        private void OnPluginAdded(IPluginInfo pluginInfo)
        {
            OnPluginAdded(pluginInfo.Id, pluginInfo.Name, pluginInfo.Roles);
        }
        private void OnPluginAdded(PluginId pluginId, string pluginName, List<string> pluginRole)
        {
            if (pluginId == Id)
            {
                return;
            }
            if (PluginTypeChecker.IsSitePlugin(pluginRole))
            {
                var displayName = _adapter.GetSitePluginDisplayName(pluginId);
                _adapter.OnSiteAdded(pluginId, displayName);
            }
            else if (PluginTypeChecker.IsBrowserPlugin(pluginRole))
            {
                var browserProfiles = Host.RequestMessage(new GetDirectMessage(pluginId, new GetBrowserProfiles())) as ReplyBrowserProfiles;
                if (browserProfiles is null)
                {
                    throw new Exception("");
                }
                foreach (var profile in browserProfiles.Profiles)
                {
                    _adapter.AddBrowserProfile(profile);
                }
            }
            else
            {
                _adapter.OnPluginAdded(pluginId, pluginName);
            }
        }

        public void SetMessage(ISetMessageToPluginV2 message)
        {
            switch (message)
            {
                case SetLoading _:
                    {
                        _options.Set(LoadOptions());
                        _adapter = new IAdapter(Host, _options);
                        _vm = new MainViewModel(_adapter);//_adapterのイベントを購読する処理がctorにある。SiteAddedがOnLoaded()の前に来るからこのタイミングで初期化しないと間に合わない。            
                        _adapter.AddEmptyBrowserProfile();
                        Host.SetMessage(new SetPluginHello(Id, Name, Roles));
                    }
                    break;
                case SetLoaded _:
                    {
                        _v = new MainWindow
                        {
                            DataContext = _vm
                        };
                        _v.Show();
                    }
                    break;
                case SetClosing _:
                    {
                        Host.SetMessage(new RequestSavePluginOptions(Name, _options.Serialize()));
                    }
                    break;
                case RequestShowSettingsPanelToPlugin _:
                    break;
                case RequestCloseToPlugin _:
                    _vm.IsClose = true;

                    _v.Visibility = System.Windows.Visibility.Hidden;

                    break;
            }
        }

        public IReplyMessageToPluginV2 RequestMessage(IGetMessageToPluginV2 message)
        {
            throw new NotImplementedException();
        }

        public MainViewPlugin()
        {
            _options = new DynamicOptionsTest();
        }
        private readonly UserStore _userStore = new();
    }
    internal class MyUser : INotifyPropertyChanged
    {
        public string? UserId { get; }
        public string? Nickname { get; internal set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        public MyUser(string? userId)
        {
            UserId = userId;
        }
    }
    class UserStore
    {
        private readonly Dictionary<string, MyUser> _dict = new();
        public MyUser GetUser(string userId)
        {
            if (_dict.TryGetValue(userId, out var user))
            {
                return user;
            }
            else
            {
                user = new MyUser(userId);
                _dict.Add(userId, user);
                return user;
            }
        }
    }
}
