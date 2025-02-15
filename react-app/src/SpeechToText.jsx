import React from "react";
import { ResultReason, SpeechConfig, AudioConfig, SpeechRecognizer } from 'microsoft-cognitiveservices-speech-sdk';

const getRequiredEnvVar = (key) => {
    const value = process.env[key];
    if (!value) {
        return null;
    }
    return value;
};

export default function SpeechToText() {
    async function sttFromMic() {
        
        const speechConfig = SpeechConfig.fromSubscription(
            getRequiredEnvVar("AzureSpeech") ?? "1UFKL0cnbN2D6Fu5ZyLatbJLvpkZmG2DLrqotnVWGkmedKUcbie4JQQJ99AJAC5RqLJXJ3w3AAAYACOG9cwP", 
            getRequiredEnvVar("AzureSpeechRegion") ?? "westeurope"
        );
        console.log("AzureSpeech");
        speechConfig.speechRecognitionLanguage = 'en-US';

        const audioConfig = AudioConfig.fromDefaultMicrophoneInput();
        const recognizer = new SpeechRecognizer(speechConfig, audioConfig);

        recognizer.recognizeOnceAsync(result => {
                const item = document.getElementById("query-field");
                if (item) {
                    result.text != undefined
                    ? item.value = result.text.replace(".", "")
                    : "";
                }
            }, 
        (err) => {
                console.error(`Error recognizing speech: ${err}`);
        });
    }

    const buttonStyle = {
        '--button-hover-background': 'var(--example-color-alt)',
        '--button-color': 'var(--white)',
        '--button-background': 'var(--example-color-alt)',
        '--button-margin-bottom': '0'
    };

    return (
        <button class="flat" onClick={() => sttFromMic()} style={buttonStyle}>
            <i class="fas fa-microphone fa-fw fa-xl"></i>
        </button>
    );
}
