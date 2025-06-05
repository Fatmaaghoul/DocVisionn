import os
import requests
from dotenv import load_dotenv

load_dotenv()
OLLAMA_HOST = os.getenv("OLLAMA_HOST", "localhost")
OLLAMA_PORT = os.getenv("OLLAMA_PORT", "11434")
OLLAMA_BASE_URL = f"http://{OLLAMA_HOST}:{OLLAMA_PORT}"

def run_model(model_name: str) -> bool:
    try:
        print(f"⏳ Préchargement du modèle {model_name}...")
        response = requests.post(
            f"{OLLAMA_BASE_URL}/api/generate",
            json={
                "model": model_name,
                "prompt": "Hello! This is a warmup request.",
                "stream": False,
                "options": {"num_predict": 1}  # Limiter la génération pour accélérer
            }
        )
        response.raise_for_status()
        result = response.json()
        print(f"✅ Modèle {model_name} préchargé avec succès")
        return True
    except requests.exceptions.RequestException as e:
        print(f"❌ Erreur lors du préchargement du modèle {model_name}: {e}")
        return False


