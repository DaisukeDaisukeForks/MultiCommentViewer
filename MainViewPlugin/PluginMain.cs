﻿using Mcv.PluginV2;
using Mcv.PluginV2.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mcv.MainViewPlugin;

[Export(typeof(IPlugin))]
public class MainViewPlugin : IPlugin
{
    public PluginId Id { get; } = new PluginId(new Guid("A53CCC44-6A14-4533-99CA-184D5584E257"));
    public string Name => "MainViewPlugin";
    public List<string> Roles { get; } = new List<string> { "mainview" };
    public IPluginHost Host { get; set; } = default!;
    private IAdapter _adapter = default!;
    private MainViewModel _vm = default!;
    private readonly DynamicOptionsTest _options;
    private MainWindow _v = default!;
    private async Task<IMainViewPluginOptions> LoadOptions()
    {
        var loadedOptions = await Host.RequestMessageAsync(new RequestLoadPluginOptions(Name)) as ReplyPluginOptions;

        var options = new DynamicOptionsTest();
        options.Deserialize(loadedOptions?.RawOptions);
        return options;
    }
    public async Task SetMessageAsync(INotifyMessageV2 message)
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
                        await OnPluginAdded(pluginInfo);
                    }
                }
                break;
            case NotifyConnectionStatusChanged connStChanged:
                _adapter.OnConnectionStatusChanged(connStChanged.ConnStDiff);
                break;
            case NotifyMessageReceived messageReceived:
                {
                    MyUser? user = null;
                    if (messageReceived.UserId is not null)
                    {
                        user = _adapter.GetUser(messageReceived.UserId);
                        user.Name = messageReceived.Username;
                        user.Nickname = messageReceived.Nickname;
                        user.IsNgUser = messageReceived.IsNgUser;
                    }
                    _adapter.OnMessageReceived(messageReceived, user);
                }
                break;
            case NotifyMetadataUpdated metadataUpdated:
                _adapter.OnMetadataUpdated(metadataUpdated);
                break;
            case NotifyPluginAdded pluginAdded:
                {
                    await OnPluginAdded(pluginAdded.PluginId, pluginAdded.PluginName, pluginAdded.PluginRole);
                }
                break;
            case NotifyDownloadProgress progress:
                //_adapter.OnDownloadProgress(progress);
                break;
        }
    }
    private Task OnPluginAdded(IPluginInfo pluginInfo)
    {
        return OnPluginAdded(pluginInfo.Id, pluginInfo.Name, pluginInfo.Roles);
    }
    private async Task OnPluginAdded(PluginId pluginId, string pluginName, List<string> pluginRole)
    {
        Debug.WriteLine($"OnPluginAdded={pluginName}");
        if (pluginId == Id)
        {
            return;
        }
        if (PluginTypeChecker.IsSitePlugin(pluginRole))
        {
            var displayName = await _adapter.GetSitePluginDisplayName(pluginId);
            _adapter.OnSiteAdded(pluginId, displayName);
        }
        else if (PluginTypeChecker.IsBrowserPlugin(pluginRole))
        {
            var browserProfiles = await Host.RequestMessageAsync(new GetDirectMessage(pluginId, new GetBrowserProfiles())) as ReplyBrowserProfiles;
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

    public async Task SetMessageAsync(ISetMessageToPluginV2 message)
    {
        switch (message)
        {
            case SetLoading _:
                {
                    _options.Set(await LoadOptions());
                    _adapter = new IAdapter(Host, _options);
                    _vm = new MainViewModel(_adapter);//_adapterのイベントを購読する処理がctorにある。SiteAddedがOnLoaded()の前に来るからこのタイミングで初期化しないと間に合わない。            
                    _adapter.AddEmptyBrowserProfile();
                    await Host.SetMessageAsync(new SetPluginHello(Id, Name, Roles));
                }
                break;
            case SetLoaded _:
                {
                    await _adapter.LoadUserStoreAsync();
                    _v = new MainWindow
                    {
                        DataContext = _vm
                    };
                    _v.Show();
                    _v.Activate();
                    //アップデートがあるか確認する
                    var (updateExists, url, current, latest) = await _adapter.CheckIfUpdateExistsAsync();
                    if (updateExists)
                    {
                        //ユーザーにアップデートがあることを伝え、アップデートするか聞く
                        _adapter.SuggestToUpdate(url, current, latest);
                    }
                }
                break;
            case SetClosing _:
                {
                    _vm.IsClose = true;
                    _v.Visibility = System.Windows.Visibility.Hidden;
                    await Host.SetMessageAsync(new RequestSavePluginOptions(Name, _options.Serialize()));
                    await _adapter.SaveUserStoreAsync();
                }
                break;
            case RequestShowSettingsPanelToPlugin _:
                break;
            default:
                break;
        }
    }

    public Task<IReplyMessageToPluginV2> RequestMessageAsync(IGetMessageToPluginV2 message)
    {
        throw new NotImplementedException();
    }

    public MainViewPlugin()
    {
        _options = new DynamicOptionsTest();
    }

}
