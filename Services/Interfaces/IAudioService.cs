using System;
using System.Collections.Generic;
using System.Text;

namespace Demo_Pomodoro.Services.Interfaces
{
    public interface IAudioService
    {
        Task PlayAlarmAsync();
    }
}
