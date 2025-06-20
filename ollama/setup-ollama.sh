#!/bin/bash

# remove /tmp/ollama-ready if it exists
rm -f /tmp/ollama-ready

echo "Starting Ollama server..."
ollama serve &
SERVE_PID=$!

echo "Waiting for Ollama server to be active..."
while ! ollama list | grep -q 'NAME'; do
  sleep 1
done

echo "Pulling Ollama models..."
ollama pull llama3.2:3b

echo "Warming up the model..."
curl -s -N -X POST http://ollama:11434/api/generate -H "Content-Type: application/json" -d '{"model": "llama3.2:3b", "prompt": "Respond with 'hello' and nothing else.", "keep_alive": -1}' >/tmp/model-response.txt

while [ ! -s /tmp/model-response.txt ]; do
  echo "Waiting for model to respond..."
  sleep 1
done

echo "Ollama server is active!"

# indicate that ollama is ready
touch /tmp/ollama-ready

wait $SERVE_PID
