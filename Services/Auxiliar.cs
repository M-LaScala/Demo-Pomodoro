using Demo_Pomodoro.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Demo_Pomodoro.Services
{
    public static class Auxiliar
    {
        public static bool IsInteracting { get; set; } = false;
        public static bool IsActive { get; set; } = true;

        private static readonly int MaxCycles = 8;

        public static ObservableCollection<EllipseItem> GenerateEllipseItems(int cycleCount)
        {
            var ellipses = new ObservableCollection<EllipseItem>();
            for (int i = 0; i < MaxCycles; i++)
            {
                ellipses.Add(new EllipseItem { Id = i + 1, FillColor = Colors.Transparent, Visibility = i < cycleCount });
            }
            return ellipses;
        }

        public static void UpdateEllipseFill(ObservableCollection<EllipseItem> ellipses, int completedCycles)
        {
            for (int i = 0; i < completedCycles; i++)
            {
                ellipses[i].FillColor = Color.FromArgb("#55B25A");
            }       
        }
    }
}
