# SAPI5 Adapter
A command-line .NET program to synthesize speech using Microsoft SAPI5 voices

## API
List available voices
```
function stdio(stdin: {
  "type":"listVoices"
}): Array<{
  "Name": string
  "Language": string
  "Gender": string
}>
```

Synthesize speech
```
function stdio(stdin: {
  "type": "synthesize"
  "text": string
  "voice": string
  "pitch": number
}): {
  audioUrl: string   //mp3 data URL
} | {
  error: string
}
```
