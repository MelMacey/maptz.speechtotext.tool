# Maptz Speech-to-text tool

This tool is a demo tool using the Bing Speech-to-Text cognitive service from .net core. No official samples exist in C# using the WebSocket protocol, so this is a test implementation showing how to do it.

> This is strictly demo-code only, and should be treated as instructive only. Do not use for production!

## Setup

To setup the environment for conversion, you will need to register for a Bing Speech-to-Text service on Azure. 

You will then need to set the key as a user secret. In command prompt, navigate to the `src` directoyr, and type in the following command: 

	dotnet user-secrets set BingSpeechToTextKey [Bing-SpeechToText-Key]


## Preparing a file for input. 

You will need to send a mono-16 bit 16kHz wav file for conversion. If you have FFMPEG installed and on the PATH, you can convert an audio file using FFMPEG using the following console command: 

	ffmpeg -i [INPUT_FILE_PATH] -acodec pcm_s16le -ac 1 -ar 16000 [OUTPUT_FILE_PATH]

## Running

To run the sample, open a command prompt and navigate to the `src/Maptz.SpeechToText.Tool` directory. Type the following command:

	dotnet run convert -i "[WAV_FILE_PATH]"



