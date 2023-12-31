﻿using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using MouseButton = Avalonia.Remote.Protocol.Input.MouseButton;

namespace LayoutTransformTest.Views;

public partial class MyZoomBorder : ContentControl
{
    public double MaxScale
    {
        get => GetValue(MaxScaleProperty);
        set => SetValue(MaxScaleProperty, value);
    }

    public static StyledProperty<double> MaxScaleProperty =
        AvaloniaProperty.Register<MyZoomBorder, double>(nameof(MaxScale), 5, validate: d => d>=1);

    public double MinScale
    {
        get => GetValue(MinScaleProperty);
        set => SetValue(MinScaleProperty, value);
    }

    public static StyledProperty<double> MinScaleProperty =
        AvaloniaProperty.Register<MyZoomBorder, double>(nameof(MinScale), 0.2, validate: d => d<=1);

    public double ScaleRatio
    {
        get => GetValue(ScaleRatioProperty);
        set => SetValue(ScaleRatioProperty, value);
    }

    public static StyledProperty<double> ScaleRatioProperty =
        AvaloniaProperty.Register<MyZoomBorder, double>(nameof(ScaleRatio), 0.9);

    public MouseButton PanButton
    {
        get => GetValue(PanButtonProperty);
        set => SetValue(PanButtonProperty, value);
    }

    public static StyledProperty<MouseButton> PanButtonProperty =
        AvaloniaProperty.Register<MyZoomBorder, MouseButton>(nameof(PanButton), MouseButton.Middle);


    public MyZoomBorder()
    {
        InitializeComponent();
    }

    private void FitToBounds(double width, double height, double marginX, double marginY)
    {
        double scale = 1;
        var scaleX =  Bounds.Width/width;
        var scaleY = Bounds.Height/height;
        TranslateTransform transition = null;
        if (scaleY<scaleX)
        {
            if (scaleY > MaxScale)
            {
                scaleY = MaxScale;
            }

            if (scaleY < MinScale)
            {
                scaleY = MinScale;
            }

            totalScale = scaleY;
            transition = new TranslateTransform((Bounds.Width - width * scaleY)/2 - marginX*scaleY,  -marginY*scaleY);
            TransformControl.LayoutTransform = new ScaleTransform(scaleY, scaleY);
            TransformControl.RenderTransform = transition;
            
        }
        else
        {
            if (scaleX > MaxScale)
            {
                scaleX = MaxScale;
            }

            if (scaleX < MinScale)
            {
                scaleX = MinScale;
            }

            totalScale = scaleX;
            transition = new TranslateTransform(-marginX*scaleX, (Bounds.Height - height * scaleX)/2 - marginY*scaleX);
            TransformControl.LayoutTransform = new ScaleTransform(scaleX, scaleX);
            TransformControl.RenderTransform = transition;
        }
    }

    public void FitContent()
    {
        var content = (TransformControl.Child as ContentPresenter).Child;
        if(content == null) return;
        FitToBounds(content.Bounds.Width,content.Bounds.Height, 0, 0 );
        
    }

    public void FitItems()
    {
        var itemsControl = this.FindDescendantOfType<ItemsControl>();
        var items = itemsControl.Items.Select(x => x as Control).ToList();
        var rightItem = items.MaxBy(x => x.Margin.Left);
        var width = rightItem.Margin.Left + rightItem.Width - items.MinBy(x => x.Margin.Left).Margin.Left;
        var bottomItem = items.MaxBy(x => x.Margin.Top);
        var height = bottomItem.Margin.Top+bottomItem.Height - items.MinBy(x => x.Margin.Top).Margin.Top;
        FitToBounds(width,height,items.MinBy(x => x.Margin.Left).Margin.Left, items.MinBy(x => x.Margin.Top).Margin.Top );
    }
    

    private bool _isHolding = false;
    
    private Point _lastPoint;

    private double totalScale = 1;
    private LayoutTransformControl TransformControl => (LayoutTransformControl)((Border)this.VisualChildren[0]).Child;
    
    public void ZoomOut()
    {
        SetScale(ScaleRatio);
    }

    public void ZoomIn()
    {
        SetScale(1/ScaleRatio);
    }

    private void SetScale(double addScale)
    {
        if (totalScale * addScale > MaxScale || totalScale * addScale < MinScale) return;
        totalScale *= addScale;
        
        Vector delta = this.TranslatePoint(new Point(Bounds.Width / 2, Bounds.Height / 2), TransformControl).Value- TransformControl.Bounds.TopLeft;
        delta *= (1 - addScale);

        var scale = new ScaleTransform(addScale, addScale).Value;
        var transition = new TranslateTransform(delta.X, delta.Y).Value;
        TransformControl.LayoutTransform =
            new MatrixTransform(
                TransformControl.LayoutTransform?.Value.Append(scale) ?? scale);
        TransformControl.RenderTransform = new MatrixTransform(
            TransformControl.RenderTransform?.Value.Append(transition) ?? transition);
    }
    
    private void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        var addScale = ScaleRatio;
        if (e.Delta.Y > 0)
        {
            addScale = 1 / ScaleRatio;
        }

        if (totalScale * addScale > MaxScale || totalScale * addScale < MinScale) return;

        totalScale *= addScale;
        Vector delta = e.GetPosition(TransformControl) - TransformControl.Bounds.TopLeft;
        delta *= (1 - addScale);

        var scale = new ScaleTransform(addScale, addScale).Value;
        var transition = new TranslateTransform(delta.X, delta.Y).Value;
        TransformControl.LayoutTransform =
            new MatrixTransform(
                TransformControl.LayoutTransform?.Value.Append(scale) ?? scale);
        TransformControl.RenderTransform = new MatrixTransform(
            TransformControl.RenderTransform?.Value.Append(transition) ?? transition);
    }

    private void Trans_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var props = e.GetCurrentPoint(this).Properties;
        if (props.IsLeftButtonPressed && PanButton == MouseButton.Left ||
            props.IsRightButtonPressed && PanButton == MouseButton.Right ||
            props.IsMiddleButtonPressed && PanButton == MouseButton.Middle)
        {
            _isHolding = true;
            _lastPoint = e.GetPosition(TransformControl);
        }
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isHolding)
        {
            var delta = e.GetPosition(TransformControl) - _lastPoint;
            var transition = new TranslateTransform(delta.X, delta.Y).Value;
            TransformControl.RenderTransform = new MatrixTransform(
                TransformControl.RenderTransform?.Value.Append(transition) ?? transition);
        }
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isHolding = false;
    }

    private void AvaloniaObject_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Child")
        {
            TransformControl.LayoutTransform = null;
            TransformControl.RenderTransform = null;
            (sender as ContentPresenter).Child.Loaded += OnLoaded;
            var oldChild = e.OldValue as Control;
            var newChild = e.NewValue as Control;
            /*oldChild.PropertyChanged+= OldChildOnPropertyChanged;*/
        }
    }

    private void OldChildOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        FitContent();
        (sender as Control).Loaded-= OnLoaded;
    }
}