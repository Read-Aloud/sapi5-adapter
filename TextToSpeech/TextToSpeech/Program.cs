using SpeechLib;
using System;
using System.Diagnostics;
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
                new SpeechEngine("ffmpeg.exe").Speak(Console.In.ReadToEnd(), args.Length > 0 ? args[0] : null).CopyTo(Console.OpenStandardOutput());
            }
        }
    }

    class SpeechEngine
    {
        readonly ProcessStartInfo ffmpegOpts;

        public SpeechEngine(string ffmpegPath)
        {
            ffmpegOpts = new ProcessStartInfo
            {
                Arguments = "-f s16le -ar 22050 -ac 1 -i - -f mp3 -",
                CreateNoWindow = true,
                FileName = ffmpegPath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };
        }

        public Stream Speak(string text, string voiceName)
        {
            var ms = new SpMemoryStream();
            var v = new SpVoice();
            v.Voice = FindVoice(v.GetVoices(), voiceName);
            v.AudioOutputStream = ms;
            v.Speak(text);
            byte[] pcm = ms.GetData();
            var p = Process.Start(ffmpegOpts);
            p.StandardInput.BaseStream.WriteAsync(pcm, 0, pcm.Length).ContinueWith(t => p.StandardInput.Close());
            return p.StandardOutput.BaseStream;
        }

        SpObjectToken FindVoice(ISpeechObjectTokens voices, string voiceName)
        {
            for (var i = 0; i < voices.Count; i++) if (voices.Item(i).GetAttribute("Name") == voiceName) return voices.Item(i);
            return null;
        }
    }
}
