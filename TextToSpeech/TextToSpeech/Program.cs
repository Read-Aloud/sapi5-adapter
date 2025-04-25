using System;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;
using System.Text;

namespace TextToSpeech
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;

            try
            {
                while (true)
                {
                    // Read message from stdin following the Chrome native messaging protocol
                    string json = ReadMessageFromStdin();
                    if (string.IsNullOrEmpty(json)) break;

                    var req = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                    string response;

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
                            response = Newtonsoft.Json.JsonConvert.SerializeObject(new { voices });
                            break;

                        case "synthesize":
                            var audioUrl = Synthesize(req.text.ToString(), req.voice.ToString());
                            response = Newtonsoft.Json.JsonConvert.SerializeObject(new { audioUrl });
                            break;

                        default:
                            response = Newtonsoft.Json.JsonConvert.SerializeObject(new { error = "Invalid request type" });
                            break;
                    }

                    // Write response to stdout following the Chrome native messaging protocol
                    WriteMessageToStdout(response);
                }
            }
            catch (Exception ex)
            {
                string errorResponse = Newtonsoft.Json.JsonConvert.SerializeObject(new { error = ex.Message });
                WriteMessageToStdout(errorResponse);
            }
        }

        static string ReadMessageFromStdin()
        {
            // Read the 4-byte length prefix
            byte[] lengthBytes = new byte[4];
            int bytesRead = Console.OpenStandardInput().Read(lengthBytes, 0, 4);
            if (bytesRead < 4) return null;

            // Convert the length prefix to an integer (little-endian)
            int messageLength = BitConverter.ToInt32(lengthBytes, 0);

            // Read the JSON message of the specified length
            byte[] messageBytes = new byte[messageLength];
            bytesRead = Console.OpenStandardInput().Read(messageBytes, 0, messageLength);
            if (bytesRead < messageLength) return null;

            // Convert the message bytes to a string
            return Encoding.UTF8.GetString(messageBytes);
        }

        static void WriteMessageToStdout(string message)
        {
            // Convert the message to bytes
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // Write the 4-byte length prefix (little-endian)
            byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
            Console.OpenStandardOutput().Write(lengthBytes, 0, 4);

            // Write the message bytes
            Console.OpenStandardOutput().Write(messageBytes, 0, messageBytes.Length);
            Console.OpenStandardOutput().Flush();
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
