using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Maptz.SpeechToText.Bing.Client
{
    public interface ISpeechToTextService
    {
        Task<IEnumerable<SpeechResult>> Convert(string audioFilePath);
    }

}