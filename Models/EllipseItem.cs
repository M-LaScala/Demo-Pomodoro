using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text;

namespace Demo_Pomodoro.Models
{
    public partial class EllipseItem : ObservableObject
    {
        public int Id { get; set; }

        [ObservableProperty]
        public partial Color FillColor { get; set; } = Colors.Transparent;

        [ObservableProperty]
        public partial bool Visibility { get; set; }
    }
}
