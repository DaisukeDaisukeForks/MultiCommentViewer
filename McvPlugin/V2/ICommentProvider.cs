﻿using System.ComponentModel;
using System.Net;

namespace Mcv.PluginV2;

public interface ICommentProvider
{
    event EventHandler<ConnectedEventArgs> Connected;
    event EventHandler<IMessageContext> MessageReceived;
    event EventHandler<IMetadata> MetadataUpdated;
    Task ConnectAsync(string input, List<Cookie> cookies);
    void SetMessage(string raw);
    void Disconnect();
    bool CanConnect { get; }
    bool CanDisconnect { get; }
    event EventHandler CanConnectChanged;
    event EventHandler CanDisconnectChanged;

    Task PostCommentAsync(string text);
    Task<ICurrentUserInfo> GetCurrentUserInfo(List<Cookie> cookies);
    //Guid SiteContextGuid { get; }
}
public class ConnectedEventArgs : EventArgs
{
    /// <summary>
    /// 入力値の保存が必要か
    /// YouTubeLiveの場合であれば、放送URLはfalse,channelURLはtrue
    /// </summary>
    public bool IsInputStoringNeeded { get; set; }
    /// <summary>
    /// 次回起動時にリストアするURL
    /// </summary>
    public string UrlToRestore { get; set; }
}
//[Obsolete]
//public interface IUser : INotifyPropertyChanged
//{
//    string UserId { get; }
//    IEnumerable<IMessagePart> Name { get; set; }
//    string Nickname { get; set; }
//    string ForeColorArgb { get; set; }
//    string BackColorArgb { get; set; }
//    bool IsNgUser { get; set; }
//}
//[Obsolete]
//public interface IUser2 : IUser
//{
//    bool IsSiteNgUser { get; set; }
//}
public class McvUser
{
    public string UserId { get; }
    public IEnumerable<IMessagePart>? Name { get; set; }
    public string? Nickname { get; set; }
    public bool IsNgUser { get; set; }
    public bool IsSiteNgUser { get; set; }
    public string? BackColorArgb { get; set; }
    public string? ForeColorArgb { get; set; }
    public McvUser(string userId)
    {
        UserId = userId;
    }
}