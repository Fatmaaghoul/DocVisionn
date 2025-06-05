import aiohttp
import asyncio
import base64
import os
import json
import hashlib
from typing import List, Dict
from model_manager import ModelManager
import logging
from threading import Lock

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Thread-safe cache
cache = {}
cache_lock = Lock()

def hash_image_and_objects(image_path: str, objects: List[str]) -> str:
    """Génère un hash unique basé sur l'image et les objets."""
    try:
        with open(image_path, 'rb') as f:
            image_bytes = f.read()
        return hashlib.sha256(image_bytes + ','.join(objects).encode()).hexdigest()
    except FileNotFoundError:
        logger.error(f"Échec de la lecture du fichier pour hachage : {image_path}")
        raise

def build_prompt(objects: List[str]) -> str:
    """Génère le prompt pour décrire les objets spécifiés."""
    object_list = ", ".join(objects)
    return f"""
Tu es un assistant expert en vision par ordinateur spécialisé dans la détection et la description d'objets.

---
**Règles strictes :**
- **Décris exclusivement** les objets listés ici : {object_list}.
- Pour chaque objet listé, fournis les informations suivantes :
    - Le **nombre d'occurrences** dans l'image.
    - La **taille relative** de l'objet (par exemple, "petit", "moyen", "grand" ou une proportion si applicable, ex: "occupe 20% de l'image").
    - L'**environnement direct** de l'objet, uniquement s'il est en relation spatiale ou fonctionnelle immédiate avec cet objet.
- **Ignore catégoriquement** et ne mentionne **jamais** tout objet qui n'est pas explicitement inclus dans `{object_list}`.
- Ne produis **aucune** introduction, conclusion, salutation ou tout autre texte périphérique. Commence directement par la description du premier objet.

---
**Format et style de la réponse :**
- Chaque description doit commencer par le **nom exact de l'objet** tel que fourni dans la liste.
- La description doit être **précise, factuelle, concise et entièrement en français**.
- Limite la description de chaque objet à **un maximum de 3 phrases**.
- Adopte un **ton strictement neutre et descriptif**.

---
**Exemple de sortie attendue pour un objet 'voiture' :**
Voiture : trois  voiture rouge sont visible sur la droite. elles sont de taille moyenne. Elles sont stationnée à côté d'un arbre.

---
**Instructions finales :**
Fournis uniquement les descriptions des objets demandés, en respectant le format spécifié.
"""

def format_response(content: str, objects: List[str]) -> str:
    """Formate proprement la réponse du modèle."""
    if not content:
        logger.error("Réponse vide reçue du modèle.")
        raise ValueError("Réponse vide")
    return content.strip()

async def describe_objects(image_path: str, objects: List[str], manager: ModelManager = None, cleanup: bool = True) -> Dict:
    """Génère une description des objets spécifiés dans l'image."""
    if not manager:
        logger.error("Aucun gestionnaire de modèle fourni.")
        return {"status": "error", "message": "Gestionnaire de modèle requis.", "description": None}

    # Vérifier si un modèle est actif
    current_model = manager.get_active_model()
    if not current_model:
        logger.error("Aucun modèle actif défini.")
        return {"status": "error", "message": "Aucun modèle actif défini.", "description": None}

    # Générer la clé de cache
    try:
        cache_key = hash_image_and_objects(image_path, objects)
        with cache_lock:
            if cache_key in cache:
                logger.info(f"Résultat récupéré du cache pour {image_path} avec objets {objects}")
                return {
                    "status": "success",
                    "message": "Résultat récupéré du cache.",
                    "description": cache[cache_key],
                    "model_used": current_model
                }
    except FileNotFoundError:
        logger.error(f"Fichier {image_path} introuvable pour hachage.")
        return {"status": "error", "message": f"Fichier {image_path} introuvable.", "description": None}

    try:
        # Vérifier l'existence du fichier
        if not os.path.exists(image_path):
            logger.error(f"Fichier {image_path} introuvable.")
            return {"status": "error", "message": f"Fichier {image_path} introuvable.", "description": None}

        # Lire et encoder l'image
        with open(image_path, "rb") as image_file:
            image_base64 = base64.b64encode(image_file.read()).decode('utf-8')

        # Construire le prompt
        prompt = build_prompt(objects)
        ollama_url = f"{manager.base_url}/api/generate"

        # Configurer le connecteur pour optimiser les connexions
        async with aiohttp.ClientSession(connector=aiohttp.TCPConnector(limit=10)) as session:
            async with session.post(
                ollama_url,
                json={
                    "model": current_model,
                    "prompt": prompt,
                    "images": [image_base64],
                    "stream": False,
                    "options": {"num_ctx": 2048, "temperature": 0.7}
                },
              
            ) as response:
                if response.status == 200:
                    try:
                        response_data = await response.json()
                        description = format_response(response_data.get("response", ""), objects)
                        with cache_lock:
                            cache[cache_key] = description
                        logger.info(f"Description générée pour {image_path} avec objets {objects}")
                        return {
                            "status": "success",
                            "message": "Description générée avec succès",
                            "description": description,
                            "model_used": current_model
                        }
                    except json.JSONDecodeError as e:
                        logger.error(f"Erreur de décodage JSON : {str(e)}")
                        return {"status": "error", "message": f"Erreur de décodage JSON : {str(e)}", "description": None}
                else:
                    error_text = await response.text()
                    logger.error(f"Erreur API : {response.status} - {error_text}")
                    return {"status": "error", "message": f"Erreur API : {response.status} - {error_text}", "description": None}
    except FileNotFoundError:
        logger.error(f"Fichier {image_path} introuvable.")
        return {"status": "error", "message": f"Fichier {image_path} introuvable.", "description": None}
    except aiohttp.ClientError as e:
        logger.error(f"Erreur de connexion API : {str(e)}")
        return {"status": "error", "message": f"Erreur de connexion API : {str(e)}", "description": None}
    except Exception as e:
        logger.error(f"Exception non gérée : {type(e).__name__} - {str(e)}")
        return {"status": "error", "message": f"Exception non gérée : {type(e).__name__} - {str(e)}", "description": None}
    finally:
        if cleanup and os.path.exists(image_path):
            try:
                os.remove(image_path)
                logger.info(f"Fichier {image_path} supprimé avec succès.")
            except Exception as e:
                logger.warning(f"Échec de la suppression du fichier {image_path}: {str(e)}")