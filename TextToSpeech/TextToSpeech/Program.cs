using System;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;

namespace TextToSpeech
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string json = ReadAllInput();
            var req = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
            switch (req.type.ToString())
            {
                case "listVoices":
                    var voices = new SpeechSynthesizer().GetInstalledVoices()
                        .Select(v => new
                        {
                            v.VoiceInfo.Name,
                            Gender = v.VoiceInfo.Gender.ToString(),
                            Language = v.VoiceInfo.Culture?.Name ?? "xx"
                        })
                        .ToArray();
                    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(voices));
                    break;

                case "synthesize":
                    var audioUrl = Synthesize(req.text.ToString(), req.voice.ToString());
                    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(new { audioUrl }));
                    break;
            }
        }

        static string ReadAllInput()
        {
            using (var reader = new StreamReader(Console.OpenStandardInput(), System.Text.Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        static string Synthesize(string text, string voiceName)
        {
            using (var synthesizer = new SpeechSynthesizer())
            using (var pcmStream = new MemoryStream())
            {
                // Configure the SpeechSynthesizer
                try
                {
                    synthesizer.SelectVoice(voiceName);
                    if (voiceName.StartsWith("Vocalizer")) synthesizer.Volume = 50;
                }
                catch
                {
                    // Handle voice selection errors gracefully
                }

                // Set output to PCM stream
                synthesizer.SetOutputToAudioStream(
                    pcmStream,
                    new SpeechAudioFormatInfo(22050, AudioBitsPerSample.Sixteen, AudioChannel.Mono)
                );

                // Speak the text
                if (text.StartsWith("<speak"))
                    synthesizer.SpeakSsml(text);
                else
                    synthesizer.Speak(text);

                // Reset the stream position to the beginning
                pcmStream.Position = 0;

                // Convert PCM to MP3
                using (var mp3Stream = new MemoryStream())
                {
                    using (var reader = new NAudio.Wave.RawSourceWaveStream(
                        pcmStream,
                        new NAudio.Wave.WaveFormat(22050, 16, 1)))
                    using (var writer = new NAudio.Lame.LameMP3FileWriter(
                        mp3Stream,
                        reader.WaveFormat,
                        NAudio.Lame.LAMEPreset.VBR_90))
                    {
                        reader.CopyTo(writer);
                    }

                    // Reset the MP3 stream position to the beginning
                    mp3Stream.Position = 0;

                    // Convert MP3 to base64
                    var base64Audio = Convert.ToBase64String(mp3Stream.ToArray());

                    // Return as audio data URL
                    return $"data:audio/mp3;base64,{base64Audio}";
                }
            }
        }
    }
}
