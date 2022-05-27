using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Common.Controls;

public class PlayListControl : ListBox
{
    public PlayListControl()
    {
        this.DefaultStyleKey = typeof(PlayListControl);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
    }
}

[TemplatePart(Name = PlayInfoPartName, Type = typeof(Grid))]
public class PlayListItemControl : Control
{
    private const string PlayInfoPartName = "PART_PlayInfo";
    private Grid? _playInfoPart;

    public PlayListItemControl()
    {
        this.DefaultStyleKey = typeof(PlayListItemControl);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _playInfoPart = GetTemplateChild(PlayInfoPartName) as Grid;

        if (_playInfoPart is null) return;
        _playInfoPart.MouseDown += this.PlayInfoPart_MouseDown;
    }

    private void PlayInfoPart_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
        {
            if (PlayCommand is not null)
                PlayCommand.Execute(PlayCommandParameter);
        }
    }

    public Guid? Id
    {
        get { return base.GetValue(IdProperty) as Guid?; }
        set { base.SetValue(IdProperty, value); }
    }

    public static readonly DependencyProperty IdProperty =
      DependencyProperty.Register("Id", typeof(Guid), typeof(PlayListItemControl), new UIPropertyMetadata(null));

    public static readonly DependencyProperty PlayCommandProperty =
            DependencyProperty.Register(
                "PlayCommand",
                typeof(ICommand),
                typeof(PlayListItemControl),
                new UIPropertyMetadata(null));

    public ICommand PlayCommand
    {
        get
        {
            return (ICommand)GetValue(PlayCommandProperty);
        }
        set
        {
            SetValue(PlayCommandProperty, value);
        }
    }

    public static readonly DependencyProperty PlayCommandParameterProperty =
            DependencyProperty.Register(
                "PlayCommandParameter",
                typeof(Object),
                typeof(PlayListItemControl),
                new UIPropertyMetadata(null));

    public Object? PlayCommandParameter
    {
        get
        {
            return (Object?)GetValue(PlayCommandParameterProperty);
        }
        set
        {
            SetValue(PlayCommandParameterProperty, value);
        }
    }
}