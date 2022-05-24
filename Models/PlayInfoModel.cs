using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Models;

public class PlayInfoModel : ObservableObject
{
    private TagLib.Tag _fileTag;
    private string? _albumText;
    private string? _artistText;
    private string? _titleText;
    private string? _yearText;
    private string? _genreText;
    private string? _trackText;
    private string? _discText;
    private BitmapImage? _albumImage;
    private bool _canPlay;
    private bool _canPause;
    private bool _canStop;
    private bool _isPlaying;

    public TagLib.Tag FileTag
    {
        get => _fileTag;
        set => SetProperty(ref _fileTag, value);
    }

    public string? AlbumText
    {
        get => string.IsNullOrWhiteSpace(_albumText) ? "unknown album" : _albumText;
        set => SetProperty(ref _albumText, value);
    }

    public string? ArtistText
    {
        get => string.IsNullOrWhiteSpace(_artistText) ? "unknown artist" : _artistText;
        set => SetProperty(ref _artistText, value);
    }

    public string? TitleText
    {
        get => string.IsNullOrWhiteSpace(_titleText) ? "unknown title" : _titleText;
        set => SetProperty(ref _titleText, value);
    }

    public string? YearText
    {
        get => string.IsNullOrWhiteSpace(_yearText) ? "unknown year" : _yearText;
        set => SetProperty(ref _yearText, value);
    }

    public string? GenreText
    {
        get => string.IsNullOrWhiteSpace(_genreText) ? "알 수 없음" : _genreText;
        set => SetProperty(ref _genreText, value);
    }

    public string? TrackText
    {
        get => string.IsNullOrWhiteSpace(_trackText) ? "unknown track" : _trackText;
        set => SetProperty(ref _trackText, value);
    }

    public string? DiscText
    {
        get => string.IsNullOrWhiteSpace(_discText) ? "unknown disc" : _discText;
        set => SetProperty(ref _discText, value);
    }

    public BitmapImage? AlbumImage
    {
        get => _albumImage;
        set => SetProperty(ref _albumImage, value);
    }

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

    public void SetPlayInfo(TagLib.Tag fileTag)
    {
        FileTag = fileTag;
        AlbumText = FileTag.Album;
        ArtistText = FileTag.AlbumArtists.Length > 0 ? FileTag.AlbumArtists[0] : string.Empty;
        TitleText = FileTag.Title;
        YearText = FileTag.Year.ToString(CultureInfo.InvariantCulture);
        GenreText = FileTag.Genres.Length > 0 ? FileTag.Genres[0] : string.Empty;
        TrackText = FileTag.Track.ToString(CultureInfo.InvariantCulture);
        DiscText = FileTag.Disc.ToString(CultureInfo.InvariantCulture);

        if (FileTag.Pictures.Length > 0)
        {
            using (MemoryStream albumArtworkMemStream = new MemoryStream(FileTag.Pictures[0].Data.Data))
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
}
