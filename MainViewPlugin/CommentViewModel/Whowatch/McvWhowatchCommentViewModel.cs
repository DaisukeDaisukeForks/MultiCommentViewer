﻿using Mcv.PluginV2;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WhowatchSitePlugin;

namespace Mcv.MainViewPlugin;
class McvWhowatchCommentViewModel : IMcvCommentViewModel
{
    private readonly WhowatchSitePlugin.IWhowatchMessage _message;
    private readonly IMainViewPluginOptions _options;
    private readonly MyUser? _user;

    private void SetNickname(MyUser user)
    {
        if (!string.IsNullOrEmpty(user.Nickname))
        {
            _nickItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(user.Nickname) };
        }
        else
        {
            _nickItems = null;
        }
    }
    private McvWhowatchCommentViewModel(ConnectionName connectionStatus, IMainViewPluginOptions options, MyUser? user)
    {
        ConnectionName = connectionStatus;
        _options = options;
        ConnectionName.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(ConnectionName.Name):
                    RaisePropertyChanged(nameof(ConnectionName));
                    break;
                case nameof(ConnectionName.BackColor):
                    RaisePropertyChanged(nameof(Background));
                    break;
                case nameof(ConnectionName.ForeColor):
                    RaisePropertyChanged(nameof(Foreground));
                    break;
            }
        };
        options.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(options.IsEnabledSiteConnectionColor):
                    RaisePropertyChanged(nameof(Background));
                    RaisePropertyChanged(nameof(Foreground));
                    break;
            }
        };
        options.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(options.BackColor):
                    RaisePropertyChanged(nameof(Background));
                    break;
                case nameof(options.ForeColor):
                    RaisePropertyChanged(nameof(Foreground));
                    break;
                case nameof(options.FontFamily):
                    RaisePropertyChanged(nameof(FontFamily));
                    break;
                case nameof(options.FontStyle):
                    RaisePropertyChanged(nameof(FontStyle));
                    break;
                case nameof(options.FontWeight):
                    RaisePropertyChanged(nameof(FontWeight));
                    break;
                case nameof(options.FontSize):
                    RaisePropertyChanged(nameof(FontSize));
                    break;
                case nameof(options.IsUserNameWrapping):
                    RaisePropertyChanged(nameof(UserNameWrapping));
                    break;
            }
        };
        if (user != null)
        {
            user.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(user.Nickname):
                        SetNickname(user);
                        RaisePropertyChanged(nameof(NameItems));
                        break;
                }
            };
            SetNickname(user);
        }
    }
    public McvWhowatchCommentViewModel(IWhowatchComment comment, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
   : this(connName, options, user)
    {
        _message = comment;

        _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.UserName);
        MessageItems = Common.MessagePartFactory.CreateMessageItems(comment.Comment);
        Thumbnail = comment.UserIcon;
        Id = comment.Id;
        PostTime = comment.PostTime;
    }
    public McvWhowatchCommentViewModel(IWhowatchItem item, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
       : this(connName, options, user)
    {
        var comment = item;
        _message = comment;

        _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.UserName);
        MessageItems = Common.MessagePartFactory.CreateMessageItems(comment.Comment);
        Thumbnail = new MessageImage
        {
            Url = comment.UserIconUrl,
            Alt = "",
            Height = 40,//_optionsにcolumnの幅を動的に入れて、ここで反映させたい。propertyChangedはどうやって発生させるか
            Width = 40,
        };
        Id = comment.Id.ToString();
        PostTime = UnixtimeToDateTime(comment.PostedAt / 1000).ToString("HH:mm:ss");
        Info = item.ItemCount == 1 ? item.ItemName : $"{item.ItemName} × {item.ItemCount}";
    }
    public McvWhowatchCommentViewModel(IWhowatchConnected connected, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
   : this(connName, options, user)
    {
        _message = connected;
        MessageItems = Common.MessagePartFactory.CreateMessageItems(connected.Text);
    }
    public McvWhowatchCommentViewModel(IWhowatchDisconnected disconnected, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
      : this(connName, options, user)
    {
        _message = disconnected;
        MessageItems = Common.MessagePartFactory.CreateMessageItems(disconnected.Text);
    }

    public ConnectionName ConnectionName { get; }

    private IEnumerable<IMessagePart> _nickItems { get; set; }
    private IEnumerable<IMessagePart> _nameItems { get; set; }
    public IEnumerable<IMessagePart> NameItems
    {
        get
        {
            if (_nickItems != null)
            {
                return _nickItems;
            }
            else
            {
                return _nameItems;
            }
        }
    }

    public IEnumerable<IMessagePart> MessageItems
    {
        get
        {
            return _messageItems;
        }
        set
        {
            _messageItems = value;
            RaisePropertyChanged();
        }
    }

    public SolidColorBrush Background
    {
        get
        {
            if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Site)
            {
                return new SolidColorBrush(_options.WhowatchBackColor);
            }
            else if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Connection)
            {
                return new SolidColorBrush(ConnectionName.BackColor);
            }
            else
            {
                return new SolidColorBrush(_options.BackColor);
            }
        }
    }

    public FontFamily FontFamily => _options.FontFamily;

    public int FontSize => _options.FontSize;

    public FontStyle FontStyle => _options.FontStyle;

    public FontWeight FontWeight => _options.FontWeight;

    public SolidColorBrush Foreground
    {
        get
        {
            if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Site)
            {
                return new SolidColorBrush(_options.WhowatchForeColor);
            }
            else if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Connection)
            {
                return new SolidColorBrush(ConnectionName.ForeColor);
            }
            else
            {
                return new SolidColorBrush(_options.ForeColor);
            }
        }
    }

    public string Id { get; private set; }

    public string Info { get; private set; }

    public bool IsVisible => true;

    public string PostTime { get; private set; }

    public IMessageImage Thumbnail { get; private set; }

    public string UserId => _user?.UserId;

    public TextWrapping UserNameWrapping
    {
        get
        {
            if (_options.IsUserNameWrapping)
            {
                return TextWrapping.Wrap;
            }
            else
            {
                return TextWrapping.NoWrap;
            }
        }
    }

    public bool IsTranslated { get; set; }
    #region INotifyPropertyChanged
    [NonSerialized]
    private System.ComponentModel.PropertyChangedEventHandler? _propertyChanged;
    private IEnumerable<IMessagePart> _messageItems;

    /// <summary>
    /// 
    /// </summary>
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged
    {
        add { _propertyChanged += value; }
        remove { _propertyChanged -= value; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>
    protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        _propertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
    #endregion

    private static DateTime UnixtimeToDateTime(long unixTimeStamp)
    {
        var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        return dt.AddSeconds(unixTimeStamp).ToLocalTime();
    }
}
