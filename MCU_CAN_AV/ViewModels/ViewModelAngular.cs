using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.VisualElements;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using LiveChartsCore;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.VisualElements;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.Defaults;
using CommunityToolkit.Mvvm.Input;
using System;

namespace MCU_CAN_AV.ViewModels
{

    internal partial class ViewModelAngular : ObservableObject
    {
        private readonly Random _random = new();

        public ViewModelAngular()
        {
            var sectionsOuter = 130;
            var sectionsWidth = 20;

            Needle = new NeedleVisual
            {
                Value = 45


            };

            Series = GaugeGenerator.BuildAngularGaugeSections(
                new GaugeItem(60, s => SetStyle(sectionsOuter, sectionsWidth, s)),
                new GaugeItem(30, s => SetStyle(sectionsOuter, sectionsWidth, s)),
                new GaugeItem(10, s => SetStyle(sectionsOuter, sectionsWidth, s)));

            VisualElements = new VisualElement<SkiaSharpDrawingContext>[]
            {
            new AngularTicksVisual
            {
                LabelsSize = 16,
                LabelsOuterOffset = 15,
                OuterOffset = 65,
                TicksLength = 20


            },
            Needle
            };
        }

        public IEnumerable<ISeries> Series { get; set; }

        public IEnumerable<VisualElement<SkiaSharpDrawingContext>> VisualElements { get; set; }

        public NeedleVisual Needle { get; set; }

        [RelayCommand]
        public void DoRandomChange()
        {
            // modifying the Value property updates and animates the chart automatically
            Needle.Value = _random.Next(0, 100);

        }

        private static void SetStyle(
            double sectionsOuter, double sectionsWidth, PieSeries<ObservableValue> series)
        {
            series.OuterRadiusOffset = sectionsOuter;
            series.MaxRadialColumnWidth = sectionsWidth;
        }

    }
}
