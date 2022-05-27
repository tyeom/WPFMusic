using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace Models;

public class PlayInfoModel : ObservableObject, IEquatable<PlayInfoModel>
{
    private Guid _id;
    private bool _isChecked;

    // 음원 메타 정보
    private TagLib.Tag? _tag;
    private string _albumText = "unknown album";
    private string _artistText = "unknown artist";
    private string _titleText = "unknown title";
    private string _yearText = "unknown year";
    private string _genreText = "unknown genre";
    private string _trackText = "unknown track";
    private string _discText = "unknown disc";
    private BitmapImage? _albumImage;
    
    // 플레이 정보
    /// <summary>
    /// 전체 음원 길이
    /// </summary>
    private long _channelLength;
    /// <summary>
    /// 현재 재생 위치
    /// </summary>
    private long _channelPosition;
    /// <summary>
    /// 전체 재생 시간
    /// </summary>
    private DateTime _totalTime;
    /// <summary>
    /// 현재 재생 시간
    /// </summary>
    private DateTime _elapsedTime;
    /// <summary>
    /// 현재 남은 재생 시간
    /// </summary>
    private TimeSpan _remainingTime;

    // 컨트롤 패널
    private bool _canPlay;
    private bool _canPause;
    private bool _canStop;
    private bool _isPlaying;

    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }

    public string AlbumText
    {
        get => string.IsNullOrWhiteSpace(_albumText) ? "unknown album" : _albumText;
        set => SetProperty(ref _albumText, value);
    }

    public TagLib.Tag? Tag
    {
        get => _tag;
        set => SetProperty(ref _tag, value);
    }

    public string ArtistText
    {
        get => string.IsNullOrWhiteSpace(_artistText) ? "unknown artist" : _artistText;
        set => SetProperty(ref _artistText, value);
    }

    public string TitleText
    {
        get => string.IsNullOrWhiteSpace(_titleText) ? "unknown title" : _titleText;
        set => SetProperty(ref _titleText, value);
    }

    public string YearText
    {
        get => string.IsNullOrWhiteSpace(_yearText) ? "unknown year" : _yearText;
        set => SetProperty(ref _yearText, value);
    }

    public string GenreText
    {
        get => string.IsNullOrWhiteSpace(_genreText) ? "unknown genre" : _genreText;
        set => SetProperty(ref _genreText, value);
    }

    public string TrackText
    {
        get => string.IsNullOrWhiteSpace(_trackText) ? "unknown track" : _trackText;
        set => SetProperty(ref _trackText, value);
    }

    public string DiscText
    {
        get => string.IsNullOrWhiteSpace(_discText) ? "unknown disc" : _discText;
        set => SetProperty(ref _discText, value);
    }

    public BitmapImage? AlbumImage
    {
        get => _albumImage;
        set => SetProperty(ref _albumImage, value);
    }

    public long ChannelLength
    {
        get => _channelLength;
        set => SetProperty(ref _channelLength, value);
    }

    public long ChannelPosition
    {
        get => _channelPosition;
        set
        {
            if(InTimerPositionUpdate is false)
                SetProperty(ref _channelPosition, value);
        }
    }

    public DateTime TotalTime
    {
        get => _totalTime;
        set => SetProperty(ref _totalTime, value);
    }

    public DateTime ElapsedTime
    {
        get => _elapsedTime;
        set => SetProperty(ref _elapsedTime, value);
    }

    public TimeSpan RemainingTime
    {
        get => _remainingTime;
        set => SetProperty(ref _remainingTime, value);
    }

    /// <summary>
    /// 타임 트랙 이동중 여부
    /// </summary>
    public bool InTimerPositionUpdate { get; set; }

    public bool CanPlay
    {
        get => _canPlay;
        set => SetProperty(ref _canPlay, value);
    }

    public bool CanPause
    {
        get => _canPause;
        set => SetProperty(ref _canPause, value);
    }

    public bool CanStop
    {
        get => _canStop;
        set => SetProperty(ref _canStop, value);
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        set => SetProperty(ref _isPlaying, value);
    }

    public string? FilePath { get; set; }

    /// <summary>
    /// 재생 된 곡인지 여부
    /// </summary>
    public bool IsPlayed { get; set; }

    public void SetPlayInfo()
    {
        if (Tag is null) return;

        AlbumText = Tag.Album;
        ArtistText = Tag.AlbumArtists.Length > 0 ? Tag.AlbumArtists[0] : string.Empty;
        TitleText = Tag.Title;
        YearText = Tag.Year.ToString(CultureInfo.InvariantCulture);
        GenreText = Tag.Genres.Length > 0 ? Tag.Genres[0] : string.Empty;
        TrackText = Tag.Track.ToString(CultureInfo.InvariantCulture);
        DiscText = Tag.Disc.ToString(CultureInfo.InvariantCulture);

        if (Tag.Pictures.Length > 0)
        {
            using (MemoryStream albumArtworkMemStream = new MemoryStream(Tag.Pictures[0].Data.Data))
            {
                BitmapImage albumImage = new BitmapImage();
                albumImage.BeginInit();
                albumImage.CacheOption = BitmapCacheOption.OnLoad;
                albumImage.StreamSource = albumArtworkMemStream;
                albumImage.EndInit();

                AlbumImage = albumImage;

                albumArtworkMemStream.Close();
            }
        }
    }

    #region IEquatable 구현
    public bool Equals(PlayInfoModel? other)
    {
        if(Object.ReferenceEquals(other, null)) return false;
        if (Object.ReferenceEquals(this, other)) return true;
        return FilePath.Equals(other.FilePath) || ( TitleText.Equals(other.TitleText) && ArtistText.Equals(other.ArtistText) );
    }
    #endregion  // IEquatable 구현

    public override int GetHashCode()
    {
        int hashPlayTitleName = TitleText == null ? 0 : TitleText.GetHashCode();

        //Get hash code for the Code field.
        int hashPlayArtistCode = ArtistText == null ? 0 : ArtistText.GetHashCode();

        //Calculate the hash code for the product.
        return hashPlayTitleName ^ hashPlayArtistCode;
    }
}
