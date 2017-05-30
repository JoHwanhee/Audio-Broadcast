using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audio_Broadcast
{
    interface IAudioSingleTone
    {
        void AudioSetting();
        void AudioStart();
        void AudioStop();

        bool IsAudioOpened { get; set; }
    }
}
