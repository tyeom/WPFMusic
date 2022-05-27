using Common.Base;
using Common.Extensions;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Models;
using Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Data;
using ViewModels.Messaging;

namespace ViewModels;

public class PlayListViewModel : ViewModelBase
{
    private readonly ISettingService _settingService;
    private readonly IBassService _bassService;
    private static readonly HttpClient Client = new HttpClient(new HttpClientHandler { MaxConnectionsPerServer = 10 });
    private ObservableCollection<PlayInfoModel> _playInfoList;
    private ICollectionView _playInfoListView;
    private string? _lyrics;
    private bool _showSearchPlayList = false;
    private string? _searchPlayList;

    public PlayListViewModel(ISettingService settingService, IBassService bassService)
    {
        _settingService = settingService;
        _bassService = bassService;

        WeakReferenceMessenger.Default.Register<SetPlayInfoListMessage>(this, this.SetPlayInfoList);
        WeakReferenceMessenger.Default.Register<SetPlayInfoMessage>(this, this.SetPlayInfo);
    }

    #region Properties
    public bool ShowSearchPlayList
    {
        get => _showSearchPlayList;
        set => SetProperty(ref _showSearchPlayList, value);
    }

    public string? SearchPlayList
    {
        get => _searchPlayList;
        set
        {
            SetProperty(ref _searchPlayList, value);
            if(PlayInfoListView is not null)
                PlayInfoListView.Refresh();
        }
    }

    public ObservableCollection<PlayInfoModel> PlayInfoList
    {
        get => _playInfoList;
        set => SetProperty(ref _playInfoList, value);
    }

    public ICollectionView PlayInfoListView
    {
        get => _playInfoListView;
        set => SetProperty(ref _playInfoListView, value);
    }

    public string? Lyrics
    {
        get => _lyrics;
        set => SetProperty(ref _lyrics, value);
    }
    #endregion  // Properties

    #region Commands
    private RelayCommand<bool> _showSearchPlayListCommand;
    public RelayCommand<bool> ShowSearchPlayListCommand
    {
        get
        {
            return _showSearchPlayListCommand ??
                (_showSearchPlayListCommand = new RelayCommand<bool>((param) =>
                {
                    ShowSearchPlayList = param;
                    if (param is false)
                        SearchPlayList = null;
                }));
        }
    }

    private RelayCommand<object?>? _addFileCommand;
    public RelayCommand<object?>? AddFileCommand
    {
        get
        {
            return _addFileCommand ??
                (_addFileCommand = new RelayCommand<object?>(this.AddFileExecute));
        }
    }

    private RelayCommand _removePlayListCommand;
    public RelayCommand RemovePlayListCommand
    {
        get
        {
            return _removePlayListCommand ??
                (_removePlayListCommand = new RelayCommand(this.RemovePlayListExecute));
        }
    }

    private RelayCommand? _deduplicationCommand;
    public RelayCommand? DeduplicationCommand
    {
        get
        {
            return _deduplicationCommand ??
                (_deduplicationCommand = new RelayCommand(this.DeduplicationExecute));
        }
    }

    private RelayCommand<PlayInfoModel> _playCommand;
    public RelayCommand<PlayInfoModel> PlayCommand
    {
        get
        {
            return _playCommand ??
                (_playCommand = new RelayCommand<PlayInfoModel>(this.PlayExecute));
        }
    }
    #endregion  // Commands

    #region Commands Execute Methods
    private void AddFileExecute(object? filePaths)
    {
        if (filePaths is null) return;

        string[] filePathArr = (string[])filePaths;
        foreach (var filePath in filePathArr)
        {
            TagLib.Tag tag = _bassService.GetTag(filePath);
            if (tag is null)
            {
                Logger.Log.Write($"음원 tag정보를 추출할 수 없습니다. - {filePath}");
                continue;
            }
            PlayInfoModel playInfo = new PlayInfoModel();
            playInfo.Id = Guid.NewGuid();
            playInfo.FilePath = filePath;
            playInfo.Tag = tag;
            playInfo.SetPlayInfo();

            PlayInfoList.Add(playInfo);
        }
    }

    private void RemovePlayListExecute()
    {
        PlayInfoList.ToList().ForEach(item =>
        {
            if (item.IsChecked)
                PlayInfoList.Remove(item);
        });
    }

    private void DeduplicationExecute()
    {
        var duplicate = PlayInfoList.Distinct();
        PlayInfoList.ToList().ForEach(item =>
        {
            if(duplicate.Any( p => p.Id == item.Id ) is false)
                PlayInfoList.Remove(item);
        });
    }

    private void PlayExecute(PlayInfoModel? playInfo)
    {
        if (playInfo is null) return;

        PlayResponseMessage responseMessage =
            WeakReferenceMessenger.Default.Send<PlayRequestMessage>(
                new PlayRequestMessage(playInfo)
                );
    }
    #endregion  // Commands Execute Methods

    #region Methods
    private void SetPlayInfoList(object recipient, SetPlayInfoListMessage setPlayInfoListMessage)
    {
        PlayInfoList = setPlayInfoListMessage.Value;
        PlayInfoListView = CollectionViewSource.GetDefaultView(PlayInfoList);
        if (PlayInfoListView.CanFilter)
        {
            PlayInfoListView.Filter = this.PlayInfoSearchFilter;
        }
    }

    private async void SetPlayInfo(object recipient, SetPlayInfoMessage setPlayInfoMessage)
    {
        string? lyrics = await this.GetLyricsAsync(setPlayInfoMessage.Value);
        string lyricsHtmlText = $"<head><link rel=\"stylesheet\" type=\"text/css\" href=\"https://cdn.jsdelivr.net/gh/moonspam/NanumSquare@1.0/nanumsquare.css\"><style type=\"text/css\"> body {{ font-family: 'NanumSquare', sans-serif; color: white; font-size: 13px;  }} </style></head> <body>{lyrics?? "가사 정보 요청 오류 발생"}</body>";

        Lyrics = lyricsHtmlText.Replace("cellspacing=0>", "")
            .Replace("cellpadding=0>", "")
            .Replace("cellspacing=0", "")
            .Replace("cellpadding=0", "")
            .Replace("<table", "")
            .Replace("<tr>", "")
            .Replace("<td>", "")
            .Replace("<td>", "")
            .Replace("<th>", "")
            .Replace("</tr>", "")
            .Replace("</td>", "")
            .Replace("</td>", "")
            .Replace("</th>", "")
            .Replace("</table>", "")
            .Replace("class='tabletext'", "");
    }

    private bool PlayInfoSearchFilter(object item)
    {
        if (string.IsNullOrWhiteSpace(SearchPlayList) is true)
            return true;

        PlayInfoModel? playInfo = item as PlayInfoModel;
        // Title 필터
        if (playInfo is not null &&
            playInfo.TitleText is not null &&
            playInfo.TitleText.Contains(SearchPlayList, StringComparison.OrdinalIgnoreCase) is true)
        {
            return true;
        }
        // Artist 필터
        else if (playInfo is not null &&
            playInfo.ArtistText is not null &&
            playInfo.ArtistText.Contains(SearchPlayList) is true)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private string EucKrUrlEncode(string str)
    {
        // euc-kr 코드 번호
        int euckrCodePage = 51949;
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        System.Text.Encoding euckr = System.Text.Encoding.GetEncoding(euckrCodePage);

        byte[] tmp = euckr.GetBytes(str);

        string res = "";

        foreach (byte b in tmp)
        {
            res += "%";
            res += string.Format("{0:X}", b);
        }

        return res;
    }

    private async Task<string?> GetLyricsAsync(PlayInfoModel playInfo)
    {
        string keyword = $"{(playInfo.ArtistText == "unknown artist" ? "" : playInfo.ArtistText+" ")}{(playInfo.TitleText == "unknown title" ? "" : playInfo.TitleText)}";
        if(string.IsNullOrWhiteSpace(keyword) is true)
        {
            return "곡 tag정보가 없습니다.";
        }

        try
        {
            // 01. 가사 검색 페이지 접속
            var responseMessage = await Client.GetAsync($"http://boom4u.net/lyrics/?keyword={this.EucKrUrlEncode(keyword)}&searchoption=");
            var searchResultHtml = await responseMessage.Content.ReadAsStringAsync();

            if(string.IsNullOrWhiteSpace(searchResultHtml) is true)
            {
                return "가사 검색 요청에 실패 하였습니다.";
            }

            var fixedTag = "view.php?id";
            var fixedTagIdx = searchResultHtml.IndexOf(fixedTag);
            if(fixedTagIdx <= -1)
            {
                return "가사를 찾을 수 없습니다.";
            }

            // 링크 query string id 추출
            var startIdx = searchResultHtml.IndexOf("=", (fixedTagIdx + 1));
            var endIdx = searchResultHtml.IndexOf("'", (startIdx + 1));
            var id = searchResultHtml.Substring((startIdx + 1), (endIdx - startIdx - 1));

            // 02. id로 가사 페이지 이동
            responseMessage = await Client.GetAsync($"http://boom4u.net/lyrics/view.php?id={id}");
            var lyricsResultHtmlByte = await responseMessage.Content.ReadAsByteArrayAsync();
            // 위 EucKrUrlEncode 메서드 호출에서 51949 인코드 코드를 등록했기 때문에 바로 사용할 수 있다.
            string lyricsResultHtml = System.Text.Encoding.GetEncoding(51949).GetString(lyricsResultHtmlByte);

            if (string.IsNullOrWhiteSpace(lyricsResultHtml) is true)
            {
                return "[02] 가사 검색 요청에 실패 하였습니다.";
            }

            fixedTag = "<table class='tabletext'";
            fixedTagIdx = lyricsResultHtml.IndexOf(fixedTag);
            if (fixedTagIdx <= -1)
            {
                return "[02] 가사를 찾을 수 없습니다.";
            }

            // 가사 추출
            startIdx = lyricsResultHtml.IndexOf("<", (fixedTagIdx));
            endIdx = lyricsResultHtml.IndexOf("</table>", (startIdx + 1));
            var lyrics = lyricsResultHtml.Substring((startIdx), (endIdx + 8 - startIdx));


            return lyrics;
        }
        catch(Exception ex)
        {
            Logger.Log.Error(ex);
            return null;
        }
    }
    #endregion  // Methods
}