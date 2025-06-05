import os
import aiohttp
import json
from typing import List, Literal
from pydantic import BaseModel

# Configuration d'Ollama avec les variables d'environnement
OLLAMA_HOST = os.getenv("OLLAMA_HOST", "ollama")
OLLAMA_PORT = os.getenv("OLLAMA_PORT", "11434")
OLLAMA_BASE_URL = f"http://{OLLAMA_HOST}:{OLLAMA_PORT}"

# Modèle de donnée retourné
class ModelInfo(BaseModel):
    name: str
    value: str  # Utilisation de str au lieu de ModelType
    type: Literal['vision', 'text']
    description: str

# Modèle actuellement utilisé (modifiable dynamiquement)
DEFAULT_MODEL = "gemma3:4b"
current_model = DEFAULT_MODEL

async def list_ollama_models() -> List[ModelInfo]:
    """Liste dynamique des modèles depuis Ollama avec type et description"""
    try:
        async with aiohttp.ClientSession() as session:
            async with session.get(f"{OLLAMA_BASE_URL}/api/tags") as response:
                if response.status != 200:
                    print(f"[ERROR] Erreur lors de la récupération des modèles Ollama: {response.status}")
                    return []
                
                data = await response.json()
                result = []

                for model in data.get('models', []):
                    name = model.get('name', '')
                    model_type = 'vision' if any(keyword in name.lower() for keyword in ['llava', 'gemma', 'vision']) else 'text'

                    result.append(ModelInfo(
                        name=name,
                        value=name,
                        type=model_type,
                        description=f"Modèle détecté automatiquement : {name}"
                    ))

                return result
    except Exception as e:
        print(f"[ERROR] Exception dans list_ollama_models: {str(e)}")
        return []

async def get_model_info(model_value: str) -> ModelInfo:
    """Retourne les infos détaillées pour un modèle donné"""
    models = await list_ollama_models()
    for model in models:
        if model.value == model_value:
            return model
    raise ValueError(f"Modèle {model_value} non trouvé dans Ollama.")

def get_current_model() -> str:
    """Retourne le modèle actuellement sélectionné"""
    return current_model

async def set_current_model(model_value: str) -> bool:
    """Met à jour le modèle utilisé si celui-ci existe dans Ollama"""
    global current_model
    models = await list_ollama_models()
    if any(m.value == model_value for m in models):
        current_model = model_value
        return True
    return False

async def ensure_model_available(model_name: str):
    """Télécharge un modèle si nécessaire"""
    import requests

    try:
        # Vérifier les modèles disponibles
        response = requests.get(f"{OLLAMA_BASE_URL}/api/tags")
        if response.status_code != 200:
            print(f"[ERROR] Erreur lors de la récupération des modèles: {response.status_code}")
            return
        
        data = response.json()
        model_names = [model.get('name', '') for model in data.get('models', [])]

        if model_name not in model_names:
            print(f"[INFO] Installation de {model_name}...")
            pull_response = requests.post(
                f"{OLLAMA_BASE_URL}/api/pull",
                json={"name": model_name}
            )
            if pull_response.status_code == 200:
                print(f"[SUCCESS] {model_name} installé avec succès!")
            else:
                print(f"[ERROR] Erreur lors de l'installation de {model_name}: {pull_response.status_code}")
        else:
            print(f"[INFO] {model_name} déjà installé")
    except Exception as e:
        print(f"[ERROR] Exception dans ensure_model_available: {str(e)}")

async def ensure_default_models():
    """Assure que les modèles par défaut sont installés"""
    await ensure_model_available("gemma3:4b")
    await ensure_model_available("llava:7b")
