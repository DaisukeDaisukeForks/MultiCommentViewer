﻿using System.ComponentModel;

namespace YoyakuPlugin;

public interface IOptions : INotifyPropertyChanged
{
    string ReserveCommandPattern { get; set; }
    string DeleteCommandPattern { get; set; }
    string AlreadyReserved_Message { get; set; }
    string AlreadyReserved_Se { get; set; }
    string Call_Message { get; set; }
    string Call_Se { get; set; }
    string Delete_Message { get; set; }
    string Delete_Se { get; set; }
    string DeleteByOther_Message { get; set; }
    string DeleteByOther_Se { get; set; }
    string Explain_Message { get; set; }
    string Explain_Se { get; set; }
    string HandleNameNotSubscribed_Message { get; set; }
    string HandleNameNotSubscribed_Se { get; set; }
    string HcgSettingFilePath { get; set; }
    bool IsEnabled { get; set; }
    string NotReserved_Message { get; set; }
    string NotReserved_Se { get; set; }
    string Reserved_Message { get; set; }
    string Reserved_Se { get; set; }
    double DateWidth { get; set; }
    double IdWidth { get; set; }
    double NameWidth { get; set; }
    double CalledWidth { get; set; }

    void Deserialize(string s);
    string Serialize();
    void Reset();
}