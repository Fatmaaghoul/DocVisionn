import os
import requests
import torch

OLLAMA_HOST = os.getenv("OLLAMA_HOST", "ollama")
OLLAMA_PORT = os.getenv("OLLAMA_PORT", "11434")
OLLAMA_BASE_URL = f"http://{OLLAMA_HOST}:{OLLAMA_PORT}"

# Initialize a persistent session
session = requests.Session()

def resumer(text: str = "") -> str:
    if not text.strip():
        return "Aucun texte fourni pour le résumé."
    
    prompt = f"""
    Tu es un assistant qui doit résumer un texte. Fournis un résumé **clair, concis et informatif** en **10% de text original** par des conjonctions.
    Texte à résumer :
    {text}
[important]
 - la description doit etre en francais
 - la description doit etre courte
 - ni introduction ni conclusion
    """

    payload = {
        "model": "gemma3:4b",
        "messages": [{"role": "user", "content": prompt}],
        "stream": False,
        "options": {"num_ctx": 2048, "temperature": 0.7}
    }

    try:
        print("[INFO] Envoi de la requête à Ollama (modèle : gemma3:4b)...")
        response = session.post(
            f"{OLLAMA_BASE_URL}/api/chat",
            json=payload,
            headers={"Content-Type": "application/json"},
            # timeout=300
        )

        if response.status_code != 200:
            raise Exception(f"Erreur Ollama (status {response.status_code}) : {response.text}")

        result = response.json()
        if 'message' in result and 'content' in result['message']:
            print("[INFO] Résumé généré avec succès.")
            return result['message']['content']
        else:
            raise Exception("Format de réponse inattendu d'Ollama.")
    except Exception as e:
        print(f"[ERROR] Erreur lors de la génération du résumé : {e}")
        return f"Erreur lors de la génération du résumé : {e}"