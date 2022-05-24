using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Common.Controls;

public class AlbumArtworkPanelConstrol : Control
{
    public AlbumArtworkPanelConstrol()
    {
        this.DefaultStyleKey = typeof(AlbumArtworkPanelConstrol);
    }

    public ImageSource? AlbumArtImage
    {
        get { return base.GetValue(AlbumArtImageProperty) as ImageSource; }
        set { base.SetValue(AlbumArtImageProperty, value); }
    }

    public static readonly DependencyProperty AlbumArtImageProperty =
      DependencyProperty.Register("AlbumArtImage", typeof(ImageSource), typeof(AlbumArtworkPanelConstrol));

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
    }
}
