using SpeechLib;
using System;
using System.IO;

namespace TextToSpeech
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "-l")
            {
                var voices = new SpVoice().GetVoices();
                for (var i = 0; i < voices.Count; i++) Console.WriteLine(voices.Item(i).GetAttribute("Name"));
            }
            else
            {
                var pcm = Speak(Console.In.ReadToEnd(), args.Length > 0 ? args[0] : null);
                Console.OpenStandardOutput().Write(pcm, 0, pcm.Length);
            }
        }

        static byte[] Speak(string text, string voiceName)
        {
            var ms = new SpMemoryStream();
            var v = new SpVoice();
            v.Voice = FindVoice(v.GetVoices(), voiceName);
            v.AudioOutputStream = ms;
            v.Speak(text);
            return ms.GetData();
        }

        static SpObjectToken FindVoice(ISpeechObjectTokens voices, string voiceName)
        {
            for (var i = 0; i < voices.Count; i++) if (voices.Item(i).GetAttribute("Name") == voiceName) return voices.Item(i);
            return null;
        }
    }
}
