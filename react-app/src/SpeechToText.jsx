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
                    item.value = result.text.replace(".", "") 
                    ? result.text 
                    != undefined : "";
                }
            }, 
        (err) => {
                console.error(`Error recognizing speech: ${err}`);
        });
    }
    
    return (
      <i className="fas fa-microphone fa-lg mr-2" onClick={() => sttFromMic()}>
      </i>  
    );
}
